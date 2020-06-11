using System;
using System.IO;
using System.Linq;
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
    public class MapViewerScene : Scene
    {
        private enum MapViewerAction
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
            ShowMap,
            ShowRainfall,
            ShowTemperature,
            ShowDrainage,
            ShowResources,
            ShowInfluence
        }

        private World _world;
        private Surface _surface;
        private InputActionMapper<MapViewerAction> _actionMapper;
        private int _seed = 1770780010;
        private Random _random = new Random();
        private MapView _detailedView, _overviewView;

        private WorldGenerator _worldGen;
        private string _worldGenPhase;
        private double _worldGenProgress;
        private bool _isGeneratingMap = false;
        private bool _showInfluence = false;
        private City _currentCity = null; // City in whos influence radius the cursor currently is
        public MapViewerScene(Scene parent) : base(parent)
        {
            this.Initialize();
        }

        public MapViewerScene(SceneStack sceneStack, InputManager inputManager, ResourceManager resourceManager) : base(sceneStack, inputManager, resourceManager)
        {
            this.Initialize();
        }

        private void InitializeViews()
        {
            this._detailedView = new MapView(new Position(1, 1), Size.Empty);
            this._overviewView = new MapView(Position.Origin, Size.Empty);
        }

        private void RegenerateWorld(int seed)
        {
            this._worldGen = new WorldGenerator(new Size(256, 256), seed); // 340, 340
            
            this._worldGen.WorldGenerationStageChanged += (text, progress) =>
            {
                this._worldGenPhase = text;
                this._worldGenProgress = progress;
            };

            this._worldGen.WorldGenerationFinished += world =>
            {
                this._world = world;
                this._detailedView.ReplaceMap(this._world.DetailedMap);
                this._overviewView.ReplaceMap(this._world.OverviewMap);
                this._worldGen = null;
                this._isGeneratingMap = false;
                
                world.Save(Path.Combine(GameDirectories.SaveGames, "world1"));
                
                // Adjust overview map view height
                this._overviewView.Dimensions = new Size((int) (this._surface.Dimensions.Width * 0.3f) - 2,
                    Math.Min(world.OverviewDimensions.Height, (int) (this._surface.Dimensions.Height * 0.65f)));
                
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
                
                world.DetailedMap.Provinces.Add(province);
            };

            this._isGeneratingMap = true;
            this._worldGenProgress = 0.0;
            this._worldGenPhase = "Initializing..";
            
            this._worldGen.Run();
        }

        private void Initialize()
        {
            this.InitializeMapper();
            this.InitializeWorld();
            this.InitializeViews();
        }

        private void InitializeWorld()
        {
            this.RegenerateWorld(1770780010);
        }

        private void InitializeMapper()
        {
            this._actionMapper = new InputActionMapper<MapViewerAction>(this.Input,
                new InputAction<MapViewerAction>(MapViewerAction.MoveUp, KeyPressType.Down, Key.Up),
                new InputAction<MapViewerAction>(MapViewerAction.MoveDown, KeyPressType.Down, Key.Down),
                new InputAction<MapViewerAction>(MapViewerAction.MoveLeft, KeyPressType.Down, Key.Left),
                new InputAction<MapViewerAction>(MapViewerAction.MoveRight, KeyPressType.Down, Key.Right),
                new InputAction<MapViewerAction>(MapViewerAction.MoveUpFast, KeyPressType.Down, Key.Up, Key.ShiftLeft),
                new InputAction<MapViewerAction>(MapViewerAction.MoveDownFast, KeyPressType.Down, Key.Down, Key.ShiftLeft),
                new InputAction<MapViewerAction>(MapViewerAction.MoveLeftFast, KeyPressType.Down, Key.Left, Key.ShiftLeft),
                new InputAction<MapViewerAction>(MapViewerAction.MoveRightFast, KeyPressType.Down, Key.Right, Key.ShiftLeft),
                new InputAction<MapViewerAction>(MapViewerAction.RegenerateMap, KeyPressType.Pressed, Key.R),
                new InputAction<MapViewerAction>(MapViewerAction.ShowMap, KeyPressType.Pressed, Key.M),
                new InputAction<MapViewerAction>(MapViewerAction.ShowRainfall, KeyPressType.Pressed, Key.F),
                new InputAction<MapViewerAction>(MapViewerAction.ShowDrainage, KeyPressType.Pressed, Key.D),
                new InputAction<MapViewerAction>(MapViewerAction.ShowTemperature, KeyPressType.Pressed, Key.T),
                new InputAction<MapViewerAction>(MapViewerAction.ShowInfluence, KeyPressType.Pressed, Key.I),
                new InputAction<MapViewerAction>(MapViewerAction.ShowResources, KeyPressType.Down, Key.R, Key.ShiftLeft)
            );
        }

        private void DrawMap()
        {
            if (this._detailedView.HasMapData)
            {
                this._surface.DrawString(new Position(1, 0), $"Detailed Map (Seed: {this._seed})" +
                                                             $" Cursor: {this._detailedView.CursorPosition.X}:{this._detailedView.CursorPosition.Y}",
                    DefaultColors.White, DefaultColors.Black);


                this._detailedView.Render(this._surface);
            }
        }

        private void DrawTileInfo()
        {
            if (this._world == null)
                return;
            
            var position = new Position(
                this._overviewView.Position.X,
                this._overviewView.Position.Y + this._overviewView.Dimensions.Height + 1);

            var drawInfluence = this._currentCity != null;

            if (drawInfluence)
            {
                this._surface.DrawString(position, $"In Territory: {this._currentCity.Name}",
                    DefaultColors.White, DefaultColors.Black);
                
                position += new Position(0, 1);
            }
            
            this._surface.DrawString(position, TerrainTypeData.GetInfo(this._world.DetailedMap.GetTerrainType(this._detailedView.CursorPosition)).Name,
                DefaultColors.White, DefaultColors.Black);

            if (this._detailedView.ShowResources &&
                this._world.DetailedMap.Resources.ContainsKey(this._detailedView.CursorPosition))
            {
                var resourceType = this._world.DetailedMap.Resources[this._detailedView.CursorPosition];
                this._surface.DrawString(position + new Position(0, 1), resourceType.DisplayName,
                    DefaultColors.White, DefaultColors.Black);
            }
        }

        private void DrawOverview()
        {
            this._surface.DrawString(new Position(this._overviewView.Position.X, 0), "Overview Map", DefaultColors.White, DefaultColors.Black);
            
            if(this._overviewView.HasMapData)
                this._overviewView.Render(this._surface);
        }

        private void HandleInput(MapViewerAction action)
        {
            if (this._isGeneratingMap)
                return;
            
            switch (action)
            {
                case MapViewerAction.RegenerateMap:
                {
                    if (!this._isGeneratingMap)
                    {
                        this._seed = this._random.Next();
                        this.RegenerateWorld(this._seed);
                    }
                    
                    break;
                }
                case MapViewerAction.MoveDown:
                {
                    this._detailedView.Down();
                    break;
                }
                case MapViewerAction.MoveUp:
                {
                    this._detailedView.Up();
                    break;
                }
                case MapViewerAction.MoveLeft:
                {
                    this._detailedView.Left();
                    break;
                }
                case MapViewerAction.MoveRight:
                {
                    this._detailedView.Right();
                    break;
                }
                case MapViewerAction.MoveDownFast:
                {
                    this._detailedView.Down(5);
                    break;
                }
                case MapViewerAction.MoveUpFast:
                {
                    this._detailedView.Up(5);
                    break;
                }
                case MapViewerAction.MoveLeftFast:
                {
                    this._detailedView.Left(5);
                    break;
                }
                case MapViewerAction.MoveRightFast:
                {
                    this._detailedView.Right(5);
                    break;
                }
                case MapViewerAction.ShowResources:
                {
                    this._detailedView.ShowResources = !this._detailedView.ShowResources;
                    break;
                }
                case MapViewerAction.ShowInfluence:
                {
                    this._showInfluence = !this._showInfluence;
                    this._detailedView.DrawCityInfluence = this._showInfluence;

                    if (!this._showInfluence)
                        this._detailedView.CursorMode = CursorMode.Normal;
                        
                    break;
                }
                case MapViewerAction.ShowMap:
                {
                    this._detailedView.DisplayMode = MapViewMode.Terrain;
                    this._overviewView.DisplayMode = MapViewMode.Terrain;
                    break;
                }
                case MapViewerAction.ShowRainfall:
                {
                    this._detailedView.DisplayMode = MapViewMode.Rainfall;
                    this._overviewView.DisplayMode = MapViewMode.Rainfall;
                    break;
                }
                case MapViewerAction.ShowDrainage:
                {
                    this._detailedView.DisplayMode = MapViewMode.Drainage;
                    this._overviewView.DisplayMode = MapViewMode.Drainage;
                    break;
                }
                case MapViewerAction.ShowTemperature:
                {
                    this._detailedView.DisplayMode = MapViewMode.Temperature;
                    this._overviewView.DisplayMode = MapViewMode.Temperature;
                    break;
                }
            }
        }
        
        public override void Render(RenderParams rp)
        {
            this._surface.Clear();
            this.DrawMap();
            this.DrawOverview();
            this.DrawWorldGenProgress();
            this.DrawTileInfo();
            this.DrawKeybindings();
            
            this._surface.Render(rp);
        }

        private void DrawKeybindings()
        {
            if (this._isGeneratingMap)
                return;
            
            var next = 0;

            next = this._surface.DrawKeybinding(new Position(1, this._surface.Dimensions.Height - 2), "r", "New map",
                UiColors.Keybinding, UiColors.ActiveText, DefaultColors.Black);
            
            next = this._surface.DrawKeybinding(new Position(next + 3, this._surface.Dimensions.Height - 2), "mtdf", "Map mode",
                UiColors.Keybinding, UiColors.ActiveText, DefaultColors.Black);
            
            next = this._surface.DrawKeybinding(new Position(next + 3, this._surface.Dimensions.Height - 2), "R", "Show resources",
                UiColors.Keybinding, UiColors.ActiveText, DefaultColors.Black);
            
            next = this._surface.DrawKeybinding(new Position(next + 3, this._surface.Dimensions.Height - 2), "i", "Show influence",
                UiColors.Keybinding, UiColors.ActiveText, DefaultColors.Black);
        }
        
        private void DrawWorldGenProgress()
        {
            if (this._isGeneratingMap)
            {
                var windowBounds = this._surface.Bounds.Centered(new Size(35, 7));

                this._surface.DrawWindow(windowBounds, "Generating World",
                    UiColors.BorderFront, UiColors.BorderBack, UiColors.BorderTitle, DefaultColors.Black);
                
                this._surface.DrawString(windowBounds.TopLeft + new Position(2, 2), this._worldGenPhase, UiColors.ActiveText, DefaultColors.Black);
                
                var progressBarBounds = new Rectangle(windowBounds.TopLeft + new Position(2, 4), new Size(windowBounds.Size.Width - 4, 1));
                this._surface.DrawProgressBar(progressBarBounds, this._worldGenProgress, UiColors.ActiveText, DefaultColors.Black);
            }
        }

        public override void Update(double deltaTime)
        {
            this._worldGen?.ProcessEvents();
            
            this._actionMapper.Update();
            
            if (this._actionMapper.HasTriggeredAction)
            {
                this.HandleInput(this._actionMapper.TriggeredAction);
                
                this._overviewView.CursorPosition = new Position(
                    (int)(this._detailedView.CursorPosition.X * this._world.OverviewScale),
                    (int)(this._detailedView.CursorPosition.Y * this._world.OverviewScale));
            }
            
            this._overviewView.Update(deltaTime);
            this._detailedView.Update(deltaTime);
            

            // Determine in which cities influence the cursor currently is
            var citiesInView = this._detailedView.CitiesInView.Select(x => new {Site = x, Circle = x.InfluenceCircle});
            var result = citiesInView.FirstOrDefault(x => x.Circle.ContainsPoint(this._detailedView.CursorPosition));

            if (result != null)
            {
                this._detailedView.CursorMode = CursorMode.Normal;
                this._currentCity = result.Site;
            }
            else
            {
                this._detailedView.CursorMode = (this._showInfluence ? CursorMode.Invalid : CursorMode.Normal);
                this._currentCity = null;
            }
        }

        public override void Reshape(Size newSize)
        {
            base.Reshape(newSize);
            
            this._surface?.Destroy();
            
            this._surface = Surface.New()
                .Tileset(this.Resources, "myne.png")
                .PixelDimensions(this.ScreenDimensions)
                .Build();
            
            this._detailedView.Dimensions = new Size((int)(this._surface.Dimensions.Width * 0.7f) - 1, this._surface.Dimensions.Height-4);
            
            this._overviewView.Position = new Position((int)(this._surface.Dimensions.Width * 0.7f) + 1, 1);

            if (this._world != null)
            {
                var overviewHeight = this._world.OverviewDimensions.Height;
                
                this._overviewView.Dimensions = new Size((int) (this._surface.Dimensions.Width * 0.3f) - 2,
                    Math.Min(overviewHeight, (int) (this._surface.Dimensions.Height * 0.65f)));
            }
            else
            {
                this._overviewView.Dimensions = new Size((int) (this._surface.Dimensions.Width * 0.3f) - 2,
                    (int) (this._surface.Dimensions.Height * 0.65f));
            }
        }
    }
}