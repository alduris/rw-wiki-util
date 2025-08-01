using System.Collections.Generic;

namespace WikiUtil.BuiltIn
{
    internal static class RegionScannerToolHelper
    {
        public static readonly HashSet<string> BannedPlacedObjectTypes =
        [
            // Base game + DLC, as of v1.10.4
            "LightSource", "LightFixture", "SpotLight", "LightBeam", "CustomDecal", "Rainbow", "FairyParticleSettings",
            "DandelionPatch", "LanternOnStick", "InsectGroup", "BrokenShelterWaterLevel", "SnowSource", "LocalBlizzard",
            "DayNightSettings", "LightningMachine", "SteamPipe", "WallSteamer", "BlinkingFlower", "CellDistortion",
            "EnergySwirl", "AdjustableFan", "CosmeticRipple", "PomegranateVine", "HugeTurbine", "PlateTree",
            "FloatingDebris", "GooDrips", "LobeTree", "RotPlateTree", "BrainMold", "FlameJet", "WindDirection",
            "RainbowNoFade", "Filter", "RippleLevelFilter", "PrinceFilter", "WarpFilter", "NoSpearStickZone",
            "PlayerPushback", "CentipedeAttractor", "ExitSymbolShelter", "ExitSymbolAncientShelter", "ExitSymbolHidden",
            "NoLeviathanStrandingZone", "NeuronSpawner", "DeathFallFocus", "Vine", "HarmfulSteam", "WindRect",
            "ARKillRect", "WarpPoint", "DynamicWarpTarget", "ShelterSpawnPoint", "SkyWhalePathfinding", "MudPit",
            "ReliableSpear", "CosmeticSlimeMold", "CosmeticSlimeMold2", "MultiplayerItem", "CoralStem",
            "CoralStemWithNeurons", "CoralNeuron", "CoralCircuit", "WallMycelia", "ProjectedStars",
            "SuperStructureFuses", "DeepProcessing", "GravityDisruptor", "ZapCoil", "SSLightRod", "CorruptionTube",
            "CorruptionDarkness", "SuperJumpInstruction", "ProjectedImagePosition", "ScavTradeInstruction",
            "ReliableIggyDirection", "DeadTokenStalk", "TerrainHandle", "TerrainRubble", "TerrainSunOffset", "BlackSpot",
            "FluxWaterfall", "WaterCycleBottom", "WaterCycleTop", "FluxDrain", "Geyser", "WaterCurrent", "NoCurrentZone",
            "WaterCutoff", "AirPocket", "OEsphere", "MSArteryPush", "RotFlyPaper", "KarmaShrine", "AetherRainbow",
            "WallLight", "SKLightningRod", "UrbanLife", "UrbanLifePath", "KarmaFlowerPatch", "UrbanCandles",
            "UrbanCandleHolder", "ARZapperOmni", "ARZapperVertical", "ARZapperHorizontal",

            // RegionKit as of v3.17.4
            "ClimbableWire", "ClimbablePole", "ClimbableRope", "PWLightrod", "CustomEntranceSymbol", "NoWallSlideZone",
            "LittlePlanet", "ProjectedCircle", "UpsideDownWaterFall", "ColoredLightBeam", "FanLight", "NoBatflyLurkZone",
            "PCPlayerSensitiveLightSource", "WaterFallDepth", "NoDropwigPerchZone",
        ];
    }
}
