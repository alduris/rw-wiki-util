namespace WikiUtil.Tools
{
    public abstract class ToggleTool(string id, Keybind defaultKeybind) : Tool(id, defaultKeybind)
    {
        public bool toggled = false;

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

        public abstract void ToggleUpdate(RainWorld rainWorld);

        public virtual void ToggleOn(RainWorld rainWorld) { }
        public virtual void ToggleOff(RainWorld rainWorld) { }
    }
}
