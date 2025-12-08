namespace URLShortener.Application.Exceptions {
    public class ShortCodeStorageException : Exception
    {
        public ShortCodeStorageException()
        {
        }
        
        public ShortCodeStorageException(string message) : base(message)
        {
        }

        public ShortCodeStorageException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}