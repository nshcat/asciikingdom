using System;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using Engine.Core;
using Engine.Graphics;
using Engine.Input;
using Engine.Rendering;
using Engine.Resources;
using Game.Core;
using Game.Simulation;
using Game.Data;
using Game.Ui;
using Game.WorldGen;
using OpenToolkit.Graphics.OpenGL;
using OpenToolkit.Windowing.Common.Input;

namespace Game.Scenes
{
    /// <summary>
    /// The main game scene
    /// </summary>
    public partial class GameScene : Scene
    {
        /// <summary>
        /// Represents the various simulation speeds
        /// </summary>
        private enum GameSpeed
        {
            Paused = 0,
            Slow,
            Normal,
            Fast
        }

        /// <summary>
        /// The input actions the user can use in the scene
        /// </summary>
        private enum GameAction
        {
            None = 0,
            MoveUp,
            MoveDown,
            MoveRight,
            MoveLeft,
            MoveUpFast,
            MoveDownFast,
            MoveRightFast,
            MoveLeftFast,
            RegenerateMap,
            ShowResources,
            SaveAndQuit,
            PlaceCity,
            PlaceVillage,
            ToggleNewProvince,
            ToggleMapLabels,
            Return,
            Select,
            GenerateTestData,
            IncreaseGameSpeed,
            DecreaseGameSpeed
        }

        /// <summary>
        /// The various UI states the game scene can be in
        /// </summary>
        private enum GameUiState
        {
            /// <summary>
            /// No sub UI active, just the main screen.
            /// </summary>
            Main = 0,
            
            /// <summary>
            /// Placing a city or province capital
            /// </summary>
            PlaceCity,
            
            /// <summary>
            /// Placing a village
            /// </summary>
            PlaceVillage,
            
            /// <summary>
            /// Naming the site that is currently being placed
            /// </summary>
            NamePlacement
        }

        /// <summary>
        /// Enum representing which type of site is in the progress of being placed
        /// </summary>
        private enum PlacementType
        {
            None,
            Village,
            City
        }
        
        /// <summary>
        /// The current simulation state
        /// </summary>
        private SimulationState _state;

        /// <summary>
        /// The current UI state
        /// </summary>
        private GameUiState _uiState = GameUiState.Main;
        
        /// <summary>
        /// The surface used to draw the game views. Is supposed to have a tile set that is well-suited
        /// for displaying the world tiles.
        /// </summary>
        private Surface _surface;
        
        /// <summary>
        /// The input action mapper for this scene
        /// </summary>
        private InputActionMapper<GameAction> _actionMapper;
        
        /// <summary>
        /// World terrain view
        /// </summary>
        private MapView _terrainView;

        /// <summary>
        /// Elapsed seconds that weren't used up for weeks
        /// </summary>
        private double _elapsedTimeBuffer;
        
        /// <summary>
        /// World site view
        /// </summary>
        private SiteView _siteView;

        /// <summary>
        /// Whether placing a new city should also create a new province
        /// </summary>
        private bool _newProvince = false;

        /// <summary>
        /// The current game speed
        /// </summary>
        private GameSpeed _gameSpeed = GameSpeed.Paused;
        
        /// <summary>
        /// Window used to prompt the user for a site name after placement
        /// </summary>
        private TextInputWindow _placementNameWindow = new TextInputWindow("Enter Name", 0.25f);

        /// <summary>
        /// The type of the site that is currently being placed
        /// </summary>
        private PlacementType _currentPlacement = PlacementType.None;

        /// <summary>
        /// Position of the site that is currently being placed
        /// </summary>
        private Position _placementPosition;

        /// <summary>
        /// Whether the placement can actually be placed at the current cursor position
        /// </summary>
        /// <remarks>
        /// This is updated by <see cref="UpdateCursorMode"/>
        /// </remarks>
        private bool _canPlace = false;
        
        /// <summary>
        /// City in whose influence radius the cursor is currently in, if any
        /// </summary>
        private Optional<City> _currentCity = Optional<City>.Empty;

        /// <summary>
        /// Province in which the cursor currently is in
        /// </summary>
        private Optional<Province> _currentProvince = Optional<Province>.Empty;
        
        /// <summary>
        /// The world site the cursor is currently on, if any
        /// </summary>
        private Optional<IWorldSite> _cursorSite = Optional<IWorldSite>.Empty;

        /// <summary>
        /// The world terrain currently under the cursor
        /// </summary>
        private TerrainTypeInfo _cursorTerrainInfo;

        /// <summary>
        /// The world terrain type currently under the cursor
        /// </summary>
        private TerrainType _cursorTerrainType;
        
        /// <summary>
        /// Whether there are any provinces in the current state
        /// </summary>
        private bool HasAnyProvinces() => this._state.Provinces.Count > 0;
        
        /// <summary>
        /// The top left corner of the game menu
        /// </summary>
        private Position MenuTopLeft => new Position(
            this._terrainView.Position.X + this._terrainView.Dimensions.Width + 2,
            2
        );
        
        /// <summary>
        /// Create new game scene based on given simulation state 
        /// </summary>
        public GameScene(Scene parent, SimulationState state)
            : base(parent)
        {
            this.Initialize(state);
        }

        /// <summary>
        /// Create new game scene based on given simulation state 
        /// </summary>
        public GameScene(SceneStack sceneStack, InputManager inputManager, ResourceManager resourceManager, SimulationState state)
            : base(sceneStack, inputManager, resourceManager)
        {
            this.Initialize(state);
        }
        
        /// <summary>
        /// Determine the number of weeks elapsing per second based on given game speed setting
        /// </summary>
        private int WeeksPerSecond(GameSpeed speed)
        {
            return speed switch
            {
                GameSpeed.Paused => 0,
                GameSpeed.Slow => 1,
                GameSpeed.Normal => 4,
                GameSpeed.Fast => 12,
            };
        }

        /// <summary>
        /// Pause the game
        /// </summary>
        private void PauseGame()
        {
            this._gameSpeed = GameSpeed.Paused;
        }
        
        /// <summary>
        /// Initialize the game views that are part of this scene
        /// </summary>
        private void InitializeViews()
        {
            this._terrainView = new MapView(new Position(1, 1), Size.Empty) { CursorMode = CursorMode.Disabled };
            this._siteView = new SiteView(new Position(1, 1), Size.Empty);
            
            this._terrainView.ReplaceMap(this._state.World.DetailedMap);
            this._siteView.ReplaceState(this._state);

            this._terrainView.CursorMoved += (newPosition) =>
            {
                this._siteView.CursorPosition = newPosition;
                
                this._siteView.RecalulatePositions();
                
                // Detect if the cursor is currently on a site
                var sites = this._state.GetAllSites();

                if (sites.ContainsKey(newPosition))
                {
                    this._cursorSite = Optional<IWorldSite>.Of(sites[newPosition]);
                }
                else
                {
                    this._cursorSite.Reset();
                }
            };
        }

        /// <summary>
        /// Calculate how many weeks elapsed depending on the game speed
        /// </summary>
        private int CalculateElapsedWeeks(double seconds)
        {
            var total = this._elapsedTimeBuffer + seconds;
            var elapsedSeconds = Math.Truncate(total);
            this._elapsedTimeBuffer = total - elapsedSeconds;

            return this.WeeksPerSecond(this._gameSpeed) * (int) elapsedSeconds;
        }

        /// <summary>
        /// Initialize the scene
        /// </summary>
        private void Initialize(SimulationState state)
        {
            this._state = state;
            this.InitializeViews();
            this.InitializeMapper();
            this.InitializeMenu();
        }
        
        /// <summary>
        /// Initialize the input mapper
        /// </summary>
        private void InitializeMapper()
        {
            this._actionMapper = new InputActionMapper<GameAction>(this.Input,
                new InputAction<GameAction>(GameAction.MoveUp, KeyPressType.Down, Key.Up),
                new InputAction<GameAction>(GameAction.MoveDown, KeyPressType.Down, Key.Down),
                new InputAction<GameAction>(GameAction.MoveLeft, KeyPressType.Down, Key.Left),
                new InputAction<GameAction>(GameAction.MoveRight, KeyPressType.Down, Key.Right),
                new InputAction<GameAction>(GameAction.MoveUpFast, KeyPressType.Down, Key.Up, Key.ShiftLeft),
                new InputAction<GameAction>(GameAction.MoveDownFast, KeyPressType.Down, Key.Down, Key.ShiftLeft),
                new InputAction<GameAction>(GameAction.MoveLeftFast, KeyPressType.Down, Key.Left, Key.ShiftLeft),
                new InputAction<GameAction>(GameAction.MoveRightFast, KeyPressType.Down, Key.Right, Key.ShiftLeft),
                new InputAction<GameAction>(GameAction.ShowResources, KeyPressType.Down, Key.R, Key.ShiftLeft),
                new InputAction<GameAction>(GameAction.ToggleMapLabels, KeyPressType.Down, Key.L),
                new InputAction<GameAction>(GameAction.SaveAndQuit, KeyPressType.Down, Key.Q, Key.ShiftLeft),
                new InputAction<GameAction>(GameAction.PlaceCity, KeyPressType.Down, Key.C, Key.ShiftLeft),
                new InputAction<GameAction>(GameAction.PlaceVillage, KeyPressType.Down, Key.V, Key.ShiftLeft),
                new InputAction<GameAction>(GameAction.ToggleNewProvince, KeyPressType.Down, Key.P),
                new InputAction<GameAction>(GameAction.Return, KeyPressType.Down, Key.Escape),
                new InputAction<GameAction>(GameAction.Select, KeyPressType.Down, Key.Enter),
                new InputAction<GameAction>(GameAction.GenerateTestData, KeyPressType.Down, Key.T, Key.ShiftLeft),
                new InputAction<GameAction>(GameAction.IncreaseGameSpeed, KeyPressType.Pressed, Key.KeypadPlus),
                new InputAction<GameAction>(GameAction.DecreaseGameSpeed, KeyPressType.Pressed, Key.KeypadMinus)
            );
        }

        /// <summary>
        /// Draw the game views
        /// </summary>
        private void DrawViews()
        {
            this._terrainView.Render(this._surface);
            this._siteView.Render(this._surface);
        }

        /// <summary>
        /// Update the text input window with user text input, if needed
        /// </summary>
        private void UpdateTextInputWindow()
        {
            if(this._uiState == GameUiState.NamePlacement)
                this._placementNameWindow.Update(this.Input);
        }
        
        /// <summary>
        /// Draw the game action menu
        /// </summary>
        private void DrawMenu()
        {
            var position = this.MenuTopLeft;

            foreach (var entry in this._gameMenu)
                position = entry.Render(this._surface, this._uiState, position);
        }

        /// <summary>
        /// Update the cursor display mode of the site view based on the current state and cursor position
        /// </summary>
        private void UpdateCursorMode()
        {
            switch (this._uiState)
            {
                case GameUiState.PlaceCity:
                {
                    var invalid =
                        (this._newProvince && this._currentProvince.HasValue)
                        || !TerrainTypeData.AcceptsSites(this._cursorTerrainType)
                        || !this._newProvince && (!this._currentProvince.HasValue || !this._currentProvince.Value.CanSupportNewCity)
                        || this._state.GetAllSites().ContainsKey(this._siteView.CursorPosition);

                    this._siteView.CursorMode = invalid ? CursorMode.Invalid : CursorMode.Normal;
                    this._canPlace = !invalid;
                        
                    break;
                }
                case GameUiState.PlaceVillage:
                {
                    var invalid = !this._currentCity.HasValue 
                                  || !this._currentCity.Value.CanSupportNewVillage 
                                  || !TerrainTypeData.AcceptsSites(this._cursorTerrainType)
                                  || this._state.GetAllSites().ContainsKey(this._siteView.CursorPosition);
                    
                    this._siteView.CursorMode = invalid ? CursorMode.Invalid : CursorMode.Normal;
                    this._canPlace = !invalid;
                    
                    break;
                }
                default:
                    this._siteView.CursorMode = CursorMode.Normal;
                    this._canPlace = false;
                    break;
            }
        }

        /// <summary>
        /// Modify the current game speed
        /// </summary>
        private void ModifyGameSpeed(int direction)
        {
            var val = (int) this._gameSpeed;
            val += direction;

            if (val < 0)
                val = 0;

            if (val > 3)
                val = 3;

            this._gameSpeed = (GameSpeed) val;
        }
        
        /// <summary>
        /// Draw the decorative borders around the game view and menu
        /// </summary>
        private void DrawBorders()
        {
            var menuBounds = new Rectangle(
                new Position(this._terrainView.Position.X + this._terrainView.Dimensions.Width, 0),
                (Position)this._surface.Dimensions - new Position(1, 1)
            );
            
            this._surface.DrawRectangle(menuBounds, 219, UiColors.BorderBack, DefaultColors.Black);
            this._surface.DrawRectangle(new Rectangle(this._surface.Dimensions), 219, UiColors.BorderBack, DefaultColors.Black);
            this._surface.DrawStringCentered(
                new Position(this._surface.Dimensions.Width / 2, 0),
                "  Ascii Kingdom  ",
                UiColors.BorderTitle,
                UiColors.BorderBack
            );

            var dateString = this._state.Date.ToString();
            this._surface.DrawString(
                new Position(this._surface.Dimensions.Width - dateString.Length - 2, 0),
                dateString,
                UiColors.BorderTitle,
                UiColors.BorderBack
            );

            var speedStr = $"Speed: {this._gameSpeed.ToString()}";
            if (this._gameSpeed == GameSpeed.Paused)
                speedStr = "*PAUSED*";
            
            this._surface.DrawString(
                new Position(2, 0),
                speedStr,
                UiColors.BorderTitle,
                (this._gameSpeed == GameSpeed.Paused) ? Color.FromHex("#238300") : UiColors.BorderBack
            );
        }
        
        /// <summary>
        /// Draw information about the current tile
        /// </summary>
        private void DrawTileInfo()
        {
            var position = this.MenuTopLeft + new Position(0, (int)(this._surface.Dimensions.Height * 0.75f));

            if (this._currentProvince.HasValue)
            {
                this._surface.DrawString(position, $"Province: {this._currentProvince.Value.Name}",
                    UiColors.ActiveText, DefaultColors.Black);
                
                position += new Position(0, 1);
            }

            if (this._currentCity.HasValue && !(this._cursorSite.HasValue && this._cursorSite.Value is City))
            {
                this._surface.DrawString(position, $"Near city: {this._currentCity.Value.Name}",
                    UiColors.ActiveText, DefaultColors.Black);
                
                position += new Position(0, 1);
            }

            if (this._cursorSite.HasValue)
            {
                this._surface.DrawString(position, $"{this._cursorSite.Value.TypeDescriptor}: {this._cursorSite.Value.Name}",
                    UiColors.ActiveText, DefaultColors.Black);
                
                position += new Position(0, 1);
            }
            
            this._surface.DrawString(position, TerrainTypeData.GetInfo(this._state.World.DetailedMap.GetTerrainType(this._terrainView.CursorPosition)).Name,
                UiColors.ActiveText, DefaultColors.Black);

            if (this._terrainView.ShowResources &&
                this._state.World.DetailedMap.Resources.ContainsKey(this._terrainView.CursorPosition))
            {
                var resourceType = this._state.World.DetailedMap.Resources[this._terrainView.CursorPosition];
                this._surface.DrawString(position + new Position(0, 1), resourceType.DisplayName,
                    UiColors.ActiveText, DefaultColors.Black);
            }
        }

        /// <summary>
        /// Handle game view cursor movement input actions
        /// </summary>
        private void HandleCursorMove(MovementDirection direction, int amount = 1)
        {
            if (this._uiState == GameUiState.Main
                || this._uiState == GameUiState.PlaceCity
                || this._uiState == GameUiState.PlaceVillage)
            {
                this._terrainView.MoveCursor(direction, amount);
            }
        }
        
        /// <summary>
        /// Handle user input actions
        /// </summary>
        private void HandleInput(GameAction action)
        {
            switch (action)
            {
                case GameAction.MoveDown:
                {
                    this.HandleCursorMove(MovementDirection.Down);
                    break;
                }
                case GameAction.MoveUp:
                {
                    this.HandleCursorMove(MovementDirection.Up);
                    break;
                }
                case GameAction.MoveLeft:
                {
                    this.HandleCursorMove(MovementDirection.Left);
                    break;
                }
                case GameAction.MoveRight:
                {
                    this.HandleCursorMove(MovementDirection.Right);
                    break;
                }
                case GameAction.MoveDownFast:
                {
                    this.HandleCursorMove(MovementDirection.Down, 5);
                    break;
                }
                case GameAction.MoveUpFast:
                {
                    this.HandleCursorMove(MovementDirection.Up, 5);
                    break;
                }
                case GameAction.MoveLeftFast:
                {
                    this.HandleCursorMove(MovementDirection.Left, 5);
                    break;
                }
                case GameAction.MoveRightFast:
                {
                    this.HandleCursorMove(MovementDirection.Right, 5);
                    break;
                }
                case GameAction.ShowResources:
                {
                    if(this._uiState == GameUiState.Main)
                        this._terrainView.ShowResources = !this._terrainView.ShowResources;
                    
                    break;
                }
                case GameAction.ToggleMapLabels:
                {
                    if(this._uiState == GameUiState.Main)
                        this._siteView.DrawMapLabels = !this._siteView.DrawMapLabels;
                    
                    break;
                }
                case GameAction.SaveAndQuit:
                {
                    if (this._uiState == GameUiState.Main)
                    {
                        // Save the world
                        WorldManager.Instance.SaveWorld(this._state);
                        
                        // Exit to main menu
                        this.SceneStack.NextOperation = new SceneStackOperation.PopScene();
                    }

                    break;
                }
                case GameAction.PlaceCity:
                {
                    if (this._uiState == GameUiState.Main)
                    {
                        this.PauseGame();
                        this._uiState = GameUiState.PlaceCity;
                        this._newProvince = !this.HasAnyProvinces();
                        this._siteView.InfluenceMode = (this._newProvince 
                            ? SiteView.InfluenceDrawMode.InverseProvince 
                            : SiteView.InfluenceDrawMode.Province);
                    }
                    else if (this._uiState == GameUiState.PlaceCity)
                    {
                        this._uiState = GameUiState.Main;
                        this._siteView.InfluenceMode = SiteView.InfluenceDrawMode.None;
                        this._terrainView.CursorMode = CursorMode.Normal;
                    }

                    break;
                }
                case GameAction.PlaceVillage:
                {
                    if (this._uiState == GameUiState.Main)
                    {
                        this.PauseGame();
                        this._uiState = GameUiState.PlaceVillage;
                        this._siteView.InfluenceMode = SiteView.InfluenceDrawMode.City;
                    }
                    else if (this._uiState == GameUiState.PlaceVillage)
                    {
                        this._siteView.InfluenceMode = SiteView.InfluenceDrawMode.None;
                        this._terrainView.CursorMode = CursorMode.Normal;
                        this._uiState = GameUiState.Main;
                    }

                    break;
                }
                case GameAction.ToggleNewProvince:
                {
                    if (this._uiState == GameUiState.PlaceCity && this.HasAnyProvinces())
                    {
                        this._newProvince = !this._newProvince;
                        this._siteView.InfluenceMode = this._newProvince
                            ? SiteView.InfluenceDrawMode.InverseProvince
                            : SiteView.InfluenceDrawMode.Province;
                    }

                    break;
                }
                case GameAction.Return:
                {
                    if (this._uiState == GameUiState.PlaceCity
                        || this._uiState == GameUiState.PlaceVillage
                        || this._uiState == GameUiState.NamePlacement)
                    {
                        this._siteView.InfluenceMode = SiteView.InfluenceDrawMode.None;
                        this._terrainView.CursorMode = CursorMode.Normal;
                        this._uiState = GameUiState.Main;
                    }

                    break;
                }
                case GameAction.Select:
                {
                    if (this._uiState == GameUiState.PlaceCity || this._uiState == GameUiState.PlaceVillage)
                    {
                        if (this._canPlace)
                        {
                            this._placementPosition = this._terrainView.CursorPosition;
                            this._currentPlacement = (this._uiState == GameUiState.PlaceCity)
                                ? PlacementType.City
                                : PlacementType.Village;
                            this._uiState = GameUiState.NamePlacement;
                            this._placementNameWindow.Begin();
                            this._siteView.InfluenceMode = SiteView.InfluenceDrawMode.None;
                            this._terrainView.CursorMode = CursorMode.Normal;
                        }
                    }
                    else if (this._uiState == GameUiState.NamePlacement &&
                             !string.IsNullOrEmpty(this._placementNameWindow.Text))
                    {
                        this.PlaceSite();
                        this._uiState = GameUiState.Main;
                    }

                    break;
                }
                case GameAction.GenerateTestData:
                {
                    if (this._uiState == GameUiState.Main)
                        this.GenerateTestData();
                        
                    break;
                }
                case GameAction.IncreaseGameSpeed:
                {
                    this.ModifyGameSpeed(1);
                    break;
                }
                case GameAction.DecreaseGameSpeed:
                {
                    this.ModifyGameSpeed(-1);
                    break;
                }
            }
        }
        
        /// <summary>
        /// Render the scene
        /// </summary>
        public override void Render(RenderParams rp)
        {
            this._surface.Clear();
            this.DrawViews();
            this.DrawTileInfo();
            this.DrawMenu();
            this.DrawBorders();
            this.DrawNameSiteWindow();
            
            this._surface.Render(rp);
        }

        /// <summary>
        /// Finalize size placement
        /// </summary>
        private void PlaceSite()
        {
            switch (this._currentPlacement)
            {
                case PlacementType.Village:
                {
                    var city = this._currentCity.Value;
                    var village = new Village(this._placementNameWindow.Text, this._placementPosition, 10, city);
                    city.AssociatedVillages.Add(village);
                    break;
                }
                case PlacementType.City:
                {
                    var city = new City(this._placementNameWindow.Text, this._placementPosition, 50);

                    if (this._newProvince)
                    {
                        var province = new Province("", city);
                        city.AssociatedProvince = province;
                        this._state.Provinces.Add(province);
                    }
                    else
                    {
                        var province = this._currentProvince.Value;
                        city.AssociatedProvince = province;
                        province.AssociatedCities.Add(city);
                    }
                    break;
                }
                default:
                    return;
            }
        }
        
        /// <summary>
        /// Render the window used to name newly placed sites, if needed
        /// </summary>
        private void DrawNameSiteWindow()
        {
            if(this._uiState == GameUiState.NamePlacement)
                this._placementNameWindow.Render(this._surface);
        }

        /// <summary>
        /// Update state
        /// </summary>
        public override void Update(double deltaTime)
        {
            this._actionMapper.Update();
            
            if (this._actionMapper.HasTriggeredAction)
            {
                this.HandleInput(this._actionMapper.TriggeredAction);
            }

            this.UpdateTextInputWindow();
            this._terrainView.Update(deltaTime);
            this._siteView.Update(deltaTime);
            
            // Check if cursor is in any of the cities or provinces influence radii
            this.DoCursorHitTest();

            this.UpdateCursorMode();
            
            this._state.Update(this.CalculateElapsedWeeks(deltaTime));
        }

        /// <summary>
        /// Determine in which cities and provinces the current cursor is inside of
        /// </summary>
        private void DoCursorHitTest()
        {
            var cities = this._state.GetAllSites().Where(x => x.Value is City).Select(x => x.Value as City);
            
            // City region
            var cityResult = cities.FirstOrDefault(x => x.InfluenceCircle.ContainsPoint(this._terrainView.CursorPosition));
            this._currentCity = Optional<City>.SafeOf(cityResult);
            
            // Province region
            var provinceResult = cities
                .Where(x => x.IsProvinceCapital)
                .Select(x => x.AssociatedProvince)
                .FirstOrDefault(x => x.InfluenceCircle.ContainsPoint(this._terrainView.CursorPosition));
            this._currentProvince = Optional<Province>.SafeOf(provinceResult);
            
            // Terrain
            this._cursorTerrainInfo = this._state.World.DetailedMap.GetTerrainInfo(this._siteView.CursorPosition);
            this._cursorTerrainType = this._state.World.DetailedMap
                .Terrain[this._siteView.CursorPosition.X, this._siteView.CursorPosition.Y];

        }

        /// <summary>
        /// Generate test data
        /// </summary>
        private void GenerateTestData()
        {
            if (this.HasAnyProvinces())
                return;
            
            var city = new City("Weymouth", new Position(162, 111), 65000);
            var village1 = new Village("", new Position(160, 108), 103, city);
            var village2 = new Village("", new Position(157, 113), 46, city);
            var village3 = new Village("", new Position(158, 109), 16, city);
            city.AssociatedVillages.Add(village1);
            city.AssociatedVillages.Add(village2);
            city.AssociatedVillages.Add(village3);
                
            var province = new Province("Germania Magna", city);
                
            var city2 = new City("Bristol", new Position(191, 98), 150000);
            province.AssociatedCities.Add(city2);
                
            this._state.Provinces.Add(province);
        }

        /// <summary>
        /// React to screen dimensions change
        /// </summary>
        public override void Reshape(Size newSize)
        {
            base.Reshape(newSize);
            
            this._surface?.Destroy();
            
            this._surface = Surface.New()
                .Tileset(this.Resources, "myne_rect.png")
                .PixelDimensions(this.ScreenDimensions)
                .Build();
            
            this._terrainView.Dimensions = new Size((int)(this._surface.Dimensions.Width * 0.7f) - 1, this._surface.Dimensions.Height-2);
            this._siteView.Dimensions = this._terrainView.Dimensions;
            
            this._terrainView.RecalulatePositions();
            this._siteView.RecalulatePositions();
            
            this._placementNameWindow.Reshape(this._surface);
        }
    }
}