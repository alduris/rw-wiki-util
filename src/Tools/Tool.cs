using UnityEngine;

namespace WikiUtil.Tools
{
    public abstract class Tool(string id, Keybind defaultKeybind)
    {
        public readonly string id = id;
        internal readonly Keybind defaultKeybind = defaultKeybind;

        public Keybind Keybind => ToolDatabase.GetKeybind(id);

        public bool KeybindPressed
        {
            get
            {
                var keybind = Keybind;
                return Input.GetKeyDown(keybind.keyCode)
                    && !((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) ^ keybind.ctrl)
                    && !((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) ^ keybind.alt)
                    && !((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ^ keybind.shift);
            }
        }

        public bool KeybindHeld
        {
            get
            {
                var keybind = Keybind;
                return Input.GetKey(keybind.keyCode)
                    && !((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) ^ keybind.ctrl)
                    && !((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) ^ keybind.alt)
                    && !((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ^ keybind.shift);
            }
        }

        public abstract void Update(RainWorld rainWorld);
    }
}
