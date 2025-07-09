using System.Collections.Generic;
using UnityEngine;

namespace WikiUtil.Widgets
{
    /// <summary>
    /// A UI element
    /// </summary>
    public abstract class Widget
    {
        public Widget Parent { get; protected internal set; } = null;

        public abstract Vector2 Size { get; }
        public float Width => Size.x;
        public float Height => Size.y;

        /// <summary>
        /// The sprite container. Also contains the containers of any children added.
        /// </summary>
        protected internal FContainer myContainer;

        /// <summary>
        /// Initializes the sprites of this element. Called internally, do not call yourself.
        /// </summary>
        /// <returns>The FContainer</returns>
        protected internal abstract FContainer InitializeSprites();

        /// <summary>
        /// Updates the sprites of the container, if needed. Always call base.Update().
        /// </summary>
        public abstract void UpdateSprites();

        /// <summary>
        /// Removes children and their containers.
        /// </summary>
        public virtual void Destroy()
        {
            (Parent as IHaveChildren)?.RemoveChild(this);
            myContainer.RemoveAllChildren();
            myContainer.RemoveFromContainer();
        }

        public Widget TopParent => Parent?.TopParent ?? this;
        public Vector2 AbsPos => TopParent == this ? myContainer.GetPosition() : TopParent.AbsPos;
    }
}
