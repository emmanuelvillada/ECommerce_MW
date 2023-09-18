namespace ECommerce_MW.DAL.Entities
{
    public class ProductCategory : Entity
    {
        public Product Product { get; set; }
        public Category Category { get; set; }
    }
}
