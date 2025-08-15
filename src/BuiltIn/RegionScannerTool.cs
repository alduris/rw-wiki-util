using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Menu.Remix;
using UnityEngine;
using WikiUtil.Tools;

namespace WikiUtil.BuiltIn
{
    internal class RegionScannerTool : GUIToggleTool
    {
        internal const string TOOL_ID = "Region Creature/Object Scanner";
        public RegionScannerTool() : base(TOOL_ID, new Keybind(KeyCode.R, true, false, false))
        {
        }

        private static readonly HashSet<string> defaultSlugcats = ["White", "Yellow", "Red", "Gourmand", "Artificer", "Rivulet", "Spear", "Saint", "Inv"]; // I would put watcher also but they're a unique case
        private bool hasInitializedSlugcats = false;
        private string[] regions = [];
        private string[] slugcats = [];
        private bool[] slugcatsEnabled = [];
        private readonly Dictionary<string, RegionRepresentation> representations = [];

        private RegionRepresentation ActiveRegionRepresentation => regionPickerValue > -1 && representations.TryGetValue(regions[regionPickerValue], out var r) ? r : null;
        private int CountSlugcatsEnabled => slugcatsEnabled.Count(x => x);

        public override void ToggleOn(RainWorld rainWorld)
        {
            // Reset region and slugcat cache, while also keeping the previously enabled slugcats
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
            float regionPickerHeight = 24f * Mathf.CeilToInt((float)regions.Length / REGIONCOLS) - 4f;
            regionPickerScroll = GUI.BeginScrollView(new Rect(contentMargin, 44f, regionPickerWidth, contentHeight), regionPickerScroll, new Rect(0f, 0f, regionPickerWidth - 20f, regionPickerHeight));
            regionPickerValue = GUI.SelectionGrid(new Rect(0f, 0f, regionPickerWidth - 20f, regionPickerHeight), regionPickerValue, regions, REGIONCOLS);
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

            // Show scanner output
            if (ActiveRegionRepresentation != null)
            {
                var representation = ActiveRegionRepresentation;
                var spawns = representation.CalculateSharedSpawns();

                if (spawns.Count > 0)
                {
                    var height = spawns.Sum(HeightOfSharedSpawns);
                    outputAreaScroll = GUI.BeginScrollView(new Rect(rightAreaStartX, outputAreaStartY, rightAreaWidth, outputAreaHeight - contentMargin - 20f), outputAreaScroll, new Rect(0f, 0f, rightAreaWidth - 20f, height));

                    float y = 0f;
                    for (int i = 0; i < spawns.Count; i++)
                    {
                        var sharedSpawn = spawns[i];

                        float combinedNameHeight = HeightOf(sharedSpawn.CombinedName, rightAreaWidth - 20f);
                        GUI.Label(new Rect(0f, y, rightAreaWidth - 20f, combinedNameHeight), sharedSpawn.CombinedName);
                        y += combinedNameHeight + 4f;

                        for (int j = 0; j < sharedSpawn.SpawnStrings.Length; j++)
                        {
                            var (type, values) = sharedSpawn.SpawnStrings[j];

                            float typeWidth = rightAreaWidth - 20f - innerTabWidth;
                            float typeHeight = HeightOf(type, typeWidth);
                            GUI.Label(new Rect(innerTabWidth, y, typeWidth, typeHeight), type);
                            y += typeHeight + 4f;

                            float valuesWidth = rightAreaWidth - 20f - innerTabWidth * 2;
                            float valuesHeight = HeightOf(values, valuesWidth);
                            GUI.Label(new Rect(innerTabWidth * 2, y, valuesWidth, valuesHeight), values);
                            y += valuesHeight + 4f;
                        }
                    }

                    GUI.EndScrollView();

                    if (GUI.Button(new Rect(rightAreaStartX, outputAreaStartY + outputAreaHeight - 20f, rightAreaWidth, 20f), "Copy"))
                    {
                        var sb = new StringBuilder();
                        for (int i = 0; i < spawns.Count; i++)
                        {
                            var sharedSpawn = spawns[i];
                            sb.AppendLine(sharedSpawn.CombinedName);
                            for (int j = 0; j < sharedSpawn.SpawnStrings.Length; j++)
                            {
                                var (type, values) = sharedSpawn.SpawnStrings[j];
                                sb.Append('\t');
                                sb.AppendLine(type);
                                sb.Append('\t');
                                sb.Append('\t');
                                sb.AppendLine(values);
                            }
                        }

                        UniClipboard.SetText(sb.ToString());
                    }
                }
                else
                {
                    GUI.Label(new Rect(rightAreaStartX, outputAreaStartY, rightAreaWidth, outputAreaHeight), "Loading...");
                }
            }
            else
            {
                GUI.Label(new Rect(rightAreaStartX, outputAreaStartY, rightAreaWidth, outputAreaHeight), "Pick a region");
            }

            float HeightOfSharedSpawns(SharedSpawns spawns)
            {
                return HeightOf(spawns.CombinedName, rightAreaWidth - 20f) + 4f + spawns.SpawnStrings.Sum(HeightOfSpawnString);
            }

            float HeightOfSpawnString((string type, string values) spawnString)
            {
                return HeightOf(spawnString.type, rightAreaWidth - 20f - innerTabWidth) + HeightOf(spawnString.values, rightAreaWidth - 20f - innerTabWidth * 2) + 8f;
            }
        }

        private float HeightOf(string text, float width) => GUI.skin.label.CalcHeight(new GUIContent(text), width);

        private enum SpawnType
        {
            Normal,
            Lineage,
            Night,
            Precycle,
            Object
        }

        private class RoomRepresentation
        {
            public RoomRepresentation(AbstractRoom room, SlugcatStats.Timeline timeline)
            {
                offscreen = room.offScreenDen;
                name = room.name;

                // Connections
                neighbors = [];
                foreach (int connection in room.connections)
                {
                    try
                    {
                        string name = room.world.GetAbstractRoom(connection).name;
                        neighbors.Add(name);
                    }
                    catch { } // to catch random room connection user errors
                }

                // Creatires
                spawns = [];
                foreach (World.CreatureSpawner spawner in room.world.spawners.Where(x => x.den.room == room.index))
                {
                    if (spawner is World.Lineage lineage)
                    {
                        for (int i = 0; i < lineage.creatureTypes.Length; i++)
                        {
                            SpawnType type = SpawnTypeFromSpawnData(lineage.spawnData[i]);
                            string creature = lineage.creatureTypes[i] < 0 ? null : ExtEnum<CreatureTemplate.Type>.values.GetEntry(lineage.creatureTypes[i]);
                            if (i > 0)
                            {
                                AddSpawnData(SpawnType.Lineage, creature);
                            }
                            if (i == 0 || type != SpawnType.Normal)
                            {
                                AddSpawnData(type, creature);
                            }
                        }
                    }
                    else if (spawner is World.SimpleSpawner simple)
                    {
                        AddSpawnData(SpawnTypeFromSpawnData(simple.spawnDataString), simple.creatureType?.value);
                    }
                    else
                    {
                        Plugin.Logger.LogWarning("WARNING: UNKNOWN SPAWNER TYPE " + spawner.GetType().FullName);
                    }
                }

                // Account for batflies! (We have no way of knowing for sure if it'll spawn a variant though so oh well)
                if (room.swarmRoom)
                {
                    AddSpawnData(SpawnType.Normal, nameof(CreatureTemplate.Type.Fly));
                }

                // Ok load room settings
                var roomSettings = new RoomSettings(null, room.name, room.world.region, false, false, timeline, null);

                // Yippee now we load the object types
                foreach (var po in roomSettings.placedObjects)
                {
                    if (po.active && !RegionScannerToolHelper.BannedPlacedObjectTypes.Contains(po.type.value))
                    {
                        AddSpawnData(SpawnType.Object, po.type.value);
                    }
                }
            }

            private SpawnType SpawnTypeFromSpawnData(string spawnData)
            {
                if (spawnData == null) return SpawnType.Normal;
                if (spawnData.Contains("Night"))
                {
                    return SpawnType.Night;
                }
                else if (spawnData.Contains("PreCycle"))
                {
                    return SpawnType.Precycle;
                }
                return SpawnType.Normal;
            }

            private void AddSpawnData(SpawnType type, string item)
            {
                if (string.IsNullOrEmpty(item) || item == "NONE") return;
                if (!spawns.TryGetValue(type, out HashSet<string> data))
                {
                    data = [];
                    spawns.Add(type, data);
                }
                data.Add(item);
            }

            public bool offscreen;
            public string name;
            public List<string> neighbors;
            public Dictionary<SpawnType, HashSet<string>> spawns;
        }

        private class WorldstateRepresentation
        {
            public WorldstateRepresentation(string acronym, string slugcat)
            {
                this.acronym = acronym;
                this.slugcat = slugcat;

                var scugName = slugcat != null ? new SlugcatStats.Name(slugcat) : null;
                var timeline = slugcat != null ? SlugcatStats.SlugcatToTimeline(scugName) : null;
                var region = Region.LoadAllRegions(timeline, null).First(x => x.name == acronym);
                loader = new WorldLoader(null, scugName, timeline, false, region.name, region, RainWorld.LoadSetupValues(true), WorldLoader.LoadingContext.FULL);
                loader.NextActivity();
            }
            public string acronym;
            public string slugcat;
            private readonly WorldLoader loader;
            private readonly Dictionary<string, RoomRepresentation> rooms = [];

            public bool WorldReady => loader.Finished && rooms.Count > 0;

            public void Update()
            {
                if (!loader.Finished)
                {
                    for (int i = 0; i < 100 && !WorldReady; i++)
                    {
                        loader.Update();
                    }
                }
                else if (rooms.Count == 0)
                {
                    var world = loader.ReturnWorld();
                    var timeline = slugcat != null ? SlugcatStats.SlugcatToTimeline(new SlugcatStats.Name(slugcat, false)) : null;
                    foreach (var room in world.abstractRooms)
                    {
                        if (!world.DisabledMapRooms.Contains(room.name, StringComparer.OrdinalIgnoreCase))
                        {
                            rooms[room.name] = new RoomRepresentation(room, timeline);
                        }
                    }
                }
            }

            public Dictionary<SpawnType, HashSet<string>> GetSpawns()
            {
                Dictionary<SpawnType, HashSet<string>> dict = [];
                foreach (var room in rooms.Values)
                {
                    foreach (var group in room.spawns)
                    {
                        if (!dict.ContainsKey(group.Key))
                        {
                            dict[group.Key] = [];
                        }
                        dict[group.Key].UnionWith(group.Value);
                    }
                }
                return dict;
            }
        }

        private class RegionRepresentation
        {
            public RegionRepresentation(string acronym, RegionScannerTool owner)
            {
                this.owner = owner;
                foreach (var slug in owner.slugcats)
                {
                    worldstates.Add(slug, new WorldstateRepresentation(acronym, slug));
                }
            }

            private readonly RegionScannerTool owner;
            private readonly Dictionary<string, WorldstateRepresentation> worldstates = [];
            
            public void Update()
            {
                foreach (var worldstate in worldstates.Values)
                {
                    worldstate.Update();
                    if (!worldstate.WorldReady) break;
                }
            }

            private int WorldstatesReady => worldstates.Count(x => x.Value.WorldReady);
            private int lastWorldstatesReady = -1;
            private int lastSlugcatsEnabled = -1;
            private List<SharedSpawns> sharedSpawnsCache = null;

            public List<SharedSpawns> CalculateSharedSpawns()
            {
                // Return cached information if applicable
                if (sharedSpawnsCache != null && WorldstatesReady == lastWorldstatesReady && lastSlugcatsEnabled == owner.CountSlugcatsEnabled) return sharedSpawnsCache;
                lastWorldstatesReady = WorldstatesReady;
                lastSlugcatsEnabled = owner.CountSlugcatsEnabled;

                // Create new cache
                List<SharedSpawns> sharedSpawns = [];

                foreach (var worldstate in worldstates)
                {
                    int index = owner.slugcats.IndexOf(worldstate.Key);
                    if (!worldstate.Value.WorldReady || (index > -1 && !owner.slugcatsEnabled[index])) continue;
                    var spawns = worldstate.Value.GetSpawns();

                    // Check if equivalent spawns are already in list
                    bool found = false;
                    foreach (var sharedGroup in sharedSpawns)
                    {
                        var groupSpawns = sharedGroup.spawns;
                        if (groupSpawns.Keys.Except(spawns.Keys).Count() == 0 && spawns.Keys.Except(groupSpawns.Keys).Count() == 0)
                        {
                            found = true;
                            foreach (var kvp in spawns)
                            {
                                if (!groupSpawns[kvp.Key].SetEquals(kvp.Value))
                                {
                                    found = false;
                                    break;
                                }
                            }

                            if (found)
                            {
                                sharedGroup.slugcats.Add(worldstate.Key);
                                break;
                            }
                        }
                    }

                    if (!found)
                    {
                        // Add new
                        sharedSpawns.Add(new SharedSpawns()
                        {
                            slugcats = [worldstate.Key],
                            spawns = spawns
                        });
                    }

                }

                sharedSpawnsCache = sharedSpawns;
                return sharedSpawns;
            }
        }

        private class SharedSpawns
        {
            public List<string> slugcats;
            public Dictionary<SpawnType, HashSet<string>> spawns;

            private string combinedNameCache;
            private int combinedNameCacheVersion = -1;
            public string CombinedName
            {
                get
                {
                    if (combinedNameCacheVersion != slugcats.Count)
                    {
                        combinedNameCacheVersion = slugcats.Count;
                        combinedNameCache = string.Join(", ", slugcats);
                    }
                    return combinedNameCache;
                }
            }

            private int SpawnsVersion => spawns.Sum(x => x.Value.Count);
            private int lastSpawnsVersion = -1;
            private (string, string)[] spawnsCache;
            public (string type, string values)[] SpawnStrings
            {
                get
                {
                    if (lastSpawnsVersion != SpawnsVersion)
                    {
                        lastSpawnsVersion = SpawnsVersion;
                        spawnsCache = new (string, string)[spawns.Count];
                        int i = 0;
                        foreach (var spawn in spawns.OrderBy(x => (int)x.Key))
                        {
                            spawnsCache[i++] = (spawn.Key.ToString(), string.Join(", ", spawn.Value.OrderBy(x => x, StringComparer.OrdinalIgnoreCase)));
                        }
                    }
                    return spawnsCache;
                }
            }
        }
    }
}
