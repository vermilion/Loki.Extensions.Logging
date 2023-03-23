namespace Loki.Extensions.Logging.Samples.AspNetCore5
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseStartup<Startup>()
                        .ConfigureLogging((context, loggingBuilder) =>
                        {
                            loggingBuilder.AddLoki(options =>
                            {
                                options.IncludeScopes = false;
                                options.ApplicationName = context.HostingEnvironment.ApplicationName;
                            });
                        });
                });
        }
    }
}
