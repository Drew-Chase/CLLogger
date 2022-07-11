namespace ChaseLabs.CLLogger.Events
{
    /// <summary>
    /// <para>Author: Drew Chase</para>
    /// <para>Company: Chase Labs</para>
    /// </summary>
    public class LogEventArgs : EventArgs
    {
        #region Properties

        /// <summary>
        /// Log Message
        /// </summary>
        public string Log { get; set; }

        #endregion Properties
    }
}