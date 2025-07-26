namespace WikiUtil
{
    /// <summary>
    /// Basic tool
    /// </summary>
    public interface ITool
    {
        /// <summary>
        /// Whether or not the tool should be running right now.
        /// Your registered keybind is checked automatically, so this defines if it should only run under a specific circumstance.
        /// </summary>
        /// <param name="rainWorld">The current instance of <see cref="RainWorld"/>.</param>
        public bool ShouldIRun(RainWorld rainWorld);

        /// <summary>
        /// Defines what happens when the tool is run (when its keybind is pressed and ShouldIRun returns true).
        /// This is run during both graphical and fixed update ticks, you can use the parameter to differentiate.
        /// </summary>
        /// <param name="rainWorld">The current instance of <see cref="RainWorld"/>.</param>
        /// <param name="update">Set to true if this is run during a regular update tick (only for <see cref="ProcessManager.currentMainLoop"/>).</param>
        public void Run(RainWorld rainWorld, bool update);
    }
}
