using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using WikiUtil.BuiltIn;

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
        private static string _DirectoryConfigBackingField = null;
        internal static string DirectoryConfig
        {
            get => _DirectoryConfigBackingField;
            set
            {
                if (_DirectoryConfigBackingField != value && _DirectoryConfigBackingField != null && value != null)
                {
                    File.WriteAllText(DirectorySavePath, value);
                }
                _DirectoryConfigBackingField = value;
            }
        }

        internal static readonly Dictionary<string, Configurable<bool>> EnabledConfig = [];
        internal static readonly Dictionary<string, Configurable<KeyCode>> KeycodeConfig = [];
        internal static readonly Dictionary<string, Configurable<bool>> ControlConfig = [];
        internal static readonly Dictionary<string, Configurable<bool>> AltConfig = [];
        internal static readonly Dictionary<string, Configurable<bool>> ShiftConfig = [];

        public static void RegisterKeybind(string toolID, Keybind keybind)
        {
            string toolName = Regex.Replace(toolID, "[^\\w\\d_]", "_").ToLowerInvariant();
            KeycodeConfig[toolID] = instance.config.Bind($"kb_{toolName}", keybind.keyCode);
            ControlConfig[toolID] = instance.config.Bind($"ctrl_{toolName}", keybind.ctrl);
            AltConfig[toolID] = instance.config.Bind($"alt_{toolName}", keybind.alt);
            ShiftConfig[toolID] = instance.config.Bind($"shift_{toolName}", keybind.shift);
            EnabledConfig[toolID] = instance.config.Bind($"enabled_{toolName}", true);

            KeycodeConfig[toolID].OnChange += ConfigOnChange;
            ControlConfig[toolID].OnChange += ConfigOnChange;
            AltConfig[toolID].OnChange += ConfigOnChange;
            ShiftConfig[toolID].OnChange += ConfigOnChange;
            EnabledConfig[toolID].OnChange += ConfigOnChange;

            UpdateKeybind(toolID);
        }

        private static void ConfigOnChange()
        {
            foreach (var toolID in ToolDatabase.GetToolIDs())
            {
                UpdateKeybind(toolID);
            }
        }

        private static void UpdateKeybind(string toolID)
        {
            Keybind keybind = new(KeycodeConfig[toolID].Value, ControlConfig[toolID].Value, AltConfig[toolID].Value, ShiftConfig[toolID].Value);
            bool enabled = EnabledConfig[toolID].Value;
            ToolDatabase.UpdateTool(toolID, keybind, enabled);
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
                ScreenshotterTool.TOOL_ID,
                IconsTool.TOOL_ID,
                DecryptionTool.TOOL_ID,
                RegionScannerTool.TOOL_ID,
                MusicRecordsTool.TOOL_ID,
                PauseTool.TOOL_ID,
                TokenFinderTool.TOOL_ID,
                StencilTool.TOOL_ID,
            ];
            List<int> validationInts = [];
            for (int i = 0; i < baseModTools.Count; i++)
            {
                if (i % 4 == 0) validationInts.Add(0);
                if (EnabledConfig.TryGetValue(baseModTools[i], out var enabled) && enabled.Value)
                {
                    validationInts[validationInts.Count - 1] |= 1 << (i % 4);
                }
            }

            // Built-in tools
            StringBuilder sb = new();
            for (int i = 0; i < validationInts.Count; i++)
            {
                sb.Append(validationInts[i].ToString("X"));
                if (validationInts.Count > 6 && (validationInts.Count - i - 1) % 4 == 0 && i != validationInts.Count - 1)
                    sb.Append(' ');
            }

            // Extra tools
            var extraModTools = EnabledConfig.Keys.Select(x => x.ToString()).Except(baseModTools);
            foreach (var extra in extraModTools)
            {
                sb.Append(", ");
                sb.Append(extra);
            }

            return $"{ValidationString_ID()} {sb}";
        }
    }
}
