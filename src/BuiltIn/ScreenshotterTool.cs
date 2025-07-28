using System;
using UnityEngine;
using WikiUtil.Tools;

namespace WikiUtil.BuiltIn
{
    /// <summary>
    /// Takes a screenshot, but in native resolution and with no compression.
    /// </summary>
    internal class ScreenshotterTool() : ActionTool(TOOL_ID, new Keybind(KeyCode.F12))
    {
        internal const string TOOL_ID = "Screenshotter";

        public override void Action(RainWorld rainWorld)
        {
            string fullpath = ToolDatabase.GetPathTo("screenshots", DateTime.Now.Ticks + ".png");
            ScreenCapture.CaptureScreenshot(fullpath);
            if (rainWorld.processManager.menuMic != null) rainWorld.processManager.menuMic.PlaySound(SoundID.HUD_Karma_Reinforce_Bump);
            else if (rainWorld.processManager.currentMainLoop is RainWorldGame game) game.cameras[0].virtualMicrophone.PlaySound(SoundID.HUD_Karma_Reinforce_Bump, 0f, 1f, 1f, 1);
            Plugin.Logger.LogInfo("Screenshotted! Path: " + fullpath);
        }
    }
}
