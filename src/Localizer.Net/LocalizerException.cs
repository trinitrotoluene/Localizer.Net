using System;

namespace Localizer.Net
{
    public class LocalizerException : Exception
    {
        public LocalizerException()
        {
        }

        public LocalizerException(string message) : base(message)
        {
        }

        public LocalizerException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
