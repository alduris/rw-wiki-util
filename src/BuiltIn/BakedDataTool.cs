using System.Linq;
using WikiUtil.Tools;

namespace WikiUtil.BuiltIn
{
    internal class BakedDataTool : ActionTool
    {
        internal const string TOOL_ID = "Baked Data Visualizer";
        public BakedDataTool() : base(TOOL_ID, new Keybind(UnityEngine.KeyCode.A, true, false, false)) { }

        public override void Action(RainWorld rainWorld)
        {
            if (rainWorld.processManager.currentMainLoop is RainWorldGame game && game.cameras[0].room is Room room)
            {
                if (room.updateList.Any(x => x is BakedDataDrawable))
                {
                    foreach (var x in room.updateList)
                    {
                        if (x is BakedDataDrawable bdd)
                        {
                            bdd.NextPhase();
                        }
                    }
                }
                else
                {
                    room.AddObject(new BakedDataDrawable(room));
                }
            }
        }
    }
}
