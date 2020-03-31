using System;

namespace ChaseLabs.CLLogger.Events
{
    /// <summary>
    /// <para>
    /// Author: Drew Chase
    /// </para>
    /// <para>
    /// Company: Chase Labs
    /// </para>
    /// </summary>
    public class LogEventArgs : EventArgs
    {
        public string Log { get; set; }
    }
}
