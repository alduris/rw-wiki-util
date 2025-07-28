using UnityEngine;

namespace WikiUtil
{
    public struct Keybind
    {
        public Keybind(KeyCode keyCode)
        {
            this.keyCode = keyCode;
            ctrl = alt = shift = false;
        }

        public Keybind(KeyCode keyCode, bool ctrl, bool alt, bool shift)
        {
            this.keyCode = keyCode;
            this.ctrl = ctrl;
            this.alt = alt;
            this.shift = shift;
        }

        public KeyCode keyCode;
        public bool ctrl;
        public bool alt;
        public bool shift;
    }
}
