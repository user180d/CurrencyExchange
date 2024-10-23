namespace CurrencyExchange.Models
{
    public class MonoData
    {
        public int currencyCodeA { get; set; }

        public int currencyCodeB { get; set; }

        public string date { get; set; }

        public float rateSell { get; set; }

        public float rateBuy { get; set; }

    }
}
