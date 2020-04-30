using System;
using OpenToolkit.Windowing.Common.Input;

namespace Engine.Input
{
    /// <summary>
    /// A high-level input action, derived from low-level keyboard input. It is associated with a single member
    /// of a user-defined enumeration, which can be used to handle the event when its triggered.
    /// </summary>
    /// <typeparam name="TActionEnum">The user-defined action enumeration type to use</typeparam>
    public class InputAction<TActionEnum> where TActionEnum: Enum
    {
        /// <summary>
        /// The key combination that triggers this action
        /// </summary>
        public Key[] Keys { get; protected set; }
        
        /// <summary>
        /// Which kind of key press type the key combination reacts to
        /// </summary>
        public KeyPressType KeyPressType { get; protected set; }
        
        /// <summary>
        /// The enum member value this action is identified by
        /// </summary>
        public TActionEnum ActionValue { get; protected set; }

        /// <summary>
        /// Create a new input action instance with given values.
        /// </summary>
        /// <param name="actionValue">The enumeration value to generate when this action is triggered</param>
        /// <param name="keyPressType">What type of key press to detect</param>
        /// <param name="keyCombo">The key combination that triggers this action</param>
        public InputAction(TActionEnum actionValue, KeyPressType keyPressType, params Key[] keyCombo)
        {
            this.Keys = keyCombo;
            this.KeyPressType = keyPressType;
            this.ActionValue = actionValue;
        }

        /// <summary>
        /// Check whether this action is triggered based on current raw keyboard input state
        /// </summary>
        /// <param name="inputManager">Input manager to retrieve raw input from</param>
        /// <returns>Flag indicating whether this action is triggered</returns>
        public bool IsTriggered(InputManager inputManager)
        {
            return inputManager.AreKeysDown(this.KeyPressType, this.Keys);
        }
    }
}