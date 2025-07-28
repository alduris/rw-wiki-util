namespace WikiUtil.Tools
{
    public abstract class ActionTool(string id, Keybind defaultKeybind) : Tool(id, defaultKeybind)
    {
        public override void Update(RainWorld rainWorld)
        {
            if (KeybindPressed)
            {
                Action(rainWorld);
            }
        }

        public abstract void Action(RainWorld rainWorld);
    }
}
