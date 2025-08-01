using WikiUtil.Tools;

namespace WikiUtil.BuiltIn
{
    /// <summary>
    /// Test tool that completely pauses the game by not calling orig on RainWorld.Update
    /// </summary>
    internal class PauseTool() : ToggleTool(TOOL_ID, default), IPauseGame
    {
        internal const string TOOL_ID = "Complete pause";

        public bool Pause => toggled;

        public override void ToggleUpdate(RainWorld rainWorld) { }
    }
}
