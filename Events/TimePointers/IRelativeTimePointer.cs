namespace Events.TimePointers
{
    public interface IRelativeTimePointer
    {
        DateTimeOffset Adjust(DateTimeOffset dateTime);
    }
}
