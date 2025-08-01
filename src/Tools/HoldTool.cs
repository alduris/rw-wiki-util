namespace WikiUtil.Tools
{
    /// <summary>
    /// Type of <see cref="Tool"/> that runs every frame its keybind is held
    /// </summary>
    /// <param name="id">ID/display name of the tool</param>
    /// <param name="defaultKeybind">Default keybind of the tool</param>
    public abstract class HoldTool(string id, Keybind defaultKeybind) : Tool(id, defaultKeybind)
    {
        /// <summary>
        /// Runs during <see cref="RainWorld.Update"/>
        /// </summary>
        /// <param name="rainWorld">The instance of <see cref="RainWorld"/></param>
        public override void Update(RainWorld rainWorld)
        {
            if (KeybindHeld)
            {
                Held(rainWorld);
            }
        }

        /// <summary>
        /// Action to run for every frame the keybind is held
        /// </summary>
        /// <param name="rainWorld">The instance of <see cref="RainWorld"/></param>
        public abstract void Held(RainWorld rainWorld);
    }
}
