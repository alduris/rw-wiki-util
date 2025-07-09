namespace WikiUtil.Widgets
{
    public interface IHaveChildren
    {
        /// <summary>
        /// Adds a child to the element and initializes it. Child should not have already been initialized.
        /// </summary>
        /// <param name="child">The child to add</param>
        /// <returns>The child widget being added for reuse</returns>
        public abstract Widget AddChild(Widget child);

        /// <summary>
        /// Inserts a child before the specified index and initializes it. Child should not have already been initialized.
        /// </summary>
        /// <param name="child">The child to add</param>
        /// <param name="index">The index to insert at</param>
        /// <returns>The child widget being added for reuse</returns>
        public abstract Widget InsertChild(Widget child, int index);

        /// <summary>
        /// Removes and destroys a child element.
        /// </summary>
        /// <param name="child">The child to remove</param>
        /// <returns>Whether or not the child was successfully removed</returns>
        public abstract bool RemoveChild(Widget child);
    }
}
