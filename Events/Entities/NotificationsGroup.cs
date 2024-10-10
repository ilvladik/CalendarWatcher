
using Events.TimePointers;

namespace Events.Entities
{
    public class NotificationsGroup
    {
        public Action<Alarm> AlarmAction { get; private set; }
        public IEnumerable<IRelativeTimePointer> NotificationsTime { get; private set; }
        public Predicate<Event> ForEventsWhere { get; private set; } = (@event) => @event.Equals(@event);

        public NotificationsGroup(Action<Alarm> alarmAction, IEnumerable<IRelativeTimePointer> notificationsTime)
        {
            AlarmAction = alarmAction;
            NotificationsTime = notificationsTime;
        }
        public NotificationsGroup(Action<Alarm> alarmAction, IEnumerable<IRelativeTimePointer> notificationsTime, Predicate<Event> forEventWhere)
        {
            AlarmAction = alarmAction;
            NotificationsTime = notificationsTime;
            ForEventsWhere = forEventWhere;
        }
        public IEnumerable<Alarm> GetAlarms(IEnumerable<Event> events)
        {
            Dictionary<DateTimeOffset, HashSet<Event>> alarms = new();
            foreach (var pointer in NotificationsTime)
            {
                foreach (var @event in events.Where(e => ForEventsWhere.Invoke(e)))
                {
                    DateTimeOffset key = pointer.Adjust(@event.StartDate);
                    if (alarms.TryGetValue(key, out var group))
                    {
                        group.Add(@event);
                    }
                    else
                    {
                        alarms.Add(key, new HashSet<Event> { @event });
                    }
                }
            }
            return alarms.Select(keyValue => new Alarm(keyValue.Value, keyValue.Key));
        }
    }
}
