﻿namespace ChaseLabs.CLLogger.Interfaces
{
    /// <summary>
    /// <list type="table">
    /// <item>Author: Drew Chase</item>
    /// <item>Company: Chase Labs</item>
    /// </list>
    /// </summary>
    public interface ILog
    {
        #region Properties

        bool IsDebugEnabled { get; }

        bool IsErrorEnabled { get; }

        bool IsFatalEnabled { get; }

        bool IsInfoEnabled { get; }

        bool IsWarnEnabled { get; }

        string Pattern { get; }

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// A Debug Log with a Single Message
        /// </summary>
        /// <param name="message"></param>
        void Debug(object message);

        /// <summary>
        /// A Debug Log with Multiple Messages
        /// </summary>
        /// <param name="messages"></param>
        void Debug(params object[] messages);

        /// <summary>
        /// A Debug Log with a Single Message and an Exception
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        void Debug(object message, Exception exception);

        /// <summary>
        /// A Error Log with a Single Message
        /// </summary>
        /// <param name="message"></param>
        void Error(object message);

        /// <summary>
        /// A Error Log with Multiple Messages
        /// </summary>
        /// <param name="messages"></param>
        void Error(params object[] messages);

        /// <summary>
        /// A Error Log with a Single Message and a Exception
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        void Error(object message, Exception exception);

        /// <summary>
        /// A Fatal Log with a Single Message
        /// </summary>
        /// <param name="message"></param>
        void Fatal(object message);

        /// <summary>
        /// A Fatal Log with Multiple Messages
        /// </summary>
        /// <param name="messages"></param>
        void Fatal(params object[] messages);

        /// <summary>
        /// A Fatal Log with a Single Message and a Exception
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        void Fatal(object message, Exception exception);

        /// <summary>
        /// A Info Log with a Single Message and a Exception
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        void Info(object message, Exception exception);

        /// <summary>
        /// A Info Log with a Single Message
        /// </summary>
        /// <param name="message"></param>
        void Info(object message);

        /// <summary>
        /// A Info Log with Multiple Messages
        /// </summary>
        /// <param name="messages"></param>
        void Info(params object[] messages);

        /// <summary>
        /// A Warn Log with a Single Message
        /// </summary>
        /// <param name="message"></param>
        void Warn(object message);

        /// <summary>
        /// A Warn Log with Multiple Messages
        /// </summary>
        /// <param name="messages"></param>
        void Warn(params object[] messages);

        /// <summary>
        /// A Warn Log with a Single Message and an Exception
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        void Warn(object message, Exception exception);

        #endregion Public Methods
    }
}