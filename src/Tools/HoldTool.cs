namespace WikiUtil.Tools
{
    /// <summary>
    /// Type of <see cref="Tool"/> that runs every frame its keybind is held
    /// </summary>
    /// <param name="id">ID/display name of the tool</param>
    /// <param name="defaultKeybind">Default keybind of the tool</param>
    public abstract class HoldTool(string id, Keybind defaultKeybind) : Tool(id, defaultKeybind)
    {
        private bool lastKeybindHeld = false;

        /// <summary>
        /// Runs during <see cref="RainWorld.Update"/>
        /// </summary>
        /// <param name="rainWorld">The instance of <see cref="RainWorld"/></param>
        public override void Update(RainWorld rainWorld)
        {
            if (lastKeybindHeld != KeybindHeld)
            {
                if (KeybindHeld)
                {
                    HoldStart(rainWorld);
                }
                else
                {
                    HoldEnd(rainWorld);
                }
            }
            if (KeybindHeld)
            {
                Held(rainWorld);
            }
            lastKeybindHeld = KeybindHeld;
        }

        /// <summary>
        /// Action to run once the first frame the keybind is held
        /// </summary>
        /// <param name="rainWorld">The instance of <see cref="RainWorld"/></param>
        public virtual void HoldStart(RainWorld rainWorld) { }

        /// <summary>
        /// Action to run for every frame the keybind is held
        /// </summary>
        /// <param name="rainWorld">The instance of <see cref="RainWorld"/></param>
        public abstract void Held(RainWorld rainWorld);

        /// <summary>
        /// Action to run once the first frame the keybind is not held
        /// </summary>
        /// <param name="rainWorld">The instance of <see cref="RainWorld"/></param>
        public virtual void HoldEnd(RainWorld rainWorld) { }
    }
}
