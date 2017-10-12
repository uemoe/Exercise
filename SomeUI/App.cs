using Exercise.Data;
using Exercise.Data.BusinessLogic;
using Exercise.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SomeUI
{
    public class App
    {
        private readonly IJsonService _jsonService;
        private readonly ILogger<App> _logger;
        private readonly AppSettings _config;
        private readonly ISaveStrategy _strategy;
        private readonly DbContextOptions _options;

        public App(IJsonService jsonService,
            IOptions<AppSettings> config,
            ILogger<App> logger,
            ISaveStrategy strategy,
            DbContextOptions options
            )
        {
            _jsonService = jsonService;
            _logger = logger;
            _config = config.Value;
            _strategy = strategy;
            _options = options;
        }

        public void Run()
        {
            _logger.LogInformation($"This is a console application for {_config.Title}");

            _logger.LogInformation("Begin parsing jsonFile");

            var raceData = _jsonService.ParseOriginalSample();
            System.Console.WriteLine(raceData);

            _logger.LogInformation("End parsing");

            _logger.LogInformation("Begin transforming to domain entity");

            var meeting = _jsonService.Transform(raceData);

            _logger.LogInformation("End transforming");

            using (var context = new MeetingContext(_options))
            {
                context.Database.EnsureCreated();
                var service = new MeetingService(context, _strategy);
                service.SaveMeeting(meeting);
                context.SaveChanges();
            }

            var meeting2 = _jsonService.Transform(raceData);
            using (var context = new MeetingContext(_options))
            {
                var service = new MeetingService(context, _strategy);
                service.SaveMeeting(meeting2);
                context.SaveChanges();
            }

            System.Console.ReadKey();
        }
    }
}
