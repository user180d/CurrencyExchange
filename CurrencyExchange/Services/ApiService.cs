using CurrencyExchange.Interfaces;
using CurrencyExchange.Models;
using Newtonsoft.Json;

namespace CurrencyExchange.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        //Simply get all data from Web Api as string by url
        private async Task<string> GetHttpResponseAsync(string apiUrl)
        {
            var response = await _httpClient.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode(); // Throws if not successful

            return await response.Content.ReadAsStringAsync();
        }

        // Get all data from Web API as string by URL
        public async Task<string> GetApiResponseAsync(string apiUrl)
        {
            try
            {
                return await GetHttpResponseAsync(apiUrl);
            }
            catch (HttpRequestException ex)
            {
                // Return a detailed error message for easier debugging
                return $"Error: {ex.Message}";
            }
        }

        // Get data from Web API using a generic type
        public async Task<List<T>> GetApiDataAsync<T>(string url, bool returnEmptyOnFailure = true)
        {
            try
            {
                var jsonResponse = await GetHttpResponseAsync(url);
                return JsonConvert.DeserializeObject<List<T>>(jsonResponse) ?? new List<T>();
            }
            catch (JsonException ex)
            {
                // Log or handle the deserialization error here if needed
                if (!returnEmptyOnFailure)
                    throw; // Re-throw exception if desired

                return new List<T>(); // Return empty list if deserialization fails
            }
            catch (HttpRequestException)
            {
                // Optionally rethrow or return an empty list if HTTP request fails
                return new List<T>();
            }
        }
    }
}
