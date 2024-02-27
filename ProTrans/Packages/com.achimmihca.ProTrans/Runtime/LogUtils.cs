using System;
using UnityEngine;

namespace ProTrans
{
    public static class LogUtils
    {
        internal static void Log(LogLevel logLevel, Func<string> textGetter, Exception cause = null)
        {
            if ((int)logLevel < (int)TranslationConfig.Singleton.MinimumLogLevel)
            {
                return;
            }

            if (cause != null)
            {
                Debug.LogException(cause);
            }

            string text = textGetter();
            switch (logLevel)
            {
                case LogLevel.Verbose:
                case LogLevel.Debug:
                case LogLevel.Info:
                    Debug.Log(text);
                    return;
                case LogLevel.Warning:
                    Debug.LogWarning(text);
                    return;
                case LogLevel.Error:
                    Debug.LogError(text);
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
            }
        }
    }
}
