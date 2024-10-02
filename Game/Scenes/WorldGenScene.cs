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
using Game.Simulation.Worlds;

namespace Game.Scenes
{
    /// <summary>
    /// World generator scene
    /// </summary>
    public class WorldGenScene : Scene
    {
        /// <summary>
        /// Actions that the user can take in the world generation scene
        /// </summary>
        private enum WorldGenAction
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
            SaveWorld,
            ToggleFogOfWar,
            Cancel
        }

        /// <summary>
        /// The currently displayed world
        /// </summary>
        private World _world;

        /// <summary>
        /// Whether there currently is a generated map available
        /// </summary>
        private bool HasWorld => this._world != null;
        
        /// <summary>
        /// Main surface
        /// </summary>
        private Surface _surface;
        
        /// <summary>
        /// The input action mapper
        /// </summary>
        private InputActionMapper<WorldGenAction> _actionMapper;
        
        /// <summary>
        /// Current world generator seed
        /// </summary>
        private int _seed = 1770780010;

        /// <summary>
        /// The current world generator parameters
        /// </summary>
        private WorldParameters _worldParameters = new WorldParameters();
        
        /// <summary>
        /// Random number generator used to generate random seeds
        /// </summary>
        private Random _random = new Random();
        
        /// <summary>
        /// Detailed map view
        /// </summary>
        private MapView _detailedView;
        
        /// <summary>
        /// Overview map view
        /// </summary>
        private MapView _overviewView;

        /// <summary>
        /// World generator instance
        /// </summary>
        private WorldGenerator _worldGen;

        /// <summary>
        /// The current world generator phase
        /// </summary>
        private string _worldGenPhase;
        
        /// <summary>
        /// The current progress of the world generation
        /// </summary>
        private double _worldGenProgress;
        
        /// <summary>
        /// Whether we currently are generating a map
        /// </summary>
        private bool _isGeneratingMap = false;
        
        /// <summary>
        /// Create new world generator scenen instance
        /// </summary>
        public WorldGenScene(Scene parent) : base(parent)
        {
            this.Initialize();
        }

        /// <summary>
        /// Create new world generator scenen instance
        /// </summary>
        public WorldGenScene(SceneStack sceneStack, InputManager inputManager, ResourceManager resourceManager) : base(sceneStack, inputManager, resourceManager)
        {
            this.Initialize();
        }

        /// <summary>
        /// Initialize the world views
        /// </summary>
        private void InitializeViews()
        {
            this._detailedView = new MapView(new Position(1, 1), Size.Empty);
            this._overviewView = new MapView(Position.Origin, Size.Empty);

            this._detailedView.CursorMoved += (newPosition) =>
            {
                if (!this.HasWorld)
                    return;
                
                this._overviewView.CursorPosition = new Position(
                    (int)(newPosition.X * this._world.OverviewScale),
                    (int)(newPosition.Y * this._world.OverviewScale));

                this._overviewView.RecalulatePositions();
            };
        }

        /// <summary>
        /// Start world generation with given seed
        /// </summary>
        private void RegenerateWorld(int seed)
        {
            this._worldGen = new WorldGenerator(new Size(256, 256), seed);
            
            this._worldGen.WorldGenerationStageChanged += (text, progress) =>
            {
                this._worldGenPhase = text;
                this._worldGenProgress = progress;
            };

            this._worldGen.WorldGenerationFinished += world =>
            {
                this.ReplaceWorld(world);

                this._detailedView.CursorPosition = world.Metadata.InitialLocation;
                this._detailedView.RecalulatePositions();
                this._detailedView.FireCursorMovedEvent();

                this._worldGen = null;
                this._isGeneratingMap = false;
            };

            this._isGeneratingMap = true;
            this._worldGenProgress = 0.0;
            this._worldGenPhase = "Initializing..";
            
            this._worldGen.Run();
        }

        /// <summary>
        /// Replace the currently active world with given newly-generated world
        /// </summary>
        private void ReplaceWorld(World world)
        {
            this._world = world;
            this._detailedView.ReplaceMap(world.DetailedMap);
            this._overviewView.ReplaceMap(world.OverviewMap);

            if (this._surface != null)
            {
                this._overviewView.Dimensions = new Size((int) (this._surface.Dimensions.Width * 0.3f) - 2,
                    Math.Min(world.OverviewDimensions.Height, (int) (this._surface.Dimensions.Height * 0.65f)));
            }
        }

        /// <summary>
        /// Initialize scene
        /// </summary>
        private void Initialize()
        {
            this.InitializeViews();
            this.InitializeMapper();
            this.InitializeWorld();
        }

        /// <summary>
        /// Perform initial world generation with fixed see
        /// </summary>
        private void InitializeWorld()
        {
            //var defaultSeed = 1770780010;
            var defaultSeed = 97395747;
            this._seed = defaultSeed;
            this.RegenerateWorld(defaultSeed);
        }

        /// <summary>
        /// Initialize the input mapper
        /// </summary>
        private void InitializeMapper()
        {
            this._actionMapper = new InputActionMapper<WorldGenAction>(this.Input,
                new InputAction<WorldGenAction>(WorldGenAction.MoveUp, KeyPressType.Down, Key.Up),
                new InputAction<WorldGenAction>(WorldGenAction.MoveDown, KeyPressType.Down, Key.Down),
                new InputAction<WorldGenAction>(WorldGenAction.MoveLeft, KeyPressType.Down, Key.Left),
                new InputAction<WorldGenAction>(WorldGenAction.MoveRight, KeyPressType.Down, Key.Right),
                new InputAction<WorldGenAction>(WorldGenAction.MoveUpFast, KeyPressType.Down, Key.Up, Key.ShiftLeft),
                new InputAction<WorldGenAction>(WorldGenAction.MoveDownFast, KeyPressType.Down, Key.Down, Key.ShiftLeft),
                new InputAction<WorldGenAction>(WorldGenAction.MoveLeftFast, KeyPressType.Down, Key.Left, Key.ShiftLeft),
                new InputAction<WorldGenAction>(WorldGenAction.MoveRightFast, KeyPressType.Down, Key.Right, Key.ShiftLeft),
                new InputAction<WorldGenAction>(WorldGenAction.RegenerateMap, KeyPressType.Pressed, Key.R),
                new InputAction<WorldGenAction>(WorldGenAction.ShowMap, KeyPressType.Pressed, Key.M),
                new InputAction<WorldGenAction>(WorldGenAction.ShowRainfall, KeyPressType.Pressed, Key.F),
                new InputAction<WorldGenAction>(WorldGenAction.ShowDrainage, KeyPressType.Pressed, Key.D),
                new InputAction<WorldGenAction>(WorldGenAction.ShowTemperature, KeyPressType.Pressed, Key.T),
                new InputAction<WorldGenAction>(WorldGenAction.SaveWorld, KeyPressType.Pressed, Key.U),
                new InputAction<WorldGenAction>(WorldGenAction.ShowResources, KeyPressType.Down, Key.R, Key.ShiftLeft),
                new InputAction<WorldGenAction>(WorldGenAction.ToggleFogOfWar, KeyPressType.Down, Key.F, Key.ShiftLeft),
                new InputAction<WorldGenAction>(WorldGenAction.Cancel, KeyPressType.Down, Key.Q, Key.ShiftLeft)
            );
        }

        /// <summary>
        /// Draw detailed view
        /// </summary>
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

        /// <summary>
        /// Draw the tile info panel
        /// </summary>
        private void DrawTileInfo()
        {
            if (!this.HasWorld)
                return;
            
            var position = new Position(
                this._overviewView.Position.X,
                this._overviewView.Position.Y + this._overviewView.Dimensions.Height + 1);

            if (!this._world.DetailedMap.IsDiscovered(this._detailedView.CursorPosition) && this._detailedView.DrawFogOfWar)
            {
                this._surface.DrawString(position, "Unknown",
                    UiColors.ActiveText, DefaultColors.Black);

                return;
            }

            this._surface.DrawString(position, TerrainTypeData.GetInfo(this._world.DetailedMap.GetTerrainType(this._detailedView.CursorPosition)).Name,
                DefaultColors.White, DefaultColors.Black);
        }

        /// <summary>
        /// Draw overview map
        /// </summary>
        private void DrawOverview()
        {
            this._surface.DrawString(new Position(this._overviewView.Position.X, 0), "Overview Map", DefaultColors.White, DefaultColors.Black);
            
            if(this._overviewView.HasMapData)
                this._overviewView.Render(this._surface);
        }

        /// <summary>
        /// Handle input actions
        /// </summary>
        private bool HandleInput(WorldGenAction action)
        {
            if (this._isGeneratingMap)
                return false;
            
            switch (action)
            {
                case WorldGenAction.RegenerateMap:
                {
                    if (!this._isGeneratingMap)
                    {
                        this._seed = this._random.Next();
                        this.RegenerateWorld(this._seed);
                    }
                    
                    break;
                }
                case WorldGenAction.MoveDown:
                {
                    this._detailedView.MoveCursor(MovementDirection.Down);
                    break;
                }
                case WorldGenAction.MoveUp:
                {
                    this._detailedView.MoveCursor(MovementDirection.Up);
                    break;
                }
                case WorldGenAction.MoveLeft:
                {
                    this._detailedView.MoveCursor(MovementDirection.Left);
                    break;
                }
                case WorldGenAction.MoveRight:
                {
                    this._detailedView.MoveCursor(MovementDirection.Right);
                    break;
                }
                case WorldGenAction.MoveDownFast:
                {
                    this._detailedView.MoveCursor(MovementDirection.Down, 5);
                    break;
                }
                case WorldGenAction.MoveUpFast:
                {
                    this._detailedView.MoveCursor(MovementDirection.Up, 5);
                    break;
                }
                case WorldGenAction.MoveLeftFast:
                {
                    this._detailedView.MoveCursor(MovementDirection.Left, 5);
                    break;
                }
                case WorldGenAction.MoveRightFast:
                {
                    this._detailedView.MoveCursor(MovementDirection.Right, 5);
                    break;
                }
                case WorldGenAction.ShowResources:
                {
                    this._detailedView.ShowResources = !this._detailedView.ShowResources;
                    break;
                }
                case WorldGenAction.ShowMap:
                {
                    this._detailedView.DisplayMode = MapViewMode.Terrain;
                    this._overviewView.DisplayMode = MapViewMode.Terrain;
                    break;
                }
                case WorldGenAction.ShowRainfall:
                {
                    this._detailedView.DisplayMode = MapViewMode.Rainfall;
                    this._overviewView.DisplayMode = MapViewMode.Rainfall;
                    break;
                }
                case WorldGenAction.ShowDrainage:
                {
                    this._detailedView.DisplayMode = MapViewMode.Drainage;
                    this._overviewView.DisplayMode = MapViewMode.Drainage;
                    break;
                }
                case WorldGenAction.ShowTemperature:
                {
                    this._detailedView.DisplayMode = MapViewMode.Temperature;
                    this._overviewView.DisplayMode = MapViewMode.Temperature;
                    break;
                }
                case WorldGenAction.ToggleFogOfWar:
                {
                    this._detailedView.DrawFogOfWar = !this._detailedView.DrawFogOfWar;
                    this._overviewView.DrawFogOfWar = !this._overviewView.DrawFogOfWar;
                    break;
                }
                case WorldGenAction.SaveWorld:
                {
                    if (this.HasWorld)
                    {
                        this.SaveWorld();
                        this.SceneStack.NextOperation = new SceneStackOperation.PopScene();
                    }
                    break;
                }
                case WorldGenAction.Cancel:
                {
                    this.SceneStack.NextOperation = new SceneStackOperation.PopScene();
                    break;
                }
                default:
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Save the current world to disk
        /// </summary>
        private void SaveWorld()
        {
            if(!this.HasWorld)
                throw new InvalidOperationException("No world was generated");
            
            // Create empty simulation state
            var state = new SimulationState(this._world);
            WorldManager.Instance.SaveWorld(state);
        }
        
        /// <summary>
        /// Render this scene
        /// </summary>
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

        /// <summary>
        /// Draw the keybindings
        /// </summary>
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
            
            next = this._surface.DrawKeybinding(new Position(next + 3, this._surface.Dimensions.Height - 2), "u", "Use this world",
                UiColors.Keybinding, UiColors.ActiveText, DefaultColors.Black);
            
            next = this._surface.DrawKeybinding(new Position(next + 3, this._surface.Dimensions.Height - 2), "F", "Toggle fog of war",
                UiColors.Keybinding, UiColors.ActiveText, DefaultColors.Black);
        }
        
        /// <summary>
        /// Draw the world generator progress bar
        /// </summary>
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

        /// <summary>
        /// Perform logic update
        /// </summary>
        public override void Update(double deltaTime)
        {
            this._worldGen?.ProcessEvents();
            
            this._actionMapper.Update();

            foreach (var action in this._actionMapper.TriggeredActions)
            {
                if (this.HandleInput(action))
                    break;
            }

            this._overviewView.Update(deltaTime);
            this._detailedView.Update(deltaTime);
        }

        /// <summary>
        /// React to screen dimensions change
        /// </summary>
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

            if (this.HasWorld)
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

            this._detailedView.RecalulatePositions();
            this._overviewView.RecalulatePositions();
        }
    }
}