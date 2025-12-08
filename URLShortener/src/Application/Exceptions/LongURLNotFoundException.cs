namespace URLShortener.Application.Exceptions {
    public class LongURLNotFoundException : Exception
    {
        public LongURLNotFoundException()
        {
        }
        
        public LongURLNotFoundException(string message) : base(message)
        {
        }

        public LongURLNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}