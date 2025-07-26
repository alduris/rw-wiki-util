using RWCustom;
using UnityEngine;

namespace WikiUtil
{
    internal class ToolGUI : MonoBehaviour
    {
        private static ToolGUI instance = null;

        internal static void Initialize()
        {
            instance ??= new GameObject("Wiki Util GUI").AddComponent<ToolGUI>();
        }

        public void OnGUI()
        {
            var rainWorld = Custom.rainWorld;
            foreach (var (_, tool) in ToolDatabase.GetToolOrder())
            {
                if (tool.ShouldIRun(rainWorld) && tool is IHaveGUI guiHaver && guiHaver.ShowGUI)
                {
                    guiHaver.OnGUI(rainWorld);
                }
            }
        }
    }
}
