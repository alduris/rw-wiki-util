namespace WikiUtil
{
    /// <summary>
    /// For tools that have a GUI that needs to be shown.
    /// Run independent of keybind checks; if you want a keybind to activate it, you have to add that logic.
    /// </summary>
    public interface IHaveGUI
    {
        /// <summary>
        /// Whether or not to show the GUI. Also requires <see cref="ITool.ShouldIRun(RainWorld)"/> be true.
        /// </summary>
        public bool ShowGUI { get; }

        /// <summary>
        /// GUI code. Please use Unity's IMGUI.
        /// </summary>
        /// <param name="rainWorld">The current instance of <see cref="RainWorld"/>.</param>
        public void OnGUI(RainWorld rainWorld);
    }
}
