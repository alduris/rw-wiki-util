﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using WikiUtil.Remix;

namespace WikiUtil
{
    public static class ToolDatabase
    {
        public class ToolType(string value, bool register = false) : ExtEnum<ToolType>(value, register)
        {
            public static IEnumerable<ToolType> Values => values.entries.Select(x => new ToolType(x, false));
        }

        public struct KeyboardData
        {
            public KeyboardData(KeyCode keyCode)
            {
                this.keyCode = keyCode;
                ctrl = alt = shift = false;
            }

            public KeyCode keyCode;
            public bool ctrl;
            public bool alt;
            public bool shift;
        }

        public static bool CheckKeybindPressedFor(ToolType toolType)
        {
            KeyboardData? data = RemixMenu.GetKeybindFor(toolType);
            if (data.HasValue)
            {
                var keybind = data.Value;
                return Input.GetKeyDown(keybind.keyCode)
                    && !((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) ^ keybind.ctrl)
                    && !((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) ^ keybind.alt)
                    && !((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ^ keybind.shift);
            }
            return false;
        }

        private static int _cachedToolTypeVersion = -1;
        private static int _cachedToolTypeLength = -1;
        private static readonly List<KeyValuePair<ToolType, ITool>> _toolOrder = [];
        internal static List<KeyValuePair<ToolType, ITool>> GetToolOrder()
        {
            if (ToolType.valuesVersion != _cachedToolTypeVersion || ToolType.values.Count != _cachedToolTypeLength)
            {
                RegenerateToolOrder();
                _cachedToolTypeVersion = ToolType.valuesVersion;
                _cachedToolTypeLength = ToolType.values.Count;
            }
            return _toolOrder;
        }

        public static ITool GetToolFor(ToolType toolType)
        {
            return _toolOrder.FirstOrDefault(x => x.Key == toolType).Value;
        }

        public static bool ToolEnabled(ToolType toolType) => RemixMenu.EnabledConfig.TryGetValue(toolType, out var enabled) && enabled.Value;

        internal static void RegenerateToolOrder()
        {
            _toolOrder.OrderByDescending(ToolOrdering);

            static int B2I(bool value) => value ? 1 : 0;

            static int ToolOrdering(KeyValuePair<ToolType, ITool> item)
            {
                // This prioritizes keybinds with ctrl, alt, and/or shift from being run before versions of the keybinds that don't run with them
                var kb = RemixMenu.GetKeybindFor(item.Key);
                return kb is not null ? B2I(kb.Value.ctrl) + B2I(kb.Value.shift) + B2I(kb.Value.alt) + B2I(kb.Value.keyCode != KeyCode.None) * 3 : -1;
            }
        }

        /// <summary>
        /// Registers a tool with the mod
        /// </summary>
        /// <param name="type">Enum value to associate with the tool</param>
        /// <param name="tool">Instance of the actual tool to act upon</param>
        /// <param name="defaultKeybind">Default keybind to display in the Remix menu</param>
        public static void RegisterTool(ToolType type, ITool tool, KeyboardData defaultKeybind)
        {
            if (RemixMenu.TryRegisterKeybind(type, defaultKeybind))
            {
                _toolOrder.Add(new KeyValuePair<ToolType, ITool>(type, tool));
                RegenerateToolOrder();
            }
        }

        /// <summary>
        /// Gets path to specified location in user's selected work folder.
        /// </summary>
        /// <param name="paths">The relative parts of the path. Will automatically combine with the proper path separating character.</param>
        /// <returns>The full path starting from the user's selected work folder</returns>
        public static string GetPathTo(params string[] paths)
        {
            var path = Path.Combine(RemixMenu.DirectoryConfig, Path.Combine(paths));
            var dir = Path.GetDirectoryName(path);
            Directory.CreateDirectory(dir);
            return path;
        }
    }
}
