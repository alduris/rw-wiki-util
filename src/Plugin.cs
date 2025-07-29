﻿using System;
using System.Security.Permissions;
using BepInEx;
using BepInEx.Logging;
using WikiUtil.BuiltIn;
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

    private void OnEnable()
    {
        Logger = base.Logger;
        _remixMenu ??= new RemixMenu();
        On.RainWorld.OnModsInit += RegisterRemix;
        On.RainWorld.Update += ToolRunner;

        // Register our stuff
        try
        {
            ToolDatabase.RegisterTool(new ScreenshotterTool());
            ToolDatabase.RegisterTool(new IconsTool());
            ToolDatabase.RegisterTool(new DecryptionTool());
            // creature/object reader in a region
        }
        catch (Exception e)
        {
            Logger.LogError(e);
        }
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
            bool skipOrig = ToolDatabase.RunUpdateLoop(self);
            if (!skipOrig)
            {
                orig(self);
            }
        }
        else
        {
            orig(self);
        }
    }

    private void OnGUI()
    {
        try
        {
            ToolDatabase.RunGUI();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
}
