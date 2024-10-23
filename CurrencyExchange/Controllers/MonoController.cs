using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using CurrencyExchange.Interfaces;
using CurrencyExchange.Models;
namespace CurrencyExchange.Controllers
{
    [Route("rates/[controller]")]
    [ApiController]
    public class MonoController : ControllerBase
    {
        private readonly IApiService _apiService;
        private readonly ILogger<MonoController> _logger;
        public MonoController(IApiService apiService, ILogger<MonoController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }
        [HttpPost]
        public async Task<IActionResult> CheckDiferenceBetweenMonoAndNbu([FromBody] int currencyCode)
        {
            // API URLs as constants
            const string monoApiUrl = "https://api.monobank.ua/bank/currency";
            const string nbuApiUrl = "https://bank.gov.ua/NBUStatService/v1/statdirectory/exchange?json";

            try
            {
                // Fetch data from Monobank API
                var resultMono = await _apiService.GetApiDataAsync<MonoData>(monoApiUrl);
                if (resultMono == null || !resultMono.Any())
                {
                    return StatusCode(503, "Failed to retrieve data from Monobank.");
                }

                // Fetch data from NBU API
                var resultNbu = await _apiService.GetApiDataAsync<NbuData>(nbuApiUrl);
                if (resultNbu == null || !resultNbu.Any())
                {
                    return StatusCode(503, "Failed to retrieve data from NBU.");
                }

                // Extract the sell rate for the specified currency from Monobank
                float monoRate = resultMono.FirstOrDefault(c => c.currencyCodeA == currencyCode)?.rateSell ?? 0;
                if (monoRate == 0)
                {
                    return NotFound($"Monobank rate for currency {currencyCode} not found.");
                }

                // Extract the rate for the specified currency from NBU
                float nbuRate = resultNbu.FirstOrDefault(c => c.cc == currencyCode.ToString())?.rate ?? 0;
                if (nbuRate == 0)
                {
                    return NotFound($"NBU rate for currency {currencyCode} not found.");
                }

                // Calculate the difference between the two rates
                float difference = monoRate - nbuRate;

                // Return the rates and the difference
                return Ok(new
                {
                    MonoRate = monoRate,
                    NbuRate = nbuRate,
                    Difference = difference
                });
            }
            catch (Exception ex)
            {
                // Return 500 Internal Server Error with exception message
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

    }
}
