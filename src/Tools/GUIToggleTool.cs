using UnityEngine;

namespace WikiUtil.Tools
{
    /// <summary>
    /// Variant of <see cref="ToggleTool"/> that has a <see cref="GUI"/> (that is, implements <see cref="IHaveGUI"/>) which gets displayed when toggled
    /// </summary>
    /// <param name="id">ID/display name of the tool</param>
    /// <param name="defaultKeybind">Default keybind of the tool</param>
    public abstract class GUIToggleTool(string id, Keybind defaultKeybind) : ToggleTool(id, defaultKeybind), IHaveGUI
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rainWorld"></param>
        public override void ToggleUpdate(RainWorld rainWorld) { }

        /// <summary>
        /// Whether to show the window at the given moment (determines whether <see cref="OnGUI(RainWorld)"/> runs)
        /// </summary>
        public virtual bool ShowWindow => toggled;

        /// <summary>
        /// Size of the window. All content should fit into it. Window title is set to the name of the <see cref="Tool"/>.
        /// </summary>
        public abstract Rect WindowSize { get; set; }

        /// <summary>
        /// <see cref="GUI"/> code. Remember to account for the window title by leaving at least 20px of space.
        /// </summary>
        /// <param name="rainWorld">The current instance of <see cref="RainWorld"/>.</param>
        public abstract void OnGUI(RainWorld rainWorld);
    }
}
