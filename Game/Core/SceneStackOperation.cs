
using System;

namespace Game.Core
{
    /// <summary>
    /// Represents a sum type of all possible scene stack operations. An enumeration is not used here since
    /// different operations can have different arguments.
    /// </summary>
    public abstract class SceneStackOperation
    {
        // Private constructor to avoid creation of additional sum type alternatives
        private SceneStackOperation()
        {
            
        }

        /// <summary>
        /// No operation is to be performed on the scene stack.
        /// </summary>
        public class None : SceneStackOperation
        {
            
        }

        /// <summary>
        /// Pop current active scene from scene stack.
        /// </summary>
        public class PopScene : SceneStackOperation
        {
            
        }

        /// <summary>
        /// Push a new scene onto the scene stack.
        /// </summary>
        public class PushScene : SceneStackOperation
        {
            /// <summary>
            /// New scene to push onto the scene stack.
            /// </summary>
            public Scene NewScene { get; protected set; }
            
            public PushScene(Scene newScene)
            {
                this.NewScene = newScene;
            }
        }
        
        /// <summary>
        /// Replace active scene with new scene
        /// </summary>
        public class ReplaceScene : SceneStackOperation
        {
            /// <summary>
            /// New scene to push onto the scene stack.
            /// </summary>
            public Scene NewScene { get; protected set; }
            
            public ReplaceScene(Scene newScene)
            {
                this.NewScene = newScene;
            }
        }

        public void Visit(Action<SceneStackOperation.PopScene> popFunc,
            Action<SceneStackOperation.PushScene> pushFunc,
            Action<SceneStackOperation.ReplaceScene> replaceFunc)
        {
            if (this is SceneStackOperation.PopScene) popFunc(this as SceneStackOperation.PopScene);
            else if (this is SceneStackOperation.PushScene) pushFunc(this as SceneStackOperation.PushScene);
            else if (this is SceneStackOperation.ReplaceScene) replaceFunc(this as SceneStackOperation.ReplaceScene);
        }
    }
}