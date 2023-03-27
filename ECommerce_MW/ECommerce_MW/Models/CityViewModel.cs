using ECommerce_MW.DAL.Entities;

namespace ECommerce_MW.Models
{
    public class CityViewModel : City
    {
        public Guid StateId { get; set; }
    }
}
