using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace WikiUtil.Remix
{
    internal class RemixMenu : OptionInterface
    {
        public static RemixMenu instance = null;
        public RemixMenu()
        {
            instance = this;
            if (DirectoryConfig == null)
            {
                string contents;
                if (File.Exists(DirectorySavePath) && Directory.Exists(contents = File.ReadAllText(DirectorySavePath)))
                {
                    DirectoryConfig = contents;
                }
                else
                {
                    DirectoryConfig = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Configurables and its internal API

        private static readonly string DirectorySavePath = Path.Combine(Application.persistentDataPath, "wikiutil.txt");
        internal static string DirectoryConfig = null;

        internal static readonly Dictionary<ToolDatabase.ToolType, Configurable<bool>> EnabledConfig = [];
        internal static readonly Dictionary<ToolDatabase.ToolType, Configurable<KeyCode>> KeycodeConfig = [];
        internal static readonly Dictionary<ToolDatabase.ToolType, Configurable<bool>> ControlConfig = [];
        internal static readonly Dictionary<ToolDatabase.ToolType, Configurable<bool>> AltConfig = [];
        internal static readonly Dictionary<ToolDatabase.ToolType, Configurable<bool>> ShiftConfig = [];

        private static readonly Dictionary<ToolDatabase.ToolType, ToolDatabase.KeyboardData> KeyboardDataCache = [];
        public static void UpdateCachedKeybinds()
        {
            HashSet<ToolDatabase.ToolType> toAdd = [.. KeycodeConfig.Keys.Intersect(ControlConfig.Keys).Intersect(AltConfig.Keys).Intersect(ShiftConfig.Keys).Intersect(EnabledConfig.Keys)];
            KeyboardDataCache.Clear();

            foreach (ToolDatabase.ToolType t in toAdd)
            {
                KeyboardDataCache.Add(t, new ToolDatabase.KeyboardData
                {
                    keyCode = KeycodeConfig[t].Value,
                    ctrl = ControlConfig[t].Value,
                    alt = AltConfig[t].Value,
                    shift = ShiftConfig[t].Value,
                });
            }
            ToolDatabase.RegenerateToolOrder();
        }

        public static void UpdateCacheFor(ToolDatabase.ToolType t)
        {
            KeyboardDataCache[t] = new ToolDatabase.KeyboardData
            {
                keyCode = KeycodeConfig[t].Value,
                ctrl = ControlConfig[t].Value,
                alt = AltConfig[t].Value,
                shift = ShiftConfig[t].Value,
            };
            ToolDatabase.RegenerateToolOrder();
        }

        public static ToolDatabase.KeyboardData? GetKeybindFor(ToolDatabase.ToolType toolType)
        {
            if (KeyboardDataCache.TryGetValue(toolType, out var keyboardData) && EnabledConfig.TryGetValue(toolType, out var enabled) && enabled.Value)
                return keyboardData;
            return null;
        }

        public static bool TryRegisterKeybind(ToolDatabase.ToolType toolType, ToolDatabase.KeyboardData keybind)
        {
            if (KeycodeConfig.ContainsKey(toolType) && ControlConfig.ContainsKey(toolType) && AltConfig.ContainsKey(toolType) && ShiftConfig.ContainsKey(toolType))
                return false;

            string toolName = Regex.Replace(toolType.ToString(), "[^\\w\\d_]", "_");
            KeycodeConfig[toolType] = instance.config.Bind($"kb_{toolName}", keybind.keyCode);
            ControlConfig[toolType] = instance.config.Bind($"ctrl_{toolName}", keybind.ctrl);
            AltConfig[toolType] = instance.config.Bind($"alt_{toolName}", keybind.alt);
            ShiftConfig[toolType] = instance.config.Bind($"shift_{toolName}", keybind.shift);
            EnabledConfig[toolType] = instance.config.Bind($"enabled_{toolName}", true);
            UpdateCachedKeybinds();

            return true;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Actual remix menu stuff

        public override void Initialize()
        {
            base.Initialize();
            Tabs = [
                new ToolTab(this),
                new DirTab(this),
            ];
            foreach (var tab in Tabs)
            {
                if (tab is Tab t) t.Initialize();
            }
        }

        public override void Update()
        {
            base.Update();
            foreach (var tab in Tabs)
            {
                if (tab is Tab t) t.Update();
            }
        }

        public override string ValidationString()
        {
            List<string> baseModTools =
            [
                "Screenshotter"
            ];
            List<int> validationInts = [];
            for (int i = 0; i < baseModTools.Count; i++)
            {
                if (i % 4 == 0) validationInts.Add(0);
                if (EnabledConfig.TryGetValue(new ToolDatabase.ToolType(baseModTools[i], false), out var enabled) && enabled.Value)
                {
                    validationInts[validationInts.Count - 1] |= 1 << (i % 4);
                }
            }

            // Built-in tools
            StringBuilder sb = new();
            for (int i = 0; i < validationInts.Count; i++)
            {
                sb.Append(validationInts[i].ToString("X"));
                if ((validationInts.Count - i - 1) % 4 == 0 && i != validationInts.Count - 1) sb.Append(' ');
            }

            // Extra tools
            var extraModTools = EnabledConfig.Keys.Select(x => x.ToString()).Except(baseModTools);
            foreach (var extra in extraModTools)
            {
                sb.Append(' ');
                sb.Append(extra);
            }

            return sb.ToString();
        }
    }
}
