using CurrencyExchange.Models;
using Microsoft.EntityFrameworkCore;

namespace CurrencyExchange.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        { }
        public DbSet<NbuData> NbuDatas {get; set;}
    }
}
