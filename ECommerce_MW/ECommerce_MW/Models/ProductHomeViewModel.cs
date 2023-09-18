using ECommerce_MW.DAL.Entities;

namespace ECommerce_MW.Models
{
    public class ProductHomeViewModel
    {
        public ICollection<Product> Products { get; set; }
    }
}
