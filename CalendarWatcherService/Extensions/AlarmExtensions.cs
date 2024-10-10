using Events.Entities;
using System.Text;

namespace CalendarWatcherService.Extensions
{
    public static class AlarmExtensions
    {
        public static List<string> ToLongMessages(this Alarm args)
        {
            return args.Events.OrderBy(o => o.StartDate).Select(o =>
            {
                string timePointer = o.StartDate.Date == DateTime.Today ? 
                    "Сегодня!\n" : 
                    (o.StartDate.Date - DateTime.Today).TotalDays == 1 ? "Завтра!\n" : string.Empty;
                string description = o.Description != null ? $"Дополнительно:\n{o.Description}\n" : string.Empty;
                return $"{timePointer}{o.Name}\n{description}Начало: {o.StartDate.ToOffset(TimeSpan.FromHours(3)):HH:mm, dd MMMM, dddd}";
            }).ToList();
        }

        public static string ToCheckList(this Alarm alarm)
        {
            bool today = alarm.Events.All(o => o.StartDate.Date == DateTimeOffset.UtcNow.ToOffset(o.StartDate.Offset).Date);
            bool tommorow = alarm.Events.All(o => o.StartDate.Date == DateTimeOffset.UtcNow.ToOffset(o.StartDate.Offset).Date.AddDays(1));
            StringBuilder builder = new();
            if (today)
                builder.Append("События на сегодня!\n");
            else if (tommorow)
                builder.Append("События на завтра!\n");
            else
                throw new Exception($"Неожиданные данные для чек листа");
            var notAllDayMessages = alarm.Events.Where(o => !o.IsAllDay).OrderBy(o => o.StartDate).Select(
                o => $"{o.StartDate.ToOffset(TimeSpan.FromHours(3)):HH:mm} {o.Name}"
            ).ToList();
            var allDayMessages = alarm.Events.Where(o => o.IsAllDay).OrderBy(o => o.StartDate).Select(
                o => $"{o.Name}"
            ).ToList();
            for (var i = 0; i < notAllDayMessages.Count; i++)
            {
                builder.Append($"{i + 1}. {notAllDayMessages[i]}").AppendLine();
            }
            if (allDayMessages.Count > 0)
            {
                builder.Append('—').AppendLine();
                foreach (var o in allDayMessages) builder.Append(o).AppendLine();
            }
            return builder.ToString();
        }
    }
}
