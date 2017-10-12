using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;
using Exercise.Data;
using Exercise.Domain.Services;
using Exercise.Data.BusinessLogic;
using Microsoft.EntityFrameworkCore;

namespace SomeUI
{
    class Program
    {
        public static void Main(string[] args)
        {
            // create service collection
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            // create service provider
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // entry to run app
            serviceProvider.GetService<App>().Run();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // add logging
            services.AddSingleton(new LoggerFactory()
                .AddConsole()
                .AddDebug());
            services.AddLogging();

            // build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .Build();

            services.AddOptions();

            services.Configure<AppSettings>(configuration.GetSection("Configuration"));

            var connectionString = configuration["ConnectionStrings:ProdDatabase"] ?? "Server = (localdb)\\mssqllocaldb; Database = MeetingDb; Trusted_Connection = True;";
            System.Console.WriteLine(connectionString);

            services.AddDbContext<MeetingContext>(options => options.UseSqlServer(connectionString, o => o.MaxBatchSize(30)).EnableSensitiveDataLogging());

            // add services
            services.AddTransient<IJsonService, JsonService>();

            services.AddTransient<ISaveStrategy, SaveWithDelete>();

            services.AddTransient<ISaveStrategy, SaveWithUpdate>();

            //services.AddTransient<MeetingService>();

            // add app
            services.AddTransient<App>();
        }
    }
}
