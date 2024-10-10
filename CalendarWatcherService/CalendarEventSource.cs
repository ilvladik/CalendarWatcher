using Events.Entities;
using Events.Exceptions;
using Ical.Net;
using Ical.Net.CalendarComponents;

namespace Events.EventSources
{
    public class CalendarEventSource : IEventSource, IDisposable
    {
        private readonly HttpClient _httpClient = new();
        private bool disposedValue;

        public Uri Uri { get; private set; }
        public TimeZoneInfo TimeZoneInfo { get; private set; }
        public CalendarEventSource(string uri, TimeZoneInfo timeZoneInfo) 
        {
            if (!Uri.TryCreate(uri, UriKind.Absolute, out var _uri)) throw new ArgumentException("Illegal uri string");
            Uri = _uri;
            _httpClient.Timeout = Timeout.InfiniteTimeSpan;
            if (GetCalendarAsync() is null) throw new ArgumentException($"Calendar at the link: {Uri} was not found"); 
            TimeZoneInfo = timeZoneInfo;
        }

        public async Task<IEnumerable<Event>> GetEventsAsync(DateTimeOffset from, DateTimeOffset to)
        {
            Calendar? calendar = await GetCalendarAsync();
            return calendar is null
                ? throw new SourceNotFoundException($"Can't get Calendar at the link {Uri}")
                : calendar.GetOccurrences(from.UtcDateTime, to.UtcDateTime)
                        .Where(o => o.Source is CalendarEvent)
                        .Select(o =>
                        {
                            DateTimeOffset start, end;
                            bool isAllDay = ((CalendarEvent)o.Source).IsAllDay;
                            if (isAllDay)
                            {
                                start = o.Period.StartTime.AsDateTimeOffset.ToOffsetWithoutTime(TimeZoneInfo.BaseUtcOffset);
                                end = o.Period.EndTime.AsDateTimeOffset.ToOffsetWithoutTime(TimeZoneInfo.BaseUtcOffset);

                            }
                            else
                            {
                                start = o.Period.StartTime.AsDateTimeOffset.ToOffset(TimeZoneInfo.BaseUtcOffset);
                                end = o.Period.EndTime.AsDateTimeOffset.ToOffset(TimeZoneInfo.BaseUtcOffset);
                            }
                            return new Event
                            (
                                Name: ((CalendarEvent)o.Source).Summary,
                                StartDate: start,
                                EndDate: end,
                                IsAllDay: isAllDay,
                                Description: ((CalendarEvent)o.Source).Description
                            );
                        }
                        );
        }

        private async Task<Calendar?> GetCalendarAsync()
        {
            var httpResponse = await _httpClient.GetAsync(Uri);
            if (httpResponse.IsSuccessStatusCode)
            {
                await using var stream = await httpResponse.Content.ReadAsStreamAsync();
                return Calendar.Load(stream);
            }
            return null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _httpClient.Dispose();
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
