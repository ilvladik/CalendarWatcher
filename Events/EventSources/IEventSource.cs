
using Events.Entities;

namespace Events.EventSources
{
    public interface IEventSource
    {
        Task<IEnumerable<Event>> GetEventsAsync(DateTimeOffset from,  DateTimeOffset to);
    }
}
