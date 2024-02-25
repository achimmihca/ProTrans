using System;
using UnityEngine;

namespace ProTrans
{
    public static class LogUtils
    {
        internal static void Log(LogLevel logLevel, Func<string> textGetter)
        {
            if ((int)logLevel < (int)TranslationConfig.Singleton.minimumLogLevel)
            {
                return;
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
