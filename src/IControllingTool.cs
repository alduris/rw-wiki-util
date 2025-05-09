using UnityEngine;

namespace WikiUtil
{
    /// <summary>
    /// <seealso cref="ITool"/> that can take priority over the other tools, or pause the game outright. Note that these effects only happen if it is actually run first.
    /// <para>In <see cref="ITool.Run(RainWorld, bool)"/> implementation, use <seealso cref="Time.deltaTime"/> for how long it has been since the last call.</para>
    /// </summary>
    public interface IControllingTool : ITool
    {
        /// <summary>
        /// Whether or not the tool should take/remain in control.
        /// </summary>
        /// <param name="rainWorld">The current instance of <see cref="RainWorld"/>.</param>
        /// <returns>Whether or not the tool should take control for multiple frames.</returns>
        public bool ShouldITakeControl(RainWorld rainWorld);

        /// <summary>
        /// Whether or not to pause game updates (fixed or graphic) while in control.
        /// </summary>
        /// <param name="rainWorld">The current instance of <see cref="RainWorld"/>.</param>
        /// <returns>Whether or not to pause game updates.</returns>
        public bool ShouldTheGameStillRun(RainWorld rainWorld);
    }
}
