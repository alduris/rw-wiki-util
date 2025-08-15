using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using WikiUtil.Tools;

namespace WikiUtil.BuiltIn
{
    internal class CorniferTool : GUIToggleTool
    {
        internal const string TOOL_ID = "Cornifer File Generator";
        public CorniferTool() : base(TOOL_ID, new Keybind(KeyCode.C, true, false, true)) { }

        private string[] slugcats;
        private string[] regions;
        private int slugcatPickerValue;
        private int regionPickerValue;
        private readonly Dictionary<(int slug, int region), WorldLoader> worldLoaders = [];
        private WorldLoader CurrWorldLoader => worldLoaders.TryGetValue(worldLoaderSelected, out var wl) ? wl : null;
        private (int slug, int region) worldLoaderSelected = (-1, -1);

        public override void ToggleOn(RainWorld rainWorld)
        {
            slugcats = [.. SlugcatStats.Name.values.entries];
            slugcatPickerValue = -1;
            regions = [.. Region.GetFullRegionOrder().OrderBy(x => x, StringComparer.OrdinalIgnoreCase)];
            regionPickerValue = -1;
            worldLoaders.Clear();
            worldLoaderSelected = (-1, -1);
        }

        public override void ToggleUpdate(RainWorld rainWorld)
        {
            var worldLoader = CurrWorldLoader;
            if (worldLoader != null && !worldLoader.Finished)
            {
                for (int i = 0; i < 100 && !worldLoader.Finished; i++)
                {
                    worldLoader.Update();
                }
            }
        }

        public override void ToggleOff(RainWorld rainWorld)
        {
            worldLoaders.Clear();
            worldLoaderSelected = (-1, -1);
        }

        private const float contentMargin = 10f;
        private const float contentWidth = 300f;
        private const float slugcatSelectHeight = 100f;
        private const float regionSelectHeight = 300f;
        public override Rect WindowSize { get; set; } = new Rect(100f, 100f, contentMargin * 2 + contentWidth, 20f + 24f + slugcatSelectHeight + contentMargin + 24f + regionSelectHeight + contentMargin + 24f + contentMargin);

        private Vector2 slugcatSelectScroll;
        private Vector2 regionSelectScroll;

        public override void OnGUI(RainWorld rainWorld)
        {
            // Precompute some numbers
            const int SLUGCATCOLS = 3;
            const int REGIONCOLS = 4;

            int slugcatRows = Mathf.CeilToInt((float)slugcats.Length / SLUGCATCOLS);
            int regionRows = Mathf.CeilToInt((float)regions.Length / REGIONCOLS);
            float slugcatSelectInnerHeight = 24f * slugcatRows - 4f;
            float regionSelectInnerHeight = 24f * regionRows - 4f;

            // Slugcat select
            GUI.Label(new Rect(contentMargin, 20f, contentWidth, 24f), "Slugcat");
            slugcatSelectScroll = GUI.BeginScrollView(new Rect(contentMargin, 20f + 24f, contentWidth, slugcatSelectHeight), slugcatSelectScroll, new Rect(0f, 0f, contentWidth - 20f, slugcatSelectInnerHeight));
            slugcatPickerValue = GUI.SelectionGrid(new Rect(0f, 0f, contentWidth - 20f, slugcatSelectInnerHeight), slugcatPickerValue, slugcats, SLUGCATCOLS);
            GUI.EndScrollView();

            // Region select
            const float regionStartY = 20f + 24f + slugcatSelectHeight + contentMargin;
            GUI.Label(new Rect(contentMargin, regionStartY, contentWidth, 24f), "Region");
            regionSelectScroll = GUI.BeginScrollView(new Rect(contentMargin, regionStartY + 24f, contentWidth, regionSelectHeight), regionSelectScroll, new Rect(0f, 0f, contentWidth - 20f, regionSelectInnerHeight));
            regionPickerValue = GUI.SelectionGrid(new Rect(0f, 0f, contentWidth - 20f, regionSelectInnerHeight), regionPickerValue, regions, REGIONCOLS);
            GUI.EndScrollView();

            // Update world loader
            if (worldLoaderSelected != (slugcatPickerValue, regionPickerValue) && slugcatPickerValue > -1 && regionPickerValue > -1)
            {
                var slugcat = new SlugcatStats.Name(slugcats[slugcatPickerValue], false);
                var newLoader = new WorldLoader(null, slugcat, SlugcatStats.SlugcatToTimeline(slugcat), false, regions[regionPickerValue], null, rainWorld.setup, WorldLoader.LoadingContext.FULL);
                worldLoaders.Add(worldLoaderSelected, newLoader);
            }

            // Button
            const float buttonStartY = regionStartY + 24f + regionSelectHeight + contentMargin;
            var worldLoader = CurrWorldLoader;
            if (worldLoader == null)
            {
                GUI.Label(new Rect(contentMargin, buttonStartY, contentWidth, 24f), "Select a slugcat and a region first");
            }
            else if (!worldLoader.Finished)
            {
                GUI.Label(new Rect(contentMargin, buttonStartY, contentWidth, 24f), "Loading region...");
            }
            else if (GUI.Button(new Rect(contentMargin, buttonStartY, contentWidth, 24f), "Download"))
            {
                GenerateCorniferFile();
            }
        }

        private void GenerateCorniferFile()
        {
            // https://github.com/enchanted-sword/Cornifer/blob/master/Main.cs#L870
            string output = ToolDatabase.GetPathTo("cornifer", $"{regions[regionPickerValue]}_{slugcats[slugcatPickerValue]}.cornimap");

            Dictionary<string, object> saveData = [];
            saveData["slugcat"] = slugcats[slugcatPickerValue];
            saveData["installFeatures"] = CorniferToolHelper.ReturnInstallFeatures();
            saveData["region"] = CorniferToolHelper.ReturnRegionJson(CurrWorldLoader.ReturnWorld(), slugcats[slugcatPickerValue]);

            File.WriteAllText(output, Json.Serialize(saveData));
        }
    }
}
