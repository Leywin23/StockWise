namespace StockWise.Models
{
    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public long NIP { get; set; }
        public string Address { get; set; }
        public string Email {  get; set; }
        public string Phone { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool Verified {  get; set; } = false;

        public ICollection<Order> OrdersAsBuyer { get; set; }
        public ICollection<Order> OrdersAsSeller { get; set; }
        public ICollection<AppUser> Users {  get; set; }
        public ICollection<CompanyProduct> CompanyProducts { get; set; }
    }
}
