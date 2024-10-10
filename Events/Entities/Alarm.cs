
namespace Events.Entities
{
    public record Alarm(IEnumerable<Event> Events, DateTimeOffset Time);
}
