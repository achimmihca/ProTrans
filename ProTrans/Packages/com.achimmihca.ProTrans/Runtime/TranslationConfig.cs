namespace ProTrans
{
    public class TranslationConfig
    {
        public static TranslationConfig Singleton { get; set; } = new();

        public LogLevel minimumLogLevel;
    }
}
