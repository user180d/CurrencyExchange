namespace CurrencyExchange.Interfaces
{
    public interface IApiService
    {
        Task<string> GetApiResponseAsync(string apiUrl);
        Task<List<T>> GetApiDataAsync<T>(string url, bool returnEmptyOnFailure = true);
    }
}
