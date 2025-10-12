using WikiUtil.Tools;

namespace WikiUtil.BuiltIn
{
    internal class HideMarkTool : ToggleTool
    {
        internal const string TOOL_ID = "Hide Mark";

        public HideMarkTool() : base(TOOL_ID, new Keybind(UnityEngine.KeyCode.M))
        {
            On.PlayerGraphics.Update += HideGlow;
            On.PlayerGraphics.DrawSprites += HideMark;
        }

        public override void ToggleUpdate(RainWorld rainWorld) { }

        private void HideGlow(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
        {
            orig(self);
            if (toggled && self.lightSource != null)
            {
                self.lightSource.HardSetAlpha(0f);
            }
        }

        private void HideMark(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, UnityEngine.Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);
            if (toggled)
            {
                sLeaser.sprites[10].alpha = 0f;
                sLeaser.sprites[11].alpha = 0f;
            }
        }
    }
}
