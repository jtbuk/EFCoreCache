using Microsoft.EntityFrameworkCore;

namespace Jtbuk.EFCoreCache
{
    public class WeatherContext : DbContext
    {
        public WeatherContext(DbContextOptions<WeatherContext> options) : base(options) { }
        public DbSet<WeatherRecord> WeatherRecords { get; init; } = null!;
    }

    public class WeatherRecord
    {
        public int Id { get; set; }
        public int Temperature { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
