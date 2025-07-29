using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RWCustom;
using UnityEngine;
using WikiUtil.Remix;
using WikiUtil.Tools;

namespace WikiUtil
{
    public static class ToolDatabase
    {
        private struct ToolInfo
        {
            public Keybind keybind;
            public bool enabled;
        }

        private static int windowIDCounter = 0;
        private static readonly List<Tool> tools = [];
        private static readonly Dictionary<string, ToolInfo> toolInfos = [];
        private static readonly Dictionary<IHaveGUI, int> guiToID = [];
        private static readonly Dictionary<int, IHaveGUI> idToGUI = [];

        public static void RegisterTool(Tool tool)
        {
            tools.Add(tool);
            if (!toolInfos.ContainsKey(tool.id))
            {
                toolInfos.Add(tool.id, new ToolInfo { keybind = tool.defaultKeybind, enabled = true });
            }
            if (tool is IHaveGUI guiHaver)
            {
                int id = windowIDCounter++;
                guiToID.Add(guiHaver, id);
                idToGUI.Add(id, guiHaver);
            }
            RemixMenu.RegisterKeybind(tool.id, tool.defaultKeybind);
        }

        internal static Keybind GetKeybind(string id) => toolInfos[id].keybind;
        internal static bool CheckEnabled(string id) => toolInfos[id].enabled;
        internal static void UpdateTool(string id, Keybind keybind, bool enabled) => toolInfos[id] = new ToolInfo { keybind = keybind, enabled = enabled };
        internal static IEnumerable<string> GetToolIDs() => tools.Select(x => x.id);


        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        internal static bool RunUpdateLoop(RainWorld rainWorld)
        {
            bool skipOrig = false;
            foreach (var tool in tools)
            {
                if (toolInfos[tool.id].enabled)
                {
                    try
                    {
                        tool.Update(rainWorld);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
            return skipOrig;
        }

        internal static void RunGUI()
        {
            foreach (var tool in tools)
            {
                if (toolInfos[tool.id].enabled && tool is IHaveGUI guiHaver && guiHaver.ShowWindow)
                {
                    guiHaver.WindowSize = GUI.Window(guiToID[guiHaver], guiHaver.WindowSize, DoWindow, tool.id);
                }
            }
        }

        internal static void DoWindow(int windowID)
        {
            IHaveGUI guiHaver = idToGUI[windowID];
            guiHaver.OnGUI(Custom.rainWorld);
            GUI.DragWindow(new Rect(0f, 0f, 10000f, 20f));
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

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
