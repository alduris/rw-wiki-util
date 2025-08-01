using UnityEngine;

namespace WikiUtil
{
    /// <summary>
    /// Keybind data
    /// </summary>
    public struct Keybind
    {
        /// <summary>
        /// Initializes with a keycode and no special keys held.
        /// </summary>
        /// <param name="keyCode">Key code</param>
        public Keybind(KeyCode keyCode)
        {
            this.keyCode = keyCode;
            ctrl = alt = shift = false;
        }

        /// <summary>
        /// Initializes with a keycode and special keys
        /// </summary>
        /// <param name="keyCode">Key code</param>
        /// <param name="ctrl">Whether to require control be held</param>
        /// <param name="alt">Whether to require alt be held</param>
        /// <param name="shift">Whether to require shift be held</param>
        public Keybind(KeyCode keyCode, bool ctrl, bool alt, bool shift)
        {
            this.keyCode = keyCode;
            this.ctrl = ctrl;
            this.alt = alt;
            this.shift = shift;
        }

        /// <summary>The key code to press</summary>
        public KeyCode keyCode;
        /// <summary>Whether Control is pressed</summary>
        public bool ctrl;
        /// <summary>Whether Alt is pressed</summary>
        public bool alt;
        /// <summary>Whether Shift is pressed</summary>
        public bool shift;
    }
}
