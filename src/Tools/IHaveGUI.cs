using UnityEngine;

namespace WikiUtil.Tools
{
    /// <summary>
    /// For tools that have a GUI that needs to be shown.
    /// Run independent of keybind checks; if you want a keybind to activate it, you have to add that logic.
    /// </summary>
    /// <remarks>
    /// For information on how to use Unity's <see cref="GUI"/> system, visit
    /// https://docs.unity3d.com/2022.3/Documentation//Manual/gui-Basics.html
    /// </remarks>
    public interface IHaveGUI
    {
        /// <summary>
        /// Whether to show the window at the given moment (determines whether <see cref="OnGUI(RainWorld)"/> runs)
        /// </summary>
        public bool ShowWindow { get; }

        /// <summary>
        /// Size of the window. All content should fit into it. Window title is set to the name of the <see cref="Tool"/>.
        /// </summary>
        public Rect WindowSize { get; set; }

        /// <summary>
        /// <see cref="GUI"/> code. Remember to account for the window title by leaving at least 20px of space.
        /// </summary>
        /// <param name="rainWorld">The current instance of <see cref="RainWorld"/>.</param>
        public void OnGUI(RainWorld rainWorld);
    }
}
