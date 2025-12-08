namespace URLShortener.Application.Exceptions {
    public class LongCodeStorageException : Exception
    {
        public LongCodeStorageException()
        {
        }
        
        public LongCodeStorageException(string message) : base(message)
        {
        }

        public LongCodeStorageException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}