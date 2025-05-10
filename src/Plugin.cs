using BepInEx;
using BepInEx.Logging;
using System.Security.Permissions;
using UnityEngine;
using WikiUtil.Remix;
using WikiUtil.Tools;

// Allows access to private members
#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace WikiUtil;

[BepInPlugin("alduris.wikiutil", "Wiki Util", "1.0")]
sealed class Plugin : BaseUnityPlugin
{
    public static new ManualLogSource Logger;
    private bool _isInit;
    private static RemixMenu _remixMenu;

    public void OnEnable()
    {
        Logger = base.Logger;
        _remixMenu ??= new RemixMenu();
        On.RainWorld.OnModsInit += RegisterRemix;
        On.RainWorld.Update += ToolRunner;

        // Register our stuff
        ToolDatabase.RegisterTool(new ToolDatabase.ToolType("Screenshotter", true), new ScreenshotterTool(), new ToolDatabase.KeyboardData(KeyCode.F12));
    }

    private void RegisterRemix(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        // Register Remix menu
        orig(self);
        if (_isInit) return;
        _isInit = true;
        MachineConnector.SetRegisteredOI("alduris.wikiutil", _remixMenu);
    }

    private void ToolRunner(On.RainWorld.orig_Update orig, RainWorld self)
    {
        if (self.processManager != null)
        {
            var currProc = self.processManager.currentMainLoop;
            bool regularUpdate = currProc != null && (currProc.myTimeStacker + currProc.framesPerSecond * Time.deltaTime) > 1f;

            foreach (var (type, tool) in ToolDatabase.GetToolOrder())
            {
                if (ToolDatabase.CheckKeybindPressedFor(type) && tool.ShouldIRun(self))
                {
                    tool.Run(self, regularUpdate);
                }
            }
            orig(self);
        }
        else
        {
            orig(self);
        }
    }
}
