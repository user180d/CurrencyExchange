using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using CurrencyExchange.Interfaces;
using CurrencyExchange.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace CurrencyExchange.Controllers
{
    [Route("rates/[controller]")]
    [ApiController]
    public class MonoUpToDateController : ControllerBase
    {
        private readonly IApiService _apiService;
        private readonly ILogger<MonoController> _logger;
        public MonoUpToDateController(IApiService apiService, ILogger<MonoController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }
        [HttpGet]
        // https://api.monobank.ua/bank/currency
        public async Task<IActionResult> FetchDataFromMonoApi()
        {
            //Monobank api url for currency exchnge rate get 
            string apiUrl = "https://api.monobank.ua/bank/currency"; 
            //Getting result of GET request to monobank api
            var result = await _apiService.GetApiDataAsync<MonoData>(apiUrl);
            //deserelization of json response from api
            
            if (long.TryParse(result[0].date, out long unixTime))
            {
                return Ok(new
                {
                    Date = "This data is current as of " + DateTimeOffset.FromUnixTimeSeconds(unixTime).UtcDateTime
                });
            }
            else
            {
                return Ok(new { Date = "Error"});
            }
            
        }

    }
}
