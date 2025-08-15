using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WikiUtil.BuiltIn
{
    internal static class CorniferToolHelper
    {
        public static int ReturnInstallFeatures()
        {
            int result = 0;
            // 1 = legacy, aka 1.5, we don't support that so we can skip
            if (ModManager.MMF) result |= 2;
            if (ModManager.MSC) result |= 4;
            if (SteamManager.Initialized) result |= 8;
            if (ModManager.ActiveMods.Any(x => x.id == "crs")) result |= 16;
            if (ModManager.Watcher) result |= 32;
            return result;
        }

        private static string[] ReadFileLines(string path)
        {
            // for file text, we need to enforce consistent line endings
            path = AssetManager.ResolveFilePath(path);
            if (File.Exists(path)) return File.ReadAllLines(path);
            return [""];
        }

        private static string ReadFileWithEnforcedLineEnding(string path)
        {
            // for file text, we need to enforce consistent line endings
            return string.Join("\n", ReadFileLines(path));
        }

        public static Dictionary<string, object> ReturnRegionJson(World world, string slugcat)
        {
            // https://github.com/enchanted-sword/Cornifer/blob/master/Region.cs#L921
            Dictionary<string, object> saveData = [];
            saveData["id"] = world.name;
            saveData["legacy"] = false;
            saveData["world"] = ReadFileWithEnforcedLineEnding(Path.Combine("world", world.name, $"world_{world.name}.txt"));
            string properties = ReadFileWithEnforcedLineEnding(Path.Combine("world", world.name, $"properties-{slugcat}.txt"));
            if (properties.Length == 0)
            {
                saveData["properties"] = properties = ReadFileWithEnforcedLineEnding(Path.Combine("world", world.name, "properties.txt"));
            }
            else
            {
                saveData["properties"] = properties;
            }
            string slugcatMap = ReadFileWithEnforcedLineEnding(Path.Combine("world", world.name, $"map_{world.name}-{slugcat}.txt"));
            if (slugcatMap.Length == 0)
            {
                saveData["map"] = ReadFileWithEnforcedLineEnding(Path.Combine("world", world.name, $"map_{world.name}.txt"));
            }
            else
            {
                saveData["map"] = slugcatMap;
            }
            HashSet<string> gates = [.. world.abstractRooms.Where(x => x.gate).Select(x => x.name)];
            var rawLocks = ReadFileLines(Path.Combine("world", "gates", "locks.txt"));
            saveData["locks"] = string.Join("\n", rawLocks.Where(x => gates.Contains(x.Split([" : "], StringSplitOptions.None)[0])));
            var subregions = properties.Split('\n').Where(x => x.StartsWith("Subregion: ")).Select(x => x.Split([": "], 2, StringSplitOptions.None)[1]).ToList();
            saveData["subregionOrder"] = subregions;
            saveData["rooms"] = world.abstractRooms.Select(
                x => new Dictionary<string, object>()
                {
                    ["id"] = x.name,
                    ["data"] = string.Join("\n", ReadFileLines(Path.Combine("world", world.name + "-rooms", x.name + ".txt"))
                                                    .Select((y, i) => (i != 0 && i != 1 && i != 11) ? "" : y)),
                    ["settings"] = File.Exists(AssetManager.ResolveFilePath(Path.Combine("world", $"{world.name}-rooms", $"{x.name}_settings-{slugcat}.txt")))
                        ? ReadFileWithEnforcedLineEnding(Path.Combine("world", $"{world.name}-rooms", $"{x.name}_settings-{slugcat}.txt"))
                        : ReadFileWithEnforcedLineEnding(Path.Combine("world", $"{world.name}-rooms", $"{x.name}_settings.txt"))
                }
                ).ToList();
            saveData["subregions"] = subregions.Select(
                x => new Dictionary<string, object>()
                {
                    ["name"] = x,
                    ["background"] = "#ffffff",
                    ["water"] = "#0077ff"
                }
                );
            return saveData;
        }
    }
}
