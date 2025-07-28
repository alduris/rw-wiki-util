namespace WikiUtil.Tools
{
    public abstract class HoldTool(string id, Keybind defaultKeybind) : Tool(id, defaultKeybind)
    {
        public override void Update(RainWorld rainWorld)
        {
            if (KeybindHeld)
            {
                HoldUpdate(rainWorld);
            }
        }

        public abstract void HoldUpdate(RainWorld rainWorld);
    }
}
