using CalendarWatcherService.Options;
using Microsoft.Extensions.Options;
using Events.TimePointers;
using Telegram.Bot;
using CalendarWatcherService.Extensions;
using Events.Entities;
using Events;
using Events.EventSources;

namespace CalendarWatcherService
{
    public class CalendarWatcherService : BackgroundService
    {
        private readonly ILogger<CalendarWatcherService> _logger;
        private readonly AppSettings _settings;
        private readonly ITelegramBotClient _bot;


        public CalendarWatcherService(ILogger<CalendarWatcherService> logger, IOptions<AppSettings> options)
        {
            _logger = logger;
            _settings = options.Value;
            _bot = new TelegramBotClient(_settings.BotToken);
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using EventsTimer watcher = ConfigureEventsTimer();
            await watcher.StartAsync(stoppingToken);
        }
        private EventsTimer ConfigureEventsTimer()
        {
            CalendarEventSource source = new(_settings.CalendarUri, TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow"));
            List<NotificationsGroup> notificationsGroups = new() {
                new(
                    alarmAction: SendMessageForEachEvent,
                    notificationsTime: new IRelativeTimePointer[] { new TimeSpanPointer(TimeSpan.FromHours(-1))},
                    forEventWhere: @event => !@event.IsAllDay
                    ),
                new(
                    alarmAction: SendMessageForEachEvent,
                    notificationsTime: new IRelativeTimePointer[] { new DayTimePointer(new TimeOnly(9, 00)) },
                    forEventWhere: @event => @event.IsAllDay
                    ),
                new(
                    alarmAction: SendMessageForCheckList,
                    notificationsTime: new IRelativeTimePointer[] { new DayTimePointer(new TimeOnly(8, 00)), new DayTimePointer(new TimeOnly(15, 00), -1) }
                    )
            };
            EventsTimer watcher = new(source, notificationsGroups)
            {
                LeftSearchInterval = TimeSpan.FromDays(5),
                RightSearchInterval = TimeSpan.FromDays(5)
            };
            watcher.Error += HandleException;
            return watcher;
        }
        private async void HandleException(object? sender, ExceptionEventArgs e)
        {
            try
            {
                await _bot.SendTextMessageAsync(chatId: _settings.ManagerChatId, text: e.Exception.Message);
                _logger.LogError($"{e.Exception.Message} {DateTimeOffset.UtcNow}");
            }
            catch (Exception exception)
            {
                _logger.LogError($"{exception.Message} {DateTimeOffset.UtcNow}");
            }
        }
        private async void SendMessageForCheckList(Alarm alarm)
        {       
            try
            {
                string checkList = alarm.ToCheckList();
                await _bot.SendTextMessageAsync(chatId: _settings.ChatId, text: checkList);
                _logger.LogInformation(checkList);
            }
            catch (Exception exception)
            {
                _logger.LogError($"{exception.Message} {DateTimeOffset.UtcNow}");
            }
        }
        private async void SendMessageForEachEvent(Alarm alarm)
        {
            try
            {
                var messages = alarm.ToLongMessages();
                foreach (var message in messages)
                {
                    await _bot.SendTextMessageAsync(chatId: _settings.ChatId, text: message);
                    _logger.LogInformation(message);
                }
            }
            catch (Exception exception)
            {
                _logger.LogError($"{exception.Message} {DateTimeOffset.UtcNow}");
            }
        }
    }
}