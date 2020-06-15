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
            ToggleNewProvince,
            Return,
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
            PlaceCity = 1
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
        /// Whether the site view is currently showing the influence range of cities
        /// </summary>
        private bool _showInfluence = false;

        /// <summary>
        /// Whether placing a new city should also create a new province
        /// </summary>
        private bool _newProvince = false;

        /// <summary>
        /// The current game speed
        /// </summary>
        private GameSpeed _gameSpeed = GameSpeed.Paused;
        
        /// <summary>
        /// City in whose influence radius the cursor is currently in, if any
        /// </summary>
        private Optional<City> _currentCity = Optional<City>.Empty;
        
        /// <summary>
        /// The world site the cursor is currently on, if any
        /// </summary>
        private Optional<IWorldSite> _cursorSite = Optional<IWorldSite>.Empty;

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
                GameSpeed.Fast => 3,
            };
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
                new InputAction<GameAction>(GameAction.SaveAndQuit, KeyPressType.Down, Key.Q, Key.ShiftLeft),
                new InputAction<GameAction>(GameAction.PlaceCity, KeyPressType.Down, Key.C, Key.ShiftLeft),
                new InputAction<GameAction>(GameAction.ToggleNewProvince, KeyPressType.Down, Key.P),
                new InputAction<GameAction>(GameAction.Return, KeyPressType.Down, Key.Escape),
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
        /// Draw the game action menu
        /// </summary>
        private void DrawMenu()
        {
            var position = this.MenuTopLeft;

            foreach (var entry in this._gameMenu)
                position = entry.Render(this._surface, this._uiState, position);
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

            if (this._currentCity.HasValue)
            {
                this._surface.DrawString(position, $"In Territory: {this._currentCity.Value.Name}",
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
        /// Handle user input actions
        /// </summary>
        private void HandleInput(GameAction action)
        {
            switch (action)
            {
                case GameAction.MoveDown:
                {
                    this._terrainView.MoveCursor(MovementDirection.Down);
                    break;
                }
                case GameAction.MoveUp:
                {
                    this._terrainView.MoveCursor(MovementDirection.Up);
                    break;
                }
                case GameAction.MoveLeft:
                {
                    this._terrainView.MoveCursor(MovementDirection.Left);
                    break;
                }
                case GameAction.MoveRight:
                {
                    this._terrainView.MoveCursor(MovementDirection.Right);
                    break;
                }
                case GameAction.MoveDownFast:
                {
                    this._terrainView.MoveCursor(MovementDirection.Down, 5);
                    break;
                }
                case GameAction.MoveUpFast:
                {
                    this._terrainView.MoveCursor(MovementDirection.Up, 5);
                    break;
                }
                case GameAction.MoveLeftFast:
                {
                    this._terrainView.MoveCursor(MovementDirection.Left, 5);
                    break;
                }
                case GameAction.MoveRightFast:
                {
                    this._terrainView.MoveCursor(MovementDirection.Right, 5);
                    break;
                }
                case GameAction.ShowResources:
                {
                    if(this._uiState == GameUiState.Main)
                        this._terrainView.ShowResources = !this._terrainView.ShowResources;
                    
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
                        this._uiState = GameUiState.PlaceCity;
                        this._newProvince = !this.HasAnyProvinces();
                    }
                    else if (this._uiState == GameUiState.PlaceCity)
                        this._uiState = GameUiState.Main;

                    break;
                }
                case GameAction.ToggleNewProvince:
                {
                    if (this._uiState == GameUiState.PlaceCity && this.HasAnyProvinces())
                    {
                        this._newProvince = !this._newProvince;
                    }

                    break;
                }
                case GameAction.Return:
                {
                    if (this._uiState == GameUiState.PlaceCity)
                        this._uiState = GameUiState.Main;

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
            
            this._surface.Render(rp);
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
            
            this._terrainView.Update(deltaTime);
            this._siteView.Update(deltaTime);
            
            UpdateCursorInfluence();
            
            this._state.Update(this.CalculateElapsedWeeks(deltaTime));
        }

        /// <summary>
        /// Determine in which cities influence the cursor currently is
        /// </summary>
        private void UpdateCursorInfluence()
        {
            var citiesInView = this._siteView.CitiesInView.Select(x => new {Site = x, Circle = x.InfluenceCircle});
            var result = citiesInView.FirstOrDefault(x => x.Circle.ContainsPoint(this._terrainView.CursorPosition));

            if (result != null)
            {
                this._siteView.CursorMode = CursorMode.Normal;
                this._currentCity = Optional<City>.Of(result.Site);
            }
            else
            {
                this._siteView.CursorMode = (this._showInfluence ? CursorMode.Invalid : CursorMode.Normal);
                this._currentCity = Optional<City>.Empty;
            }
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
                
            var province = new Province(city);
                
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
        }
    }
}