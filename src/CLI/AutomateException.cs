using System;
using System.Runtime.Serialization;

namespace automate
{
    [Serializable]
    internal class AutomateException : Exception
    {
        public AutomateException()
        {
        }

        public AutomateException(string message) : base(message)
        {
        }

        public AutomateException(string message, Exception inner) : base(message, inner)
        {
        }

        protected AutomateException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}