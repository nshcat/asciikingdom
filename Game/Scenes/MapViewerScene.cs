using System;
using Engine.Core;
using Engine.Graphics;
using Engine.Input;
using Engine.Rendering;
using Engine.Resources;
using Game.Core;
using Game.Data;
using Game.Ui;
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
            ShowTemperature
        }

        private World _world;
        private Surface _surface;
        private InputActionMapper<MapViewerAction> _actionMapper;
        private int _seed = 1337;
        private Random _random = new Random();

        private MapView _detailedView, _overviewView;

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
            this._detailedView = new MapView(new Position(1, 1), Size.Empty, this._world.DetailMapTiles);
            this._overviewView = new MapView(Position.Origin, Size.Empty, this._world.OverviewMapTiles);
        }

        private void Initialize()
        {
            this.InitializeMapper();
            this.InitializeWorld();
            this.InitializeViews();
        }

        private void InitializeWorld()
        {
            this._world = World.GenerateWorld(new Size(1024, 1024), 1337);
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
                new InputAction<MapViewerAction>(MapViewerAction.ShowTemperature, KeyPressType.Pressed, Key.T)
            );
        }

        private void DrawMap()
        {
            this._surface.DrawString(new Position(1, 0), $"Detailed Map (Seed: {this._seed})" +
                                                         $" Cursor: {this._detailedView.CursorPosition.X}:{this._detailedView.CursorPosition.Y}" +
                                                         $" {TerrainTypeData.GetInfo(this._world.GetTerrainType(this._detailedView.CursorPosition)).Name}",
                DefaultColors.White, DefaultColors.Black);
            
            this._detailedView.Render(this._surface);
        }

        private void DrawOverview()
        {
            this._surface.DrawString(new Position(this._overviewView.Position.X, 0), "Overview Map", DefaultColors.White, DefaultColors.Black);
            
            this._overviewView.Render(this._surface);
        }

        private void HandleInput(MapViewerAction action)
        {
            switch (action)
            {
                case MapViewerAction.RegenerateMap:
                {
                    this._seed = this._random.Next();
                    
                    this._world = World.GenerateWorld(new Size(1024, 1024), this._seed);
                    this._detailedView.MapData = this._world.DetailMapTiles;
                    this._detailedView.Recenter();
                    this._overviewView.MapData = this._world.OverviewMapTiles;
                    this._overviewView.Recenter();

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
                case MapViewerAction.ShowMap:
                {
                    this._detailedView.MapData = this._world.DetailMapTiles;
                    this._overviewView.MapData = this._world.OverviewMapTiles;
                    break;
                }
                case MapViewerAction.ShowRainfall:
                {
                    this._detailedView.MapData = this._world.RainfallMapTiles;
                    this._overviewView.MapData = this._world.RainfallOverviewTiles;
                    break;
                }
                case MapViewerAction.ShowTemperature:
                {
                    this._detailedView.MapData = this._world.TemperatureMapTiles;
                    this._overviewView.MapData = this._world.TemperatureOverviewTiles;
                    break;
                }
            }
        }
        
        
        public override void Render(RenderParams rp)
        {
            this._surface.Clear();
            this.DrawMap();
            this.DrawOverview();

            this._surface.Render(rp);
        }

        public override void Update(double deltaTime)
        {
            this._actionMapper.Update();
            
            if (this._actionMapper.HasTriggeredAction)
            {
                this.HandleInput(this._actionMapper.TriggeredAction);
                
                this._overviewView.CursorPosition = new Position(
                    (int)(this._detailedView.CursorPosition.X * this._world.OverviewScale),
                    (int)(this._detailedView.CursorPosition.Y * this._world.OverviewScale));
            }
        }

        public override void Reshape(Size newSize)
        {
            base.Reshape(newSize);
            
            this._surface?.Destroy();
            
            this._surface = Surface.New()
                .Tileset(this.Resources, "default.png")
                .PixelDimensions(this.ScreenDimensions)
                .Build();
            
            this._detailedView.Dimensions = new Size((int)(this._surface.Dimensions.Width * 0.7f) - 1, this._surface.Dimensions.Height-2);
            
            this._overviewView.Position = new Position((int)(this._surface.Dimensions.Width * 0.7f) + 1, 1);
            this._overviewView.Dimensions = new Size((int)(this._surface.Dimensions.Width * 0.3f)-2, (int)(this._surface.Dimensions.Height * 0.5f));
        }
    }
}