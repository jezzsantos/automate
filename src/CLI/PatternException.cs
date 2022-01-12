using System;
using System.Runtime.Serialization;

namespace automate
{
    [Serializable]
    internal class PatternException : Exception
    {
        public PatternException()
        {
        }

        public PatternException(string message) : base(message)
        {
        }

        public PatternException(string message, Exception inner) : base(message, inner)
        {
        }

        protected PatternException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}