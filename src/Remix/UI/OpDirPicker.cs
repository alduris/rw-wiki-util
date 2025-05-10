using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace WikiUtil.Remix.UI
{
    /// <summary>
    /// Directory picker for remix menu.
    /// </summary>
    internal class OpDirPicker : OpScrollBox
    {
        private bool dirty = true;

        private DirectoryInfo _CurrentDirBackingField = null;
        public DirectoryInfo CurrentDir
        {
            get
            {
                return _CurrentDirBackingField;
            }
            set
            {
                _CurrentDirBackingField = value;
                RemixMenu.DirectoryConfig = value?.FullName ?? "C:\\";
            }
        }

        public bool ValidDir => CurrentDir != null;

        public OpDirPicker(OpTab tab) : base(tab, 0f, false, true)
        {
            _CurrentDirBackingField = new DirectoryInfo(RemixMenu.DirectoryConfig);
        }

        public OpDirPicker(Vector2 pos, Vector2 size) : base(pos, size, 0f, false, true, true)
        {
            this.size = new Vector2(Mathf.Max(160f, size.x), size.y);
            _CurrentDirBackingField = new DirectoryInfo(RemixMenu.DirectoryConfig);
        }

        public override void Update()
        {
            base.Update();

            if (dirty)
            {
                dirty = false;

                // Clear out old items
                foreach (var item in items)
                {
                    item.Deactivate();
                    item.tab._RemoveItem(item);
                }
                items.Clear();
                SetContentSize(0f);

                // Put head
                const float GAP = 6f;

                float y = size.y - GAP - 24f;
                float height = GAP * 4 + 26f;
                float fullWidth = size.x - 2 * GAP - 20f;

                var upButton = new OpSimpleImageButton(new Vector2(GAP, y), new Vector2(24f, 24f), "Menu_Symbol_Arrow");
                var refreshButton = new OpSimpleImageButton(new Vector2(GAP * 2 + 24f, y), new Vector2(24f, 24f), "Menu_Symbol_Repeats");
                var doneButton = new OpSimpleImageButton(new Vector2(size.x - GAP - 25f - 20f, y), new Vector2(24f, 24f), "Menu_Symbol_CheckBox");
                var pathInput = new OpTextBox(
                    new Configurable<string>(RemixMenu.instance, null, CurrentDir?.FullName ?? "\\", null),
                    new Vector2(GAP * 3 + 24f * 2, y),
                    size.x - GAP * 5 - 24f * 3 - 20f)
                    { maxLength = 65535 };

                upButton.OnClick += UpButton_OnClick;
                refreshButton.OnClick += RefreshButton_OnClick;
                doneButton.OnClick += (_) => DoneButton_OnClick(pathInput);

                y -= GAP + 2f;

                AddItems(
                    upButton,
                    refreshButton,
                    doneButton,
                    pathInput,
                    new OpImage(new(GAP, y), "pixel") { scale = new Vector2(fullWidth, 2f), color = colorEdge }
                    );

                y -= GAP + 24f;

                // Get directory contents
                Dictionary<string, DirectoryInfo> map = [];
                bool errored = false;
                do
                {
                    if (CurrentDir == null)
                    {
                        foreach (var item in Directory.GetLogicalDrives())
                        {
                            map.Add(item, new DirectoryInfo(item));
                        }
                    }
                    else
                    {
                        try
                        {
                            foreach (var item in CurrentDir.GetDirectories())
                            {
                                map.Add(item.Name, item);
                            }
                        }
                        catch (UnauthorizedAccessException)
                        {
                            if (CurrentDir == CurrentDir.Root)
                            {
                                CurrentDir = null;
                            }
                            else
                            {
                                CurrentDir = CurrentDir.Parent;
                            }
                            errored = true;
                        }
                    }
                }
                while (errored);

                // Sort and put in the thingamabob
                var list = map.ToList();
                list.Sort(new Comparison<KeyValuePair<string, DirectoryInfo>>((a, b) => StringComparer.InvariantCultureIgnoreCase.Compare(a.Key, b.Key)));
                foreach (var kv in list)
                {
                    var dirButton = new OpTextButton(new Vector2(GAP, y), new Vector2(fullWidth, 24f), kv.Key) { alignment = FLabelAlignment.Left };
                    dirButton.OnClick += (_) => DirButton_OnClick(kv.Value);
                    AddItems(dirButton);
                    y -= 24f;
                    height += 24f;
                }

                // Set content size
                SetContentSize(height, true);
                ScrollToTop(true);
            }
        }

        private void DirButton_OnClick(DirectoryInfo dir)
        {
            var oldDir = CurrentDir;
            try
            {
                CurrentDir = dir;
                CurrentDir?.GetDirectories();
                dirty = true;
            }
            catch (Exception e)
            {
                CurrentDir = oldDir;
                PlaySound(SoundID.MENU_Error_Ping);
                Plugin.Logger.LogError(e);
            }
        }

        private void UpButton_OnClick(UIfocusable trigger)
        {
            if (CurrentDir == null)
            {
                PlaySound(SoundID.MENU_Error_Ping);
            }
            else
            {
                var oldDir = CurrentDir;
                try
                {
                    CurrentDir = CurrentDir.Parent;
                    CurrentDir?.GetDirectories();
                    dirty = true;
                }
                catch (Exception e)
                {
                    CurrentDir = oldDir;
                    PlaySound(SoundID.MENU_Error_Ping);
                    Plugin.Logger.LogError(e);
                }
            }
        }

        private void RefreshButton_OnClick(UIfocusable trigger)
        {
            dirty = true;
            CurrentDir.Refresh();
        }

        private void DoneButton_OnClick(OpTextBox textbox)
        {
            var oldDir = CurrentDir;
            try
            {
                CurrentDir = Directory.CreateDirectory(textbox.value);
                dirty = true;
            }
            catch (Exception e)
            {
                CurrentDir = oldDir;
                PlaySound(SoundID.MENU_Error_Ping);
                Plugin.Logger.LogError(e);
            }
        }
    }
}
