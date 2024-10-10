namespace System
{
    public static class DateTimeOffsetExtensions
    {
        public static DateTimeOffset ToOffsetWithoutTime(this DateTimeOffset dateTimeOffset, TimeSpan offset)
        {
            return dateTimeOffset
                .Subtract(offset)
                .ToOffset(offset);
        }
    }
}
