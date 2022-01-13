using System;

namespace AudioSwitcher.AudioApi.CoreAudio.Threading
{
    public sealed class InvalidThreadException : Exception
    {

        public InvalidThreadException()
        {
            
        }

        public InvalidThreadException(string message)
            : base(message)
        {
        }
    }
}