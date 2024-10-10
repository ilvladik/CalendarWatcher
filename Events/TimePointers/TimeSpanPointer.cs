namespace Events.TimePointers
{
    public class TimeSpanPointer : IRelativeTimePointer
    {
        private readonly TimeSpan _time;

        public TimeSpanPointer(TimeSpan time)
        {
            _time = time;
        }
        public DateTimeOffset Adjust(DateTimeOffset dateTime)
        {
            return dateTime.Add(_time);
        }
    }
}
