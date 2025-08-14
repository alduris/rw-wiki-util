using WikiUtil.Tools;

namespace WikiUtil.BuiltIn
{
    /// <summary>
    /// Test tool that completely pauses the game by not calling orig on RainWorld.Update
    /// </summary>
    internal class PauseTool() : HoldTool(TOOL_ID, new Keybind(UnityEngine.KeyCode.Escape, false, false, true)), IPauseGame
    {
        internal const string TOOL_ID = "Complete pause";

        public bool Pause => KeybindHeld;

        public override void Held(RainWorld rainWorld) { }
    }
}
