namespace CarTracker.Persistence
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class PersistenceException : Exception
    {
        public PersistenceException()
            : this((string)null)
        {
        }

        public PersistenceException(string message)
            : this(message, null)
        {
        }

        public PersistenceException(Exception inner)
            : this("A persistence exception has occurred.", inner)
        {
        }

        public PersistenceException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected PersistenceException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
