using BepInEx;
using BepInEx.Logging;
using System.Security.Permissions;
using UnityEngine;
using WikiUtil.Remix;

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
        On.RainWorld.OnModsInit += OnModsInit;
        On.RainWorld.Update += RainWorld_Update;
    }

    private void OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);

        if (_isInit) return;
        _isInit = true;

        MachineConnector.SetRegisteredOI("alduris.finder", _remixMenu);
    }

    private IControllingTool controllingTool = null;
    private void RainWorld_Update(On.RainWorld.orig_Update orig, RainWorld self)
    {
        if (self.processManager != null)
        {
            var currProc = self.processManager.currentMainLoop;
            bool regularUpdate = currProc != null && (currProc.myTimeStacker + currProc.framesPerSecond * Time.deltaTime) > 1f;

            ITool currTool = controllingTool;
            if (!controllingTool.ShouldIRun(self) || !controllingTool.ShouldITakeControl(self))
            {
                controllingTool = null;
                currTool = null;
            }
            if (currTool == null)
            {
                foreach (var (type, tool) in ToolDatabase.GetToolOrder())
                {
                    if (ToolDatabase.CheckKeybindPressedFor(type) && tool.ShouldIRun(self))
                    {
                        if (tool is IControllingTool cTool && cTool.ShouldITakeControl(self))
                        {
                            controllingTool = cTool;
                            tool.Run(self, regularUpdate);
                            break;
                        }
                        else
                        {
                            tool.Run(self, regularUpdate);
                        }
                    }
                }
            }

            if (controllingTool == null || !controllingTool.ShouldTheGameStillRun(self))
            {
                orig(self);
            }
        }
        else
        {
            orig(self);
        }
    }
}
