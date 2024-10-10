namespace Events.Entities
{
    public record Event(string Name, DateTimeOffset StartDate, DateTimeOffset EndDate, bool IsAllDay, string? Description);
}
