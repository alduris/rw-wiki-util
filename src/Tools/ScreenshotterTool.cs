using System;
using UnityEngine;

namespace WikiUtil.Tools
{
    /// <summary>
    /// Takes a screenshott, but in native resolution and with no compression.
    /// </summary>
    internal class ScreenshotterTool : ITool
    {
        public bool ShouldIRun(RainWorld world) => true;

        public void Run(RainWorld rainWorld, bool update)
        {
            string fullpath = ToolDatabase.GetPathTo("screenshots", DateTime.Now.Ticks + ".png");
            ScreenCapture.CaptureScreenshot(fullpath);
            if (rainWorld.processManager.menuMic != null) rainWorld.processManager.menuMic.PlaySound(SoundID.HUD_Karma_Reinforce_Bump);
            else if (rainWorld.processManager.currentMainLoop is RainWorldGame game) game.cameras[0].virtualMicrophone.PlaySound(SoundID.HUD_Karma_Reinforce_Bump, 0f, 1f, 1f, 1);
            Plugin.Logger.LogInfo("Screenshotted! Path: " + fullpath);
        }
    }
}
