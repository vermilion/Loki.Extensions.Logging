using Microsoft.AspNetCore.Mvc;

namespace Loki.Extensions.Logging.Samples.AspNetCore5.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            _logger.LogTrace("A trace level log");
            _logger.LogDebug("A debug level log");
            _logger.LogInformation("A info level log");
            _logger.LogWarning("A warn level log");
            _logger.LogError("A error level log");
            _logger.LogCritical("A critical level log");

            _logger.LogInformation("Getting weather at {weather_time}", DateTime.Now);

            try
            {
                throw new NotImplementedException("eeeee");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error happened");
            }


            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
                .ToArray();
        }
    }
}
