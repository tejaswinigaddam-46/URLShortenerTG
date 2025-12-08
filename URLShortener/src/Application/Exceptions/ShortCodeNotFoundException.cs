namespace URLShortener.Application.Exceptions {
    public class ShortCodeNotFoundException : Exception
    {
        public ShortCodeNotFoundException()
        {
        }
        
        public ShortCodeNotFoundException(string message) : base(message)
        {
        }

        public ShortCodeNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}