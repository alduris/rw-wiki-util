using System;
using System.Collections.Generic;
using UnityEngine;

namespace WikiUtil.Widgets
{
    /// <summary>
    /// Widget that holds other widgets.
    /// </summary>
    internal class Container : Widget, IHaveChildren
    {

        public bool horizontal = false;
        public StyleDefinition style;

        /// <summary>
        /// Initializes a container.
        /// </summary>
        /// <param name="horizontal">Whether or not to construct the container horizontally</param>
        /// <param name="style"></param>
        public Container(bool horizontal = false, StyleDefinition? style = null)
        {
            this.horizontal = horizontal;
            this.style = style ?? new StyleDefinition();
        }

        public virtual List<Widget> Children { get; } = [];

        public override Vector2 Size
        {
            get
            {
                Vector2 cellMinSize = style.cellMinSize ?? Vector2.zero;
                float w = 0f, h = 0f;

                foreach (Widget child in Children)
                {
                    Vector2 size = child.Size;
                    if (horizontal)
                    {
                        w += Mathf.Max(size.x, cellMinSize.x);
                        h = Mathf.Max(size.y, cellMinSize.y, h);
                    }
                    else
                    {
                        w = Mathf.Max(size.x, cellMinSize.x, w);
                        h += Mathf.Max(size.y, cellMinSize.y);
                    }
                }

                w += 2 * style.padding + style.borderWidth;
                h += 2 * style.padding + style.borderWidth;

                if (horizontal)
                    w += style.gap * Math.Max(0, Children.Count - 1);
                else
                    h += style.gap * Math.Max(0, Children.Count - 1);

                return new Vector2(w, h);
            }
        }

        protected internal override FContainer InitializeSprites()
        {
            throw new NotImplementedException();
        }

        public override void UpdateSprites()
        {
            foreach (var child in Children)
            {
                child.UpdateSprites();
            }
        }

        public override void Destroy()
        {
            foreach (var child in Children)
            {
                child.Destroy();
            }
            base.Destroy();
        }

        public virtual Widget AddChild(Widget child)
        {
            Children.Add(child);
            child.Parent = this;
            myContainer.AddChild(child.InitializeSprites());
            return child;
        }

        public Widget InsertChild(Widget child, int index)
        {
            Children.Insert(index, child);
            child.Parent = this;
            myContainer.AddChild(child.InitializeSprites());
            return child;
        }

        public virtual bool RemoveChild(Widget child)
        {
            bool result = Children.Remove(child);
            if (result)
            {
                myContainer.RemoveChild(child.myContainer);
            }
            return result;
        }

        public struct StyleDefinition
        {
            public Color backgroundColor = new Color(0f, 0f, 0f, 0.7f);
            public Color borderColor = Color.white;
            public int borderWidth = 2;
            public int padding = 10;
            public int gap = 6;
            public HAlignment cellHAlignment = HAlignment.Center;
            public VAlignment cellVAlignment = VAlignment.Middle;
            public Vector2? cellMinSize = null;

            public StyleDefinition()
            {
            }
        }
    }
}
