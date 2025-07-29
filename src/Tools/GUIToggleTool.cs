using UnityEngine;

namespace WikiUtil.Tools
{
    public abstract class GUIToggleTool(string id, Keybind defaultKeybind) : ToggleTool(id, defaultKeybind), IHaveGUI
    {
        public override void ToggleUpdate(RainWorld rainWorld) { }

        public virtual bool ShowWindow => toggled;
        public abstract Rect WindowSize { get; set; }

        public abstract void OnGUI(RainWorld rainWorld);
    }
}
