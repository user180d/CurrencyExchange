using CurrencyExchange.Data;
using CurrencyExchange.Interfaces;
using CurrencyExchange.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace CurrencyExchange.Controllers
{

    [Route("nbu/[controller]")]
    [ApiController]
    public class ConvertController : ControllerBase
    {
        private readonly IApiService _apiService;
        private readonly DataContext _dataContext;
        private readonly ILogger<ConvertController> _logger;

        public ConvertController(IApiService apiService, ILogger<ConvertController> logger, DataContext dataContext)
        {
            _apiService = apiService;
            _logger = logger;
            _dataContext = dataContext;
        }

        // Method to find currency rate in the database by code
        [NonAction]
        private async Task<float?> GetRateFromDbAsync(int currencyCode)
        {
            var rateData = await _dataContext.NbuDatas.FirstOrDefaultAsync(c => c.r030 == currencyCode);
            return rateData?.rate;
        }

        // Method to fetch currency rates from Monobank API
        [NonAction]
        private async Task<(float rateAtoUAH, float rateBtoUAH)> FetchRatesFromApiAsync(int currencyCodeA, int currencyCodeB, string url)
        {
            var currencyData = await _apiService.GetApiDataAsync<MonoData>(url);

            // If currencyCodeA is UAH (980), set the rate to 1. No need to fetch from API.
            float rateAtoUAH = currencyCodeA == 980 ? 1 : currencyData
                .FirstOrDefault(c => c.currencyCodeA == currencyCodeA && c.currencyCodeB == 980)?.rateSell ?? 0;

            // If currencyCodeB is UAH (980), set the rate to 1. No need to fetch from API.
            float rateBtoUAH = currencyCodeB == 980 ? 1 : currencyData
                .FirstOrDefault(c => c.currencyCodeA == currencyCodeB && c.currencyCodeB == 980)?.rateSell ?? 0;

            return (rateAtoUAH, rateBtoUAH);
        }

        // Fallback method for retrieving rates from the database
        [NonAction]
        private async Task<(float rateAtoUAH, float rateBtoUAH)> FetchRatesFromDbAsync(int currencyCodeA, int currencyCodeB)
        {

            // If one of the currencies is UAH (code 980), we don't need to fetch its rate
            float rateAtoUAH = currencyCodeA == 980 ? 1 : await GetRateFromDbAsync(currencyCodeA) ?? 0;
            float rateBtoUAH = currencyCodeB == 980 ? 1 : await GetRateFromDbAsync(currencyCodeB) ?? 0;

            return (rateAtoUAH, rateBtoUAH);
        }

        // Method to calculate the conversion rate between two currencies via UAH
        [NonAction]
        private float CalculateConversionRate(float rateAtoUAH, float rateBtoUAH)
        {
            return rateAtoUAH / rateBtoUAH;
        }

        // Main conversion method with refactored logic
        [HttpPost]
        public async Task<IActionResult> ConvertCurrencyAsync([FromBody] ConvertData model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Id1.ToString()) || string.IsNullOrWhiteSpace(model.Id2.ToString()))
            {
                return BadRequest("Invalid input data.");
            }

            float rateAtoUAH, rateBtoUAH;
            try
            {
                // Try fetching exchange rates from Monobank API
                (rateAtoUAH, rateBtoUAH) = await FetchRatesFromApiAsync(model.Id1, model.Id2, "https://api.monobank.ua/bank/currency");
                if (rateAtoUAH == 0 || rateBtoUAH == 0)
                {
                    throw new Exception("API returned incomplete data.");
                }
            }
            catch (Exception ex)
            {
                // If the API call fails, log the error and fallback to the database
                _logger.LogError(ex, "Error fetching data from Monobank API. Fallback to database.");
                (rateAtoUAH, rateBtoUAH) = await FetchRatesFromDbAsync(model.Id1, model.Id2);

                if (rateAtoUAH == 0 || rateBtoUAH == 0)
                {
                    return StatusCode(500, "Could not retrieve currency rates from either API or database.");
                }
            }

            // Calculate the conversion rate between CurrencyA and CurrencyB via UAH
            var conversionRate = CalculateConversionRate(rateAtoUAH, rateBtoUAH);

            // Return the converted amount
            var convertedAmount = model.Sum * conversionRate;

            return Ok(new
            {
                ConversionRate = conversionRate,
                ConvertedAmount = convertedAmount
            });
        }
        [HttpGet]
        public async Task<IActionResult> GetDataFromNBUasync()
        {
            // Get the API response
            var response = await _apiService.GetApiResponseAsync("https://api.monobank.ua/bank/currency");
            try
            {
                // Check if the API response contains an error or is null/empty
                if (string.IsNullOrWhiteSpace(response) || response.StartsWith("Error:"))
                {
                    // Fallback to the database if API response is null, empty, or contains an error
                    var exchangeRates = await _dataContext.NbuDatas.ToListAsync();
                    return Ok(JsonConvert.SerializeObject(exchangeRates));
                }
            }
            catch (Exception ex)
            {
                // Return 500 Internal Server Error with exception message
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }

            return Ok(response);
        }
    }

}

