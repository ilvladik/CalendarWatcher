namespace Events.TimePointers
{
    public class DayTimePointer : IRelativeTimePointer
    {
        private readonly TimeOnly _time;
        private readonly int _day;

        public DayTimePointer(TimeOnly time, int day = 0)
        {
            _time = time;
            _day = day;
        }

        public DateTimeOffset Adjust(DateTimeOffset dateTime)
        {
            return new DateTimeOffset(dateTime.Date
                .AddDays(_day)
                .Add(_time.ToTimeSpan()), dateTime.Offset); 
        }
    }
}
