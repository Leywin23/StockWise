using Microsoft.EntityFrameworkCore;
using StockWise.Models;

namespace StockWise.Data
{
    public class StockWiseDb: DbContext
    {
        public StockWiseDb(DbContextOptions<StockWiseDb> options): base(options){}

        public DbSet<Product> products { get; set; }
    }
}
