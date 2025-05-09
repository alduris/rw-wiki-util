using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WikiUtil.Remix
{
    internal class RemixMenu : OptionInterface
    {
        public static RemixMenu instance = null;
        public RemixMenu()
        {
            instance = this;
        }

        private static readonly Dictionary<ToolDatabase.ToolType, Configurable<KeyCode>> KeycodeConfig = [];
        private static readonly Dictionary<ToolDatabase.ToolType, Configurable<bool>> ControlConfig = [];
        private static readonly Dictionary<ToolDatabase.ToolType, Configurable<bool>> AltConfig = [];
        private static readonly Dictionary<ToolDatabase.ToolType, Configurable<bool>> ShiftConfig = [];

        private static readonly Dictionary<ToolDatabase.ToolType, ToolDatabase.KeyboardData> KeyboardDataCache = [];
        public static void UpdateCachedKeybinds()
        {
            HashSet<ToolDatabase.ToolType> toAdd = [.. KeycodeConfig.Keys.Intersect(ControlConfig.Keys).Intersect(AltConfig.Keys).Intersect(ShiftConfig.Keys)];
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
        }

        public static ToolDatabase.KeyboardData? GetKeybindFor(ToolDatabase.ToolType toolType)
        {
            if (KeyboardDataCache.TryGetValue(toolType, out var keyboardData))
                return keyboardData;
            return null;
        }

        public static bool TryRegisterKeybind(ToolDatabase.ToolType toolType, ToolDatabase.KeyboardData keybind)
        {
            if (KeycodeConfig.ContainsKey(toolType) && ControlConfig.ContainsKey(toolType) && AltConfig.ContainsKey(toolType) && ShiftConfig.ContainsKey(toolType))
                return false;

            KeycodeConfig[toolType] = new Configurable<KeyCode>(instance, $"kb_{toolType}", keybind.keyCode, null);
            ControlConfig[toolType] = new Configurable<bool>(instance, $"ctrl_{toolType}", keybind.ctrl, null);
            AltConfig[toolType] = new Configurable<bool>(instance, $"alt_{toolType}", keybind.alt, null);
            ShiftConfig[toolType] = new Configurable<bool>(instance, $"shift_{toolType}", keybind.shift, null);
            UpdateCachedKeybinds();

            return true;
        }
    }
}
