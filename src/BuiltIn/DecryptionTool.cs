using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Menu.Remix;
using RWCustom;
using UnityEngine;
using WikiUtil.Tools;

namespace WikiUtil.BuiltIn
{
    internal class DecryptionTool : GUIToggleTool
    {
        internal const string TOOL_ID = "Text File Decryption";

        public DecryptionTool() : base(TOOL_ID, new Keybind(KeyCode.D, true, false, false))
        {
        }

        private List<DirectoryRepresentation> modDirectories;

        public override void ToggleOn(RainWorld rainWorld)
        {
            var mods = ModManager.ActiveMods.Select(x => new ModRepresentation(x));
            modDirectories = [new BaseGameRepresentation()];
            modDirectories.AddRange(mods);
            modDirectories = [.. modDirectories.OrderBy(x => x.name, StringComparer.OrdinalIgnoreCase)];
            fileContents = "Open a file to read its contents";
        }

        public override void ToggleOff(RainWorld rainWorld)
        {
            modDirectories.Clear();
            modDirectories = null;
        }

        private const float leftWidth = 400f;
        private const float rightWidth = 300f;
        private const float contentMargin = 10f;
        public override Rect WindowSize { get; set; } = new Rect(100f, 100f, leftWidth + rightWidth + contentMargin * 3, 530f);

        private Vector2 dirPickerScrollOffset;
        private Vector2 textScrollOffset;

        private string fileContents;

        public override void OnGUI(RainWorld rainWorld)
        {
            // Directory list
            int itemsToShow = modDirectories.Sum(x => x.HeightContribution);
            dirPickerScrollOffset = GUI.BeginScrollView(new Rect(contentMargin, 20f, leftWidth, 500f), dirPickerScrollOffset, new Rect(0f, 0f, leftWidth - 20f, 24f * itemsToShow - 4f), false, true);

            float y = 0f;
            try
            {
                RenderDirectoryLevel(modDirectories, 0, ref y);
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError(e);
            }

            GUI.EndScrollView();

            // File contents
            float textHeight = GUI.skin.label.CalcHeight(new GUIContent(fileContents), rightWidth - 20f);
            textScrollOffset = GUI.BeginScrollView(new Rect(contentMargin * 2 + leftWidth, 20f, rightWidth, 470f), textScrollOffset, new Rect(0f, 0f, rightWidth - 20f, textHeight));

            GUI.Label(new Rect(0f, 0f, rightWidth - 20f, textHeight), fileContents);

            GUI.EndScrollView();

            if (GUI.Button(new Rect(contentMargin * 2 + leftWidth, 500f, rightWidth, 20f), "Copy"))
            {
                UniClipboard.SetText(fileContents);
            }
        }

        private void RenderDirectoryLevel(List<DirectoryRepresentation> directories, int indentLevel, ref float y)
        {
            float indentWidth = 24f * indentLevel;
            float textSpace = leftWidth - 20f - indentWidth;

            float innerIndentWidth = indentWidth + 24f;
            float innerTextSpace = leftWidth - 20f - innerIndentWidth;
            foreach (DirectoryRepresentation d in directories)
            {
                if (GUI.Button(new Rect(indentWidth, y, 20f, 20f), d.open ? "-" : "+"))
                {
                    d.ToggleOpen();
                }

                GUIStyle labelStyle = GUI.skin.label;
                if (d.openError)
                {
                    labelStyle.normal.textColor = Color.red;
                }
                GUI.Label(new Rect(indentWidth + 24f, y, textSpace, 20f), d.name, labelStyle);

                y += 24f;

                if (d.open && !d.openError)
                {
                    // Render subdirectories
                    RenderDirectoryLevel(d.subdirs, indentLevel + 1, ref y);

                    // Render files
                    foreach (var file in d.files)
                    {
                        if (GUI.Button(new Rect(innerIndentWidth, y, 48f, 20f), "Read"))
                        {
                            ReadFile(Path.Combine(d.DirPath, file));
                        }
                        GUI.Label(new Rect(innerIndentWidth + 52f, y, innerTextSpace - 52f, 20f), file);
                        y += 24f;
                    }
                }
            }
        }

        private void ReadFile(string path)
        {
            fileContents = "";
            textScrollOffset = Vector2.zero;

            try
            {
                string translatedContent = InGameTranslator.EncryptDecryptFile(path, false, true);
                if (translatedContent != null)
                {
                    fileContents = translatedContent;
                }
                else
                {
                    fileContents = File.ReadAllText(path);
                }
            }
            catch (Exception e)
            {
                fileContents = e.Message;
                Plugin.Logger.LogError(e);
            }
        }

        private class DirectoryRepresentation(string name, DirectoryRepresentation parent)
        {
            private static readonly HashSet<string> FORBIDDEN_SUBDIRS = [
                "world", "levels",  // prevent showing level files
                "decals", "palettes", "terrainpalettes", "illustrations", "scenes", "fairypresets", "projections",  // prevent image-exclusive folders
                "music", "loadedsoundeffects",  // sound/music folders
                "atlases", "assetbundles",  // asset collections that generally aren't ever human-readable text
                "plugins",  // code
                "mods", "aa", "eos",  // for base game folder
                "leditor", ".git"  // misc other weird exceptions
                ];

            public string name = name;
            public DirectoryRepresentation parent = parent;
            public List<DirectoryRepresentation> subdirs;
            public List<string> files;
            public bool openError = false;
            public bool open = false;
            private bool opened = false;

            public int HeightContribution => open && !openError ? subdirs.Sum(x => x.HeightContribution) + files.Count + 1 : 1;

            public virtual string DirPath => Path.Combine(parent.DirPath, name);

            public void ToggleOpen()
            {
                open = !open;
                if (!opened) Open();
            }

            protected void Open()
            {
                if (opened) return;

                try
                {
                    subdirs = [];
                    foreach (var fullDir in Directory.GetDirectories(DirPath))
                    {
                        string subdir = Path.GetFileName(fullDir);
                        if (!FORBIDDEN_SUBDIRS.Contains(subdir.ToLowerInvariant()))
                        {
                            subdirs.Add(new DirectoryRepresentation(subdir, this));
                        }
                    }

                    files = [];
                    foreach (var file in Directory.GetFiles(DirPath).Where(x => x.EndsWith(".txt")))
                    {
                        files.Add(Path.GetFileName(file));
                    }
                }
                catch (Exception e)
                {
                    openError = true;
                    Plugin.Logger.LogError(e);
                }

                opened = true;
            }
        }

        private class ModRepresentation : DirectoryRepresentation
        {
            public ModRepresentation(ModManager.Mod mod) : base(mod.name, null)
            {
                name = mod.name;
                this.mod = mod;
            }

            public ModManager.Mod mod;

            public override string DirPath => mod.basePath;
        }

        private class BaseGameRepresentation : DirectoryRepresentation
        {
            public BaseGameRepresentation() : base("Rain World", null) { }

            public override string DirPath => Custom.RootFolderDirectory();
        }
    }
}
