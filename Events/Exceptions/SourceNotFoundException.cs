namespace Events.Exceptions
{
    public class SourceNotFoundException : Exception
    {
        public SourceNotFoundException() { }
        public SourceNotFoundException(string message) : base(message) { }
    }
}
