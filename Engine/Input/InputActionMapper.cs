using System;
using System.Collections.Generic;
using System.Linq;
using Engine.Graphics;

namespace Engine.Input
{
    /// <summary>
    /// Class mapping raw keyboard input to high-level <see cref="InputAction{TActionEnum}"/> events
    /// </summary>
    /// <typeparam name="TActionEnum">The user-defined action enumeration type to use</typeparam>
    public class InputActionMapper<TActionEnum> where TActionEnum: Enum
    {
        /// <summary>
        /// The input manager instance to use as source for raw keyboard input
        /// </summary>
        public InputManager InputManager { get; protected set; }
        
        /// <summary>
        /// All actions registered with this action mapper
        /// </summary>
        public List<InputAction<TActionEnum>> Actions { get; protected set; }

        /// <summary>
        /// The currently triggered action. Might be null if no such action exists.
        /// </summary>
        public List<TActionEnum> TriggeredActions { get; protected set; }
            = new List<TActionEnum>();

        /// <summary>
        /// Check whether there currently is a triggered action.
        /// </summary>
        public bool HasTriggeredActions => this.TriggeredActions.Count > 0;

        /// <summary>
        /// Create a new input action mapper with given actions.
        /// </summary>
        /// <param name="actions">Actions to add to the mapper</param>
        public InputActionMapper(InputManager inputManager, params InputAction<TActionEnum>[] actions)
        {
            this.InputManager = inputManager;
            
            // Order actions by their key combination length, in descending order. This way, we can always trigger
            // the most complicated combination first.
            this.Actions = actions.OrderByDescending(a => a.Keys.Length).ToList();
        }

        /// <summary>
        /// Update the state of the action mapper and detect action events.
        /// </summary>
        public void Update()
        {
            // Remove previous triggered action, if any
            this.TriggeredActions.Clear();
            
            // Check each action for triggering
            foreach (var action in this.Actions)
            {
                if (action.IsTriggered(this.InputManager))
                {
                    this.TriggeredActions.Add(action.ActionValue);
                }
            }
        }
    }
}