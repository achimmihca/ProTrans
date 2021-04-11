using System;

namespace ProTrans
{
    public class TranslationException : Exception
    {
        public TranslationException(string msg) : base(msg)
        {
        }
        
        public TranslationException(string msg, Exception cause) : base(msg, cause)
        {
        }
    }
}
