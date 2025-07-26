using System.Collections.Generic;
using UnityEngine;
using static WikiUtil.Tools.IconsToolHelper;

namespace WikiUtil.Tools
{

    internal class IconsTool : ITool, IHaveGUI
    {
        private bool showGui = false;

        public bool ShowGUI => showGui;

        public bool ShouldIRun(RainWorld rainWorld) => true;

        public void Run(RainWorld rainWorld, bool update) => showGui = !showGui;

        public void OnGUI(RainWorld rainWorld)
        {
            // https://docs.unity3d.com/2022.3/Documentation//Manual/gui-Basics.html
            GUI.Box(new Rect(100f, 100f, 100f, 500f), "Test");

            // see if it has a dropdown and number input, if so then use that as the way to get icons
        }
    }
}
