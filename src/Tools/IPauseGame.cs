namespace WikiUtil.Tools
{
    /// <summary>
    /// Can pause any actions that take place within <see cref="RainWorld.Update"/>
    /// </summary>
    public interface IPauseGame
    {
        /// <summary>
        /// Whether or not to pause
        /// </summary>
        public bool Pause { get; }
    }
}
