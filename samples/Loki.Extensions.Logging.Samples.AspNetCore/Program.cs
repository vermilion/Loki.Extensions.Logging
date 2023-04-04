using System.Diagnostics;

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
                                options.Host = "10.10.25.223";

                                options.IncludeScopes = false;
                                options.ApplicationName = context.HostingEnvironment.ApplicationName;

                                options.AdditionalFieldsFactory = (level, scopeFields, ex) =>
                                {
                                    var result = new Dictionary<string, object>();
                                    if (scopeFields.ContainsKey(nameof(Activity.TraceId)))
                                    {
                                        result.Add(nameof(Activity.TraceId), scopeFields[nameof(Activity.TraceId)]);
                                    }

                                    return result;
                                };
                            });
                        });
                });
        }
    }
}
