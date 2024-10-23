using CurrencyExchange.Data;
using CurrencyExchange.Interfaces;
using CurrencyExchange.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Query;

namespace CurrencyExchange.Services
{
    public class CurrencyUpdateService : BackgroundService
    {
        private readonly ILogger<CurrencyUpdateService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public CurrencyUpdateService(ILogger<CurrencyUpdateService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        // Main method that will process instructions and update function
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Currency update service is running.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Calling 
                    await CheckAndUpdateCurrencyData(stoppingToken);

                    // Чекаємо 24 години перед наступним виконанням
                    await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    _logger.LogInformation("Currency update service is stopping.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while updating currency data.");
                }
            }
        }

        private async Task CheckAndUpdateCurrencyData(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Checking currency data...");

            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var apiService = scope.ServiceProvider.GetRequiredService<IApiService>();
                    var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();

                    // Отримуємо дані з API
                    var url = "https://bank.gov.ua/NBUStatService/v1/statdirectory/exchange?json";
                    var currencyData = await apiService.GetApiDataAsync<NbuData>(url);
                    
                    if (currencyData == null || currencyData.Count == 0)
                    {
                        _logger.LogWarning("No data returned from API.");
                        return;
                    }

                    // Остання дата обміну з API
                    var latestExchangeDate = currencyData[0].exchangedate;

                    // Отримуємо останню дату обміну з бази даних
                    var dbLatestDate = dbContext.NbuDatas.OrderByDescending(c => c.exchangedate).FirstOrDefault()?.exchangedate;

                    if (latestExchangeDate != dbLatestDate)
                    {
                        // Оновлюємо базу даних новими даними
                        foreach (var currency in currencyData)
                        {
                            var existingCurrency = dbContext.NbuDatas.FirstOrDefault(c => c.r030 == currency.r030);

                            if (existingCurrency != null)
                            {
                                existingCurrency.rate = currency.rate;
                                existingCurrency.exchangedate = currency.exchangedate;
                            }
                            else
                            {
                                dbContext.NbuDatas.Add(new NbuData
                                {
                                    r030 = currency.r030,
                                    rate = currency.rate,
                                    cc = currency.cc,
                                    txt = currency.txt,
                                    exchangedate = currency.exchangedate
                                });
                            }
                        }

                        await dbContext.SaveChangesAsync(cancellationToken);
                        _logger.LogInformation("Currency data updated.");
                    }
                    else
                    {
                        _logger.LogInformation("No update required.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating currency data.");
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Currency update service is stopping.");
            await base.StopAsync(stoppingToken);
        }
    }

}
