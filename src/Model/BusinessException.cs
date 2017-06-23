namespace CarTracker.Model
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class BusinessException : Exception
    {
        public BusinessException()
            : this((string)null)
        {
        }

        public BusinessException(string message)
            : this(message, null)
        {
        }

        public BusinessException(Exception inner)
            : this(null, inner)
        {
        }

        public BusinessException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected BusinessException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
