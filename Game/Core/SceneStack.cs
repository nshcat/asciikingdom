using System;
using System.Collections.Generic;
using Engine.Core;
using Engine.Rendering;

namespace Game.Core
{
    /// <summary>
    /// Represents a stack of scenes that can be influenced by the scenes within using <see cref="SceneOperation"/>
    /// instances.
    /// </summary>
    public class SceneStack : IRenderable, ILogic
    {
        /// <summary>
        /// Actual scene stack data structure
        /// </summary>
        protected Stack<Scene> Scenes { get; set; } = new Stack<Scene>();

        /// <summary>
        /// Whether the scene stack is currently empty
        /// </summary>
        public bool IsEmpty => this.Scenes.Count <= 0;
        
        /// <summary>
        /// The scene stack operation to be executed next.
        /// </summary>
        public SceneStackOperation NextOperation { get; set; } = new SceneStackOperation.None();

        /// <summary>
        /// Sets the initial game scene. Can only be called once.
        /// </summary>
        /// <param name="initialScene">Initial game scene</param>
        public void AddInitialScene(Scene initialScene)
        {
            if(this.Scenes.Count > 0)
                throw new InvalidOperationException("Can't add initial scene to non-empty scene stack");

            this.Scenes.Push(initialScene);
            initialScene.Activate();
        }

        /// <summary>
        /// Render active scene on the stack
        /// </summary>
        /// <param name="rp">Render parameters to use</param>
        public void Render(RenderParams rp)
        {
            // Render top scene, if any
            if (this.Scenes.Count > 0)
            {
                this.Scenes.Peek().Render(rp);
            }
        }

        /// <summary>
        /// Perform pending scene stack operation and update currently active scene
        /// </summary>
        /// <param name="deltaTime">Time elapsed since last update, in seconds</param>
        public void Update(double deltaTime)
        {
            // Perform pending operation, if any
            this.NextOperation.Visit(
                _ =>
                {
                    // Remove active scene, if it exists
                    if (this.Scenes.Count > 0)
                        this.Scenes.Pop();
                    
                    if(this.Scenes.Count > 0)
                        this.Scenes.Peek().Activate();
                },
                op =>
                {
                    // This is required since new scenes pushed onto the scene stack might not get a screen
                    // reshape event if the window isnt resized, and thus might run into nun-initialized surfaces
                    if (this.Scenes.Count > 0)
                    {
                        op.NewScene.Reshape(this.Scenes.Peek().ScreenDimensions);
                    }
                    
                    // Push new scenes onto the scene stack
                    this.Scenes.Push(op.NewScene);
                    op.NewScene.Activate();
                },
                op =>
                {
                    if (this.Scenes.Count > 0)
                        this.Scenes.Pop();
                    
                    this.Scenes.Push(op.NewScene);
                    op.NewScene.Activate();
                }
            );
            
            if(this.Scenes.Count > 0)
                this.Scenes.Peek().Update(deltaTime);
        }

        /// <summary>
        /// React to a change of screen dimensions
        /// </summary>
        /// <param name="newSize">New screen dimensions</param>
        public void Reshape(Size newSize)
        {
            // All scenes have to be notified, not only the active one.
            foreach(var scene in this.Scenes)
                scene.Reshape(newSize);
        }
    }
}