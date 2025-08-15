using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Watcher;
using WikiUtil.Tools;

namespace WikiUtil.BuiltIn
{
    internal class TokenFinderTool : GUIToggleTool
    {
        internal const string TOOL_ID = "Token Scanner"; // probably better named something with "Points of Interest" but I can't figure out how to make that sound cool

        private static readonly HashSet<string> defaultSlugcats = ["White", "Yellow", "Red", "Gourmand", "Artificer", "Rivulet", "Spear", "Saint", "Inv", "Watcher"];

        public TokenFinderTool() : base(TOOL_ID, new Keybind(KeyCode.T, true, false, false))
        {
        }

        private bool hasInitializedSlugcats = false;
        private Dictionary<string, RegionRepresentation> representations = [];
        private string[] regions = [];
        private string[] slugcats = [];
        private bool[] slugcatsEnabled = [];

        private RegionRepresentation ActiveRegionRepresentation => regionPickerValue > -1 && representations.TryGetValue(regions[regionPickerValue], out var r) ? r : null;
        private int CountSlugcatsEnabled => slugcatsEnabled.Count(x => x);

        public override void ToggleOn(RainWorld rainWorld)
        {
            regions = [.. Region.GetFullRegionOrder().OrderBy(x => x, StringComparer.OrdinalIgnoreCase)];
            regionPickerValue = -1;

            if (hasInitializedSlugcats)
            {
                int oldSlugCount = slugcats.Length;
                HashSet<string> oldSlugsEnabled = [];
                for (int i = 0; i < slugcats.Length; i++)
                {
                    if (slugcatsEnabled[i])
                    {
                        oldSlugsEnabled.Add(slugcats[i]);
                    }
                }
                slugcats = [.. SlugcatStats.Name.values.entries];
                slugcatsEnabled = new bool[slugcats.Length];
                for (int i = 0; i < slugcats.Length; i++)
                {
                    slugcatsEnabled[i] = oldSlugsEnabled.Contains(slugcats[i]);
                }

                // Also check if we need to reset representations (we do if slugcat count has changed)
                if (oldSlugCount != slugcats.Length)
                {
                    representations.Clear();
                }
            }
            else
            {
                // Initialize slugcats array with default slugcats
                hasInitializedSlugcats = true;
                slugcats = [.. SlugcatStats.Name.values.entries];
                slugcatsEnabled = new bool[slugcats.Length];
                for (int i = 0; i < slugcats.Length; i++)
                {
                    slugcatsEnabled[i] = defaultSlugcats.Contains(slugcats[i]);
                }
            }
        }

        public override void ToggleUpdate(RainWorld rainWorld)
        {
            foreach (var region in representations.Values)
            {
                region.Update();
            }
        }

        private const float innerTabWidth = 30f;
        private const float contentMargin = 10f;
        private const float regionPickerWidth = 200f;
        private const float rightAreaWidth = 450f;
        private const float rightAreaStartX = 2 * contentMargin + regionPickerWidth;
        private const float slugcatPickerHeight = 100f;
        private const float outputAreaStartY = 44f + slugcatPickerHeight + contentMargin;
        private const float outputAreaHeight = 400f;
        private const float contentHeight = slugcatPickerHeight + contentMargin + outputAreaHeight;

        public override Rect WindowSize { get; set; } = new Rect(100f, 100f, 3 * contentMargin + regionPickerWidth + rightAreaWidth, 44f + contentHeight + contentMargin);

        private Vector2 regionPickerScroll;
        private int regionPickerValue = -1;
        private Vector2 slugcatPickerScroll;
        private Vector2 outputAreaScroll;

        public override void OnGUI(RainWorld rainWorld)
        {
            // Warning label
            GUI.Label(new Rect(contentMargin, 20f, WindowSize.width - 2 * contentMargin, 24f), "WARNING: this tool is very resource intensive. Make sure you have the CPU and RAM for it.");

            // Region picker
            const int REGIONCOLS = 3;
            float regionPickerInnerHeight = 24f * Mathf.CeilToInt((float)regions.Length / REGIONCOLS) - 4f;
            regionPickerScroll = GUI.BeginScrollView(new Rect(contentMargin, 44f, regionPickerWidth, contentHeight), regionPickerScroll, new Rect(0f, 0f, regionPickerWidth - 20f, regionPickerInnerHeight));
            regionPickerValue = GUI.SelectionGrid(new Rect(0f, 0f, regionPickerWidth - 20f, regionPickerInnerHeight), regionPickerValue, regions, REGIONCOLS);
            GUI.EndScrollView();

            // Load selected region
            if (regionPickerValue > -1 && ActiveRegionRepresentation == null)
            {
                representations[regions[regionPickerValue]] = new RegionRepresentation(regions[regionPickerValue], this);
            }

            // Slugcat selection
            const int SLUGCATCOLS = 4;
            float slugcatPickerInnerHeight = 24f * Mathf.CeilToInt((float)slugcats.Length / SLUGCATCOLS) - 4f;
            slugcatPickerScroll = GUI.BeginScrollView(new Rect(rightAreaStartX, 44f, rightAreaWidth, slugcatPickerHeight), slugcatPickerScroll, new Rect(0f, 0f, rightAreaWidth - 20f, slugcatPickerInnerHeight));
            for (int i = 0; i < slugcats.Length; i++)
            {
                float x = rightAreaWidth / SLUGCATCOLS * (i % SLUGCATCOLS);
                float y = 24f * (i / SLUGCATCOLS);
                slugcatsEnabled[i] = GUI.Toggle(new Rect(x, y, rightAreaWidth / SLUGCATCOLS, 20f), slugcatsEnabled[i], slugcats[i]);
            }
            GUI.EndScrollView();

            // Data
            if (ActiveRegionRepresentation != null)
            {
                float height = ActiveRegionRepresentation.EstimatedHeight();
                if (height > 0f)
                {
                    outputAreaScroll = GUI.BeginScrollView(new Rect(rightAreaStartX, outputAreaStartY, rightAreaWidth, outputAreaHeight), outputAreaScroll, new Rect(0f, 0f, rightAreaWidth - 20f, height), false, true);
                    ActiveRegionRepresentation.Render();
                    GUI.EndScrollView();
                }
                else
                {
                    GUI.Label(new Rect(rightAreaStartX, outputAreaStartY, rightAreaWidth, 24f), "Loading...");
                    outputAreaScroll = Vector2.zero;
                }
            }
            else
            {
                GUI.Label(new Rect(rightAreaStartX, outputAreaStartY, rightAreaWidth, outputAreaHeight), "Pick a region");
            }
        }

        private readonly struct FoundItem<T>(string room, T item)
        {
            public readonly string room = room;
            public readonly T item = item;
        }

        private class WorldstateRepresentation
        {
            public WorldstateRepresentation(string acronym, SlugcatStats.Name slugcat)
            {
                this.slugcat = slugcat;
                timeline = SlugcatStats.SlugcatToTimeline(slugcat);
                var region = Region.LoadAllRegions(timeline, null).First(x => x.name == acronym);
                loader = new WorldLoader(null, slugcat, timeline, false, region.name, region, RainWorld.LoadSetupValues(true), WorldLoader.LoadingContext.FULL);
                loader.NextActivity();
            }

            public bool Ready => loader.Finished && hasScanned;

            private bool hasScanned = false;
            private readonly WorldLoader loader;
            private readonly SlugcatStats.Name slugcat;
            private readonly SlugcatStats.Timeline timeline;

            public void Update()
            {
                if (!loader.Finished)
                {
                    for (int i = 0; i < 100 && !Ready; i++)
                    {
                        loader.Update();
                    }
                }
                else if (!hasScanned)
                {
                    var world = loader.ReturnWorld();
                    foreach (var room in world.abstractRooms)
                    {
                        if (!world.DisabledMapRooms.Contains(room.name, StringComparer.OrdinalIgnoreCase))
                        {
                            ScanRoom(room);
                        }
                    }
                    hasScanned = true;
                }
            }

            // Things we can attach a specific enum to from the settings
            public readonly List<FoundItem<DataPearl.AbstractDataPearl.DataPearlType>> foundPearls = [];
            public readonly List<FoundItem<MultiplayerUnlocks.SandboxUnlockID>> foundSandboxUnlocks = [];
            public readonly List<FoundItem<MultiplayerUnlocks.LevelUnlockID>> foundArenaUnlocks = [];
            public readonly List<FoundItem<MultiplayerUnlocks.SafariUnlockID>> foundSafariUnlocks = [];
            public readonly List<FoundItem<MultiplayerUnlocks.SlugcatUnlockID>> foundSlugcatUnlocks = [];
            public readonly List<FoundItem<MoreSlugcats.ChatlogData.ChatlogID>> foundChatlogs = [];

            // Things which do not have associated enums that we can get easily
            public readonly List<string> foundDevLogs = [];
            public readonly List<string> foundUnknownTokens = [];
            public readonly List<string> foundEchoes = [];
            public readonly List<string> foundSpinningTops = [];
            public readonly List<string> foundWeavers = [];

            private void ScanRoom(AbstractRoom room)
            {
                var roomSettings = new RoomSettings(null, room.name, room.world.region, false, false, timeline, null);

                // Yippee now we load the object types
                foreach (var po in roomSettings.placedObjects)
                {
                    if (po.active)
                    {
                        if (po.data is PlacedObject.DataPearlData pearlData && !pearlData.hidden && TokenFinderToolHelper.IsAllowedPearl(pearlData.pearlType))
                        {
                            foundPearls.Add(new(room.name, pearlData.pearlType));
                        }
                        else if (po.data is CollectToken.CollectTokenData tokenData && tokenData.availableToPlayers.Contains(slugcat))
                        {
                            if (tokenData.SafariUnlock != null)
                            {
                                // have to do safari first due to an oversight in the game's code
                                foundSafariUnlocks.Add(new(room.name, tokenData.SafariUnlock));
                            }
                            else if (tokenData.SandboxUnlock != null)
                            {
                                foundSandboxUnlocks.Add(new(room.name, tokenData.SandboxUnlock));
                            }
                            else if (tokenData.LevelUnlock != null)
                            {
                                foundArenaUnlocks.Add(new(room.name, tokenData.LevelUnlock));
                            }
                            else if (tokenData.SlugcatUnlock != null)
                            {
                                foundSlugcatUnlocks.Add(new(room.name, tokenData.SlugcatUnlock));
                            }
                            else if (tokenData.ChatlogCollect != null)
                            {
                                foundChatlogs.Add(new(room.name, tokenData.ChatlogCollect));
                            }
                            else if (tokenData.isDev)
                            {
                                foundDevLogs.Add(room.name);
                            }
                            else
                            {
                                foundUnknownTokens.Add(room.name);
                            }
                        }
                        else if (po.type == PlacedObject.Type.GhostSpot || po.type.value == "EEGhostSpot")
                        {
                            foundEchoes.Add(room.name);
                        }
                        else if (ModManager.Watcher && po.type == WatcherEnums.PlacedObjectType.SpinningTopSpot)
                        {
                            foundSpinningTops.Add(room.name);
                        }
                        else if (ModManager.Watcher && po.type == WatcherEnums.PlacedObjectType.WeaverSpot)
                        {
                            foundWeavers.Add(room.name);
                        }
                    }
                }
            }
        }

        private class RegionRepresentation
        {
            public RegionRepresentation(string acronym, TokenFinderTool owner)
            {
                this.owner = owner;
                foreach (var name in SlugcatStats.Name.values.entries)
                {
                    var slugcat = new SlugcatStats.Name(name, false);
                    worldstates.Add(name, new WorldstateRepresentation(acronym, slugcat));
                }
            }

            private readonly TokenFinderTool owner;
            private readonly Dictionary<string, WorldstateRepresentation> worldstates = [];
            private int WorldstatesReady => worldstates.Count(x => x.Value.Ready);
            private int lastWorldstatesReady = 0;
            private int lastSlugcatsEnabled = 0;

            public void Update()
            {
                // Update world loaders
                foreach (var worldstate in worldstates.Values)
                {
                    worldstate.Update();
                    if (!worldstate.Ready) break;
                }

                // Aggregate data
                if (lastWorldstatesReady != WorldstatesReady || lastSlugcatsEnabled != owner.CountSlugcatsEnabled)
                {
                    lastWorldstatesReady = WorldstatesReady;
                    lastSlugcatsEnabled = owner.CountSlugcatsEnabled;

                    // Rebuild caches
                    aggregatedPearls = GetAggregatedItems(GetFieldsFromWorldstates(x => x.foundPearls));
                    aggregatedSandboxes = GetAggregatedItems(GetFieldsFromWorldstates(x => x.foundSandboxUnlocks));
                    aggregatedArenas = GetAggregatedItems(GetFieldsFromWorldstates(x => x.foundArenaUnlocks));
                    aggregatedSafaris = GetAggregatedItems(GetFieldsFromWorldstates(x => x.foundSafariUnlocks));
                    aggregatedChatlogs = GetAggregatedItems(GetFieldsFromWorldstates(x => x.foundChatlogs));
                    aggregatedDevlogs = GetAggregatedRooms(GetFieldsFromWorldstates(x => x.foundDevLogs));
                    aggregatedUnknownTokens = GetAggregatedRooms(GetFieldsFromWorldstates(x => x.foundUnknownTokens));
                    aggregatedEchoes = GetAggregatedRooms(GetFieldsFromWorldstates(x => x.foundEchoes));
                    aggregatedSpinningTops = GetAggregatedRooms(GetFieldsFromWorldstates(x => x.foundSpinningTops));
                    aggregatedWeavers = GetAggregatedRooms(GetFieldsFromWorldstates(x => x.foundWeavers));
                }
            }

            private List<AggregatedItem> aggregatedPearls = [];
            private List<AggregatedItem> aggregatedSandboxes = [];
            private List<AggregatedItem> aggregatedArenas = [];
            private List<AggregatedItem> aggregatedSafaris = [];
            private List<AggregatedItem> aggregatedChatlogs = [];
            private List<AggregatedRoom> aggregatedDevlogs = [];
            private List<AggregatedRoom> aggregatedUnknownTokens = [];
            private List<AggregatedRoom> aggregatedEchoes = [];
            private List<AggregatedRoom> aggregatedSpinningTops = [];
            private List<AggregatedRoom> aggregatedWeavers = [];

            private Dictionary<string, List<T>> GetFieldsFromWorldstates<T>(Func<WorldstateRepresentation, List<T>> getField)
            {
                Dictionary<string, List<T>> fields = [];
                foreach (var item in worldstates)
                {
                    fields.Add(item.Key, getField(item.Value));
                }
                return fields;
            }

            private List<AggregatedItem> GetAggregatedItems<T>(Dictionary<string, List<FoundItem<T>>> itemsFromWorldstates) where T : ExtEnum<T>
            {
                List<AggregatedItem> aggregates = [];

                foreach (var worldstate in itemsFromWorldstates)
                {
                    if (!owner.slugcatsEnabled[owner.slugcats.IndexOf(worldstate.Key)]) continue;
                    foreach (var foundItem in worldstate.Value)
                    {
                        bool match = false;
                        foreach (var aggregate in aggregates)
                        {
                            if (aggregate.item == foundItem.item.value && aggregate.room == foundItem.room)
                            {
                                aggregate.slugcats.Add(worldstate.Key);
                                match = true;
                                break;
                            }
                        }

                        if (!match)
                        {
                            aggregates.Add(new AggregatedItem(foundItem.item.value, foundItem.room, [worldstate.Key]));
                        }
                        else
                        {
                            continue;
                        }
                    }
                }

                return aggregates;
            }

            private List<AggregatedRoom> GetAggregatedRooms(Dictionary<string, List<string>> itemsFromWorldstates)
            {
                List<AggregatedRoom> aggregates = [];

                foreach (var worldstate in itemsFromWorldstates)
                {
                    if (!owner.slugcatsEnabled[owner.slugcats.IndexOf(worldstate.Key)]) continue;
                    foreach (var room in worldstate.Value)
                    {
                        bool match = false;
                        foreach (var aggregate in aggregates)
                        {
                            if (aggregate.room == room)
                            {
                                aggregate.slugcats.Add(worldstate.Key);
                                match = true;
                                break;
                            }
                        }

                        if (!match)
                        {
                            aggregates.Add(new AggregatedRoom(room, [worldstate.Key]));
                        }
                        else
                        {
                            continue;
                        }
                    }
                }

                return aggregates;
            }

            public float EstimatedHeight()
            {
                static int bool2int(bool b) => b ? 1 : 0;
                static float CalcHeightI(List<AggregatedItem> items) => items.Count * 20f + items.Sum(x => HeightOf(string.Join(", ", x.slugcats), rightAreaWidth - 20f - innerTabWidth));
                static float CalcHeightR(List<AggregatedRoom> items) => items.Count * 20f + items.Sum(x => HeightOf(string.Join(", ", x.slugcats), rightAreaWidth - 20f - innerTabWidth));

                int numGroups = bool2int(aggregatedPearls.Count > 0)
                              + bool2int(aggregatedSandboxes.Count > 0)
                              + bool2int(aggregatedArenas.Count > 0)
                              + bool2int(aggregatedSafaris.Count > 0)
                              + bool2int(aggregatedChatlogs.Count > 0)
                              + bool2int(aggregatedDevlogs.Count > 0)
                              + bool2int(aggregatedUnknownTokens.Count > 0)
                              + bool2int(aggregatedEchoes.Count > 0)
                              + bool2int(aggregatedSpinningTops.Count > 0)
                              + bool2int(aggregatedWeavers.Count > 0);

                return CalcHeightI(aggregatedPearls)
                     + CalcHeightI(aggregatedSandboxes)
                     + CalcHeightI(aggregatedArenas)
                     + CalcHeightI(aggregatedSafaris)
                     + CalcHeightI(aggregatedChatlogs)
                     + CalcHeightR(aggregatedDevlogs)
                     + CalcHeightR(aggregatedUnknownTokens)
                     + CalcHeightR(aggregatedEchoes)
                     + CalcHeightR(aggregatedSpinningTops)
                     + CalcHeightR(aggregatedWeavers)
                     + contentMargin * (numGroups - 1);
            }

            private static float HeightOf(string text, float width) => GUI.skin.label.CalcHeight(new GUIContent(text), width);

            public void Render()
            {
                float y = 0f;
                bool renderMargin = false;

                RenderAggregatedItems("Pearl", aggregatedPearls, ref y, ref renderMargin);
                RenderAggregatedItems("Sandbox unlock", aggregatedSandboxes, ref y, ref renderMargin);
                RenderAggregatedItems("Arena set", aggregatedArenas, ref y, ref renderMargin);
                RenderAggregatedItems("Safari region", aggregatedSafaris, ref y, ref renderMargin);
                RenderAggregatedItems("Chatlog", aggregatedChatlogs, ref y, ref renderMargin);
                RenderAggregatedRooms("Devlog", aggregatedDevlogs, ref y, ref renderMargin);
                RenderAggregatedRooms("Unknown token", aggregatedUnknownTokens, ref y, ref renderMargin);
                RenderAggregatedRooms("Echo", aggregatedEchoes, ref y, ref renderMargin);
                RenderAggregatedRooms("Spinning Top location", aggregatedSpinningTops, ref y, ref renderMargin);
                RenderAggregatedRooms("Void Weaver spot", aggregatedWeavers, ref y, ref renderMargin);
            }

            private void AddMargin(ref float y, ref bool shouldAdd)
            {
                if (shouldAdd)
                {
                    y += contentMargin;
                }
                shouldAdd = true;
            }

            private void RenderAggregatedItems(string frontText, List<AggregatedItem> items, ref float y, ref bool renderMargin)
            {
                const float width = rightAreaWidth - 20f;
                if (items.Count > 0)
                {
                    AddMargin(ref y, ref renderMargin);

                    foreach (var item in items)
                    {
                        GUI.Label(new Rect(0f, y, width, 24f), $"{frontText}: {item.item} ({item.room})");
                        y += 20f;
                        string slugcats = string.Join(", ", item.slugcats);
                        float height = HeightOf(slugcats, width - innerTabWidth);
                        GUI.Label(new Rect(innerTabWidth, y, width - innerTabWidth, height), slugcats);
                        y += height;
                    }
                }
            }

            private void RenderAggregatedRooms(string frontText, List<AggregatedRoom> items, ref float y, ref bool renderLine)
            {
                const float width = rightAreaWidth - 20f;
                if (items.Count > 0)
                {
                    AddMargin(ref y, ref renderLine);

                    foreach (var item in items)
                    {
                        GUI.Label(new Rect(0f, y, width, 24f), $"{frontText} ({item.room})");
                        y += 20f;
                        string slugcats = string.Join(", ", item.slugcats);
                        float height = HeightOf(slugcats, width - innerTabWidth);
                        GUI.Label(new Rect(innerTabWidth, y, width - innerTabWidth, height), slugcats);
                        y += height;
                    }
                }
            }
        }

        private readonly struct AggregatedItem(string item, string room, List<string> slugcats)
        {
            public readonly string item = item;
            public readonly string room = room;
            public readonly List<string> slugcats = slugcats;
        }

        private readonly struct AggregatedRoom(string room, List<string> slugcats)
        {
            public readonly string room = room;
            public readonly List<string> slugcats = slugcats;
        }
    }
}
