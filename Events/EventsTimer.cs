
using Events.Entities;
using Events.EventSources;


namespace Events
{
    public class EventsTimer : IDisposable
    {
        public static readonly TimeSpan MinUpdatePeriod = TimeSpan.FromMinutes(5);
        public static readonly TimeSpan MaxSearchInterval = TimeSpan.FromDays(5 * 365);

        private bool disposedValue;
        private readonly IEnumerable<NotificationsGroup> _notificationsGroups;
        private readonly IEventSource _source;

        private TimeSpan updatePeriod = TimeSpan.FromHours(1);
        private TimeSpan leftSearchInterval = TimeSpan.FromDays(30);
        private TimeSpan rightSearchInterval = TimeSpan.FromDays(30);

        public EventsTimer(IEventSource source, IEnumerable<NotificationsGroup> notificationsGroups)
        {
            _source = source;
            _notificationsGroups = notificationsGroups;
        }

        public TimeSpan UpdatePeriod
        {
            get => updatePeriod;
            init
            {
                if (value < MinUpdatePeriod)
                    throw new ArgumentOutOfRangeException(nameof(value), $"{nameof(UpdatePeriod)} must be greater than {MinUpdatePeriod}");
                updatePeriod = value;
            }
        }
        public TimeSpan LeftSearchInterval
        {
            get => leftSearchInterval;
            init
            {
                if (value < TimeSpan.Zero || value > MaxSearchInterval)
                    throw new ArgumentOutOfRangeException(nameof(value), $"{nameof(LeftSearchInterval)} must be positive and less than {MaxSearchInterval}");
                leftSearchInterval = value;
            }
        }
        public TimeSpan RightSearchInterval
        {
            get => rightSearchInterval;
            init
            {
                if (value < TimeSpan.Zero)
                    throw new ArgumentOutOfRangeException(nameof(value), $"{nameof(RightSearchInterval)} must be positive and less than {MaxSearchInterval}");
                rightSearchInterval = value;
            }
        }

        public event EventHandler<ExceptionEventArgs>? Error;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            for (DateTimeOffset start = DateTimeOffset.UtcNow, end = start + UpdatePeriod; !cancellationToken.IsCancellationRequested; start = end, end += UpdatePeriod)
            {
                Task delayTask = Task.Delay(end - start, cancellationToken);
                try
                {
                    var alarmsAndActions = GetAlarms(await _source
                        .GetEventsAsync(start-LeftSearchInterval, end+RightSearchInterval), start, end)
                        .OrderBy(alarm => alarm.Alarm.Time);
                    foreach (var alarmAndAction in alarmsAndActions)
                    {
                        await DelayWithNegative(alarmAndAction.Alarm.Time);
                        alarmAndAction.AlarmAction(alarmAndAction.Alarm);
                    }
                    await delayTask;
                }
                catch (Exception ex)
                {
                    RaiseError(ex);
                }
                finally { await delayTask; }
            }
        }

        private IEnumerable<(Alarm Alarm, Action<Alarm> AlarmAction)> GetAlarms(IEnumerable<Event> events, DateTimeOffset from, DateTimeOffset to)
        {
            List<ValueTuple<Alarm, Action<Alarm>>> alarms = new();
            foreach (NotificationsGroup group in _notificationsGroups)
            {
                alarms.AddRange(group.GetAlarms(events).Where(alarm => alarm.Time >= from && alarm.Time < to)
                    .Select(alarm => (Alarm: alarm, group.AlarmAction)));
            }
            return alarms;
        }

        private static async Task DelayWithNegative(DateTimeOffset end)
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            if (end <= now)
                return;
            await Task.Delay(end - now);
        }

        private void RaiseError(Exception exception)
        {
            Error?.Invoke(this, new ExceptionEventArgs(exception));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_source is IDisposable disposable)
                        disposable.Dispose();
                }
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
