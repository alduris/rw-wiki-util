namespace WikiUtil.Tools
{
    /// <summary>
    /// Type of <see cref="Tool"/> that can be toggled on and off.
    /// </summary>
    /// <param name="id">ID/display name of the tool</param>
    /// <param name="defaultKeybind">Default keybind of the tool</param>
    /// <remarks>
    /// For a variation with a GUI that gets toggled on and off via the keybind, see <see cref="GUIToggleTool"/>.
    /// </remarks>
    public abstract class ToggleTool(string id, Keybind defaultKeybind) : Tool(id, defaultKeybind)
    {
        /// <summary>
        /// Current toggled mode
        /// </summary>
        public bool toggled = false;

        /// <summary>
        /// Runs during <see cref="RainWorld.Update"/>
        /// </summary>
        /// <param name="rainWorld">The instance of <see cref="RainWorld"/></param>
        public override void Update(RainWorld rainWorld)
        {
            if (KeybindPressed)
            {
                toggled = !toggled;
                if (toggled)
                    ToggleOn(rainWorld);
                else
                    ToggleOff(rainWorld);
            }

            if (toggled)
            {
                ToggleUpdate(rainWorld);
            }
        }

        /// <summary>
        /// Runs every frame that <see cref="toggled"/> is <c>true</c>
        /// </summary>
        /// <param name="rainWorld">The instance of <see cref="RainWorld"/></param>
        public abstract void ToggleUpdate(RainWorld rainWorld);

        /// <summary>
        /// Runs on the first frame of being toggled on via the keybind
        /// </summary>
        /// <param name="rainWorld">The instance of <see cref="RainWorld"/></param>
        public virtual void ToggleOn(RainWorld rainWorld) { }

        /// <summary>
        /// Runs on the first frame of being toggled off via the keybind
        /// </summary>
        /// <param name="rainWorld">The instance of <see cref="RainWorld"/></param>
        public virtual void ToggleOff(RainWorld rainWorld) { }
    }
}
