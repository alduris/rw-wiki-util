namespace WikiUtil.Tools
{
    /// <summary>
    /// Type of <see cref="Tool"/> that runs an action when its keybind is pressed
    /// </summary>
    /// <param name="id">ID/display name of the tool</param>
    /// <param name="defaultKeybind">Default keybind of the tool</param>
    public abstract class ActionTool(string id, Keybind defaultKeybind) : Tool(id, defaultKeybind)
    {
        /// <summary>
        /// Runs during <see cref="RainWorld.Update"/>
        /// </summary>
        /// <param name="rainWorld">The instance of <see cref="RainWorld"/></param>
        public override void Update(RainWorld rainWorld)
        {
            if (KeybindPressed)
            {
                Action(rainWorld);
            }
        }

        /// <summary>
        /// Action to run when the keybind is pressed
        /// </summary>
        /// <param name="rainWorld">The instance of <see cref="RainWorld"/></param>
        public abstract void Action(RainWorld rainWorld);
    }
}
