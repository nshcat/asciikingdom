using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using OpenToolkit.Windowing.Common.Input;
using OpenToolkit.Windowing.Desktop;

namespace Engine.Input
{
    /// <summary>
    /// Represents a wrapper around the OpenTK keyboard system that allows configurable detecting
    /// of keyboard events, such as restricting to only new pressed or allowing continuous input.
    /// </summary>
    public class InputManager
    {
        /// <summary>
        /// List of key modifiers and their corresponding key
        /// </summary>
        private List<(KeyModifiers, Key)> Modifiers { get; } = new List<(KeyModifiers, Key)>()
        {
            (KeyModifiers.Shift, Key.ShiftLeft),
            (KeyModifiers.Control, Key.ControlLeft),
            (KeyModifiers.Shift, Key.ShiftLeft)
        };

        /// <summary>
        /// List of all modifier keys that can be generated
        /// </summary>
        private HashSet<Key> ModifierKeys { get; } = new HashSet<Key>()
        {
            Key.ShiftLeft,
            Key.ControlLeft,
            Key.AltLeft
        };
        
        /// <summary>
        /// The game window instance the inputs are derived from
        /// </summary>
        private GameWindow Window { get; set; }
        
        /// <summary>
        /// The currently modified state. Will become the current state after a frame.
        /// </summary>
        private KeyboardState _activeState = new KeyboardState();

        /// <summary>
        /// The current keyboard input state
        /// </summary>
        private KeyboardState CurrentState => _currentState;
        
        private KeyboardState _currentState = new KeyboardState();

        /// <summary>
        /// The previous keyboard input state
        /// </summary>
        private KeyboardState PreviousState { get; set; }

        /// <summary>
        /// String buffer used to accumulate keyboard text input
        /// </summary>
        private StringBuilder TextBuffer { get; set; } = new StringBuilder();

        /// <summary>
        /// Accumulated text input. Can be cleared using <see cref="ClearText"/>.
        /// </summary>
        public string Text => this.TextBuffer.ToString();

        /// <summary>
        /// Construct new input manager attached to given game window
        /// </summary>
        /// <param name="gameWindow">Game window instance to collect input events from</param>
        public InputManager(GameWindow gameWindow)
        {
            this.Window = gameWindow;
            this.Window.TextInput += args =>
            {
                this.TextBuffer.Append(args.AsString);
            };

            this.Window.KeyDown += args =>
            {
                this._activeState[args.Key] = true;

                if (args.Modifiers != 0)
                    this._currentState[args.Key] = false;

                // Check all modifiers
                foreach (var (mod, key) in this.Modifiers)
                {
                    if (args.Modifiers.HasFlag(mod))
                    {
                        this._activeState[key] = true;
                    }
                }
            };
            
            this.Window.KeyUp += args =>
            {
                this._activeState[args.Key] = false;

                // Check all modifiers
                foreach (var (mod, key) in this.Modifiers)
                {
                    if (args.Modifiers.HasFlag(mod))
                    {
                        this._activeState[key] = false;
                    }
                }
            };
        }

        /// <summary>
        /// Clear any accumulated text input
        /// </summary>
        public void ClearText()
        {
            this.TextBuffer.Clear();
        }

        /// <summary>
        /// Perform input update.
        /// </summary>
        public void Update()
        {
            this.PreviousState = this.CurrentState;
            this._currentState = this._activeState;
        }

        /// <summary>
        /// Check if given key is currently pressed
        /// </summary>
        /// <param name="key">Key to check</param>
        /// <param name="type">Key press type to detect</param>
        /// <returns>Whether the given key is pressed according to the given key detection type</returns>
        public bool IsKeyDown(KeyPressType type, Key key)
        {
            if (this.ModifierKeys.Contains(key))
            {
                return (type == KeyPressType.Released) ? !this.CurrentState[key] : this.CurrentState[key];
            }
            else
            {
                switch (type)
                {
                    case KeyPressType.Down:
                        return this.CurrentState[key];

                    case KeyPressType.Pressed:
                        return this.CurrentState[key] && !this.PreviousState[key];

                    case KeyPressType.Released:
                    default:
                        return !this.CurrentState[key] && this.PreviousState[key];
                }
            }
        }

        /// <summary>
        /// Check if given keys are all pressed
        /// </summary>
        /// <param name="type">Key press type to detect</param>
        /// <param name="keys">Keys to check</param>
        /// <returns>Whether the given keys are pressed according to the given key detection type</returns>
        public bool AreKeysDown(KeyPressType type, params Key[] keys)
        {
            return keys.Select(k => this.IsKeyDown(type, k)).All(x => x);
        }
    }
}