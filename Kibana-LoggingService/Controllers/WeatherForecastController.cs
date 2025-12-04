using Kibana_Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Kibana_LoggingService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly ILoggerService _loggerService;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, ILoggerService loggerService)
        {
            _logger = logger;
            _loggerService = loggerService;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            Debug.WriteLine("This is a test log message");
            _loggerService.LogDebug("Debug Weather forecast.");
            _loggerService.LogInformation("Information Weather forecast.");
            _loggerService.LogWarning("Warning Weather forecast.");
            try
            {
                var data = 1 / int.Parse("0");
            }
            catch (Exception ex)
            {
                _loggerService.LogError("Error Weather forecast.",ex);
                _loggerService.LogCritical("Critical Weather forecast.",ex);
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
