using UnityEngine;

namespace WikiUtil.Tools
{
    /// <summary>
    /// Basic tool that runs code every <see cref="RainWorld.Update"/> if enabled.
    /// </summary>
    /// <param name="id">ID/display name of the tool</param>
    /// <param name="defaultKeybind">Default keybind of the tool</param>
    public abstract class Tool(string id, Keybind defaultKeybind)
    {
        /// <summary>ID/display name of the tool</summary>
        public readonly string id = id;
        internal readonly Keybind defaultKeybind = defaultKeybind;

        /// <summary>Returns the keybind associated with the tool</summary>
        public Keybind Keybind => ToolDatabase.GetKeybind(id);

        /// <summary>Checks whether the tool's associated keybind is pressed (that is, in the first frame it is held)</summary>
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

        /// <summary>Checks whether the tool's associated keybind is held</summary>
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

        /// <summary>
        /// Runs during <see cref="RainWorld.Update"/>
        /// </summary>
        /// <param name="rainWorld">The instance of <see cref="RainWorld"/></param>
        public abstract void Update(RainWorld rainWorld);
    }
}
