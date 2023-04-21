using Engine.Core;
using Engine.Graphics;
using Engine.Input;
using OpenToolkit.Graphics.OpenGL;
using SixLabors.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using OpenToolkit.Windowing.Common.Input;
using OpenToolkit.Windowing.GraphicsLibraryFramework;

namespace Game.Ui.Toolkit
{
    /// <summary>
    /// Class encapsulating the state of an immediate mode GUI 
    /// </summary>
    public class UIState
        : RenderCommandRecorder
    {
        #region Properties
        /// <summary>
        /// Whether the UI will wrap around the selection if the last or first widget is reached
        /// </summary>
        public bool WrapSelection { get; set; }
            = true;
        #endregion

        #region Fields
        /// <summary>
        /// Whether this UI currently has focus and therefore receives input
        /// </summary>
        protected bool _hasFocus
            = false;

        /// <summary>
        /// Convenience reference to the input manager
        /// </summary>
        protected InputManager _input;

        /// <summary>
        /// Stack for scoped ids
        /// </summary>
        protected Stack<string> _idStack
            = new Stack<string>();

        /// <summary>
        /// Stack for indentation level modifications
        /// </summary>
        protected Stack<int> _indentStack
            = new Stack<int>();

        /// <summary>
        /// The currently active and selected GUI element, if any.
        /// </summary>
        protected string _activeId
            = null;

        /// <summary>
        /// The layout context for the current frame.
        /// </summary>
        protected LayoutContext _context;

        /// <summary>
        /// Collection of rendering commands used to draw the GUI to a surface
        /// </summary>
        private List<IRenderingCommand> _commandBuffer
            = new List<IRenderingCommand>();

        /// <summary>
        /// Sequence of control ids in the layout, for moving control focus
        /// </summary>
        private List<string> _idSequence
            = new List<string>();

        /// <summary>
        /// Stack of themes used to draw widgets
        /// </summary>
        private Stack<Theme> _themeStack
            = new Stack<Theme>();
        #endregion

        #region Constructor
        /// <summary>
        /// Create a new UI state for creating and drawing immediate mode UI.
        /// </summary>
        /// <param name="input"></param>
        public UIState(InputManager input)
        {
            this._input = input;
        }
        #endregion

        #region Widget and Layout Interface
        #region Theming
        /// <summary>
        /// Push given theme onto the theme stack, making it the active theme
        /// </summary>
        public void PushTheme(Theme theme)
        {
            this._themeStack.Push(theme);
        }

        /// <summary>
        /// Pop currently active theme from the theme stack
        /// </summary>
        public void PopTheme()
        {
            this._themeStack.Pop();
        }
        #endregion

        #region ID Handling
        /// <summary>
        /// Push given id to the id stack.
        /// </summary>
        public void PushId(string id)
        {
            this._idStack.Push(id);
        }

        /// <summary>
        /// Pop id from the id stack.
        /// </summary>
        public void PopId()
        {
            this._idStack.Pop();
        }
        #endregion

        #region Basic Layout and Indentation
        /// <summary>
        /// Go to next beginning of next line, incoropating indentation
        /// </summary>
        public void NextLine()
        {
            this._context.NextLine();
        }

        /// <summary>
        /// Insert a horizontal spacer
        /// </summary>
        public void HorizontalSpace(int value = 1)
        {
            this._context.CurrentPosition += new Position(value, 0);
        }

        /// <summary>
        /// Moves horizontal position to be at the exact center of the current layout bounds
        /// </summary>
        /// <remarks>
        /// Cannot be used when any indentation is active
        /// </remarks>
        public void HorizontalCenter()
        {
            var targetPosition = new Position(this._context.Bounds.Center.X, this._context.CurrentPosition.Y);
            this._context.CurrentPosition = targetPosition;
        }

        /// <summary>
        /// Causes the next widget command to not cause a new line.
        /// </summary>
        public void NoNewLine()
        {
            this._context.InhibitNewline = true;
        }

        /// <summary>
        /// Push indentation increase by given value.
        /// </summary>
        public void PushIndent(int value = 1)
        {
            this._indentStack.Push(value);
            this._context.IncreaseIndent(value);
        }

        /// <summary>
        /// Pop last indentation increase.
        /// </summary>
        public void PopIndent()
        {
            var value = this._indentStack.Pop();
            this._context.DecreaseIndent(value);
        }
        #endregion

        #region Key Input
        /// <summary>
        /// Checks whether the given key is currently active
        /// </summary>
        public bool HasKey(Key key, KeyPressType pressType = KeyPressType.Pressed)
        {
            return this._hasFocus && this._input.IsKeyDown(pressType, key);
        }
        #endregion

        #region Widgets
        /// <summary>
        /// Create a label at the current position
        /// </summary>
        /// <param name="text"></param>
        public void Label(string label, bool centered = false)
        {
            var wparams = new WidgetDrawParams(this._context.CurrentPosition) with { Text = label };
            this._context.CurrentPosition = this.GetActiveTheme().DrawLabel(this, wparams);
            this._context.NextLineWidget();
        }

        /// <summary>
        /// Create a button at the current position
        /// </summary>
        public bool Button(string label, bool enabled = true, bool centered = false)
        {
            if (enabled)
            {
                var id = this.RecordWidgetId(label);
                var wparams = new WidgetDrawParams(this._context.CurrentPosition)
                    with { Text = label, IsEnabled = enabled, IsSelected = this.IsSelected(id), Centered = centered };

                this._context.CurrentPosition = this.GetActiveTheme().DrawButton(this, wparams);

                this._context.NextLineWidget();

                if (this._hasFocus && this.IsSelected(id) && this._input.IsKeyDown(KeyPressType.Pressed, Key.Enter))
                    return true;
                else
                    return false;
            }
            else
            {
                var wparams = new WidgetDrawParams(this._context.CurrentPosition)
                    with { Text = label, IsEnabled = false, Centered = centered };
                this._context.CurrentPosition = this.GetActiveTheme().DrawButton(this, wparams);

                this._context.NextLineWidget();

                return false;
            }
        }

        /*/// <summary>
        /// Create a checkbox based on given boolean value.
        /// </summary>
        /// <returns>Returns true if the value has changed</returns>
        public bool Checkbox(string label, ref bool value)
        {
            var id = this.RecordWidgetId(label);
            this.RecordDrawString(this._context.CurrentPosition,
                $"{(this.IsSelected(id) ? ">" : "")}{label} [{(value ? 'X' : ' ')}]");

            this._context.NextLineWidget();

            if (this.IsSelected(id) && this._input.IsKeyDown(KeyPressType.Pressed, Key.Enter))
            {
                value = !value;
                return true;
            }
            else
            {
                return false;
            }
        }*/

        /// <summary>
        /// Draw a window with the given bounds and title. This will set the current position and origin
        /// to be contained within the windows bounds.
        /// </summary>
        public void Window(Rectangle bounds, string title = "", Padding padding = new Padding(), bool drawBorder = true)
        {
            // Calculate actual bounds
            var wparams = new WidgetDrawParams() with { Bounds = bounds, Text = title, WithBorder = drawBorder };
            this.GetActiveTheme().DrawWindow(this, wparams);

            var actualBounds = bounds.WithPadding(padding);

            // Add padding for the border, if we are drawing one.
            if (drawBorder)
                actualBounds = actualBounds.WithPadding(new Padding(1, 1, 1, 1));

            // Update bounds in layout context
            this._context.UpdateBounds(actualBounds);
        }

        /// <summary>
        /// Draw a window centered on the screen, with a width and height being a percentage of the surface dimensions
        /// </summary>
        /// <param name="factors">Percent values of parent surface height and width to use for window dimensions</param>
        public void Window(SizeF factors, string title = "", Padding padding = new Padding(), bool drawBorder = true)
        {
            // Get current bounds for dimension calculations
            var currentBounds = this._context.Bounds;
            var dimensions = currentBounds.Size * factors;

            var middleOffset = currentBounds.Size * 0.5f;
            var middle = currentBounds.TopLeft + (Position)middleOffset;

            var halfSize = dimensions * 0.5f;

            var topLeft = middle - (Position)halfSize;
            var bottomRight = middle + (Position)halfSize;

            // Now determine rectangle of window
            var windowBounds = new Rectangle(topLeft, bottomRight);

            this.Window(windowBounds, title, padding, drawBorder);
        }
        #endregion
        #endregion

        #region Frame Handling and Drawing
        /// <summary>
        /// This method has to be called each frame/update before issuing any GUI calls.
        /// It prepares the internal state.
        /// </summary>
        public void Begin(Surface surface, bool hasFocus = true, Padding padding = new Padding())
        {
            this._hasFocus = hasFocus;

            // Determine initial layout bounds based on surface dimensions
            var surfaceDimensions = surface.Dimensions;
            var surfaceBounds = new Rectangle(
                padding.Left, surfaceDimensions.Width - 1 + padding.Left - padding.Right,
                padding.Top, surfaceDimensions.Height - 1 + padding.Top - padding.Bottom
            );

            // Setup a new, empty layout context based on the surface bounds
            this._context = new LayoutContext(surfaceBounds);

            // Clear the rendering command buffer
            this._commandBuffer.Clear();

            // Reset internal stacks, like id and indent
            this._idStack.Clear();
            this._indentStack.Clear();

            // Setup default theme
            this._themeStack.Clear();
            this._themeStack.Push(new Theme());

            // Reset widget id sequence
            this._idSequence.Clear();
        }

        /// <summary>
        /// This method has to be called each frame/update after the last GUI call has
        /// been issued.
        /// </summary>
        public void End()
        {
            // Handle input if the UI has focus
            if (this._hasFocus)
            {
                // Adjust selection if the user pressed the up or down button.
                this.AdjustSelection();
            }
        }

        /// <summary>
        /// Draw result of immediate mode GUI calls to given surface.
        /// </summary>
        public void Draw(Surface surface)
        {
            // Create a rendering context for the given surface
            var context = new RenderingContext(surface);

            // Execute all recorded commands
            foreach (var command in this._commandBuffer)
                command.ApplyTo(context);
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Retrieve the currently active theme, aka the top of the theme stack
        /// </summary>
        protected Theme GetActiveTheme()
        {
            return this._themeStack.Peek();
        }

        /// <summary>
        /// Record given rendering command.
        /// </summary>
        /// <param name="command"></param>
        protected override void Record(IRenderingCommand command)
        {
            this._commandBuffer.Add(command);
        }    

        #region ID Handling
        /// <summary>
        /// Record the id for the next selectable widget.
        /// </summary>
        /// <returns>The actual widget id, including ids from the id stack</returns>
        protected string RecordWidgetId(string controlId)
        {
            // Create the actual id, based on the current ID stack
            var actualId = this.MakeId(controlId);

            // If we dont have an active ID, make the first selectable widget the selected one.
            if (this._activeId == null)
                this._activeId = actualId;

            // Record the widget id in the widget sequence
            this._idSequence.Add(actualId);

            return actualId;
        }

        /// <summary>
        /// Create a ID based on control ID given by 
        /// </summary>
        /// <param name="controlId"></param>
        /// <returns></returns>
        protected string MakeId(string id)
        {
            if (this._idStack.Count > 0)
                return string.Join("#", this._idStack.Reverse()) + "#" + id;
            else
                return id;
        }

        /// <summary>
        /// Check whether given widget id is currently active.
        /// </summary>
        protected bool IsSelected(string id)
        {
            return this._activeId == id;
        }
        #endregion

        #region General Input Handling
        /// <summary>
        /// Adjust currently selected widget based on user input
        /// </summary>
        protected void AdjustSelection()
        {
            if (this._idSequence.Count == 0)
                return;

            // Determine current selection index
            var selectionIdx = this._idSequence.IndexOf(this._activeId);

            if(this._input.IsKeyDown(KeyPressType.Down, Key.Up))
            {
                if (selectionIdx > 0)
                    selectionIdx--;
                else if(this.WrapSelection)
                {
                    selectionIdx = this._idSequence.Count - 1;
                }
            }
            else if (this._input.IsKeyDown(KeyPressType.Down, Key.Down))
            {
                if (selectionIdx < this._idSequence.Count - 1)
                    selectionIdx++;
                else if (this.WrapSelection)
                {
                    selectionIdx = 0;
                }
            }

            this._activeId = this._idSequence[selectionIdx];
        }
        #endregion
        #endregion
    }
}
