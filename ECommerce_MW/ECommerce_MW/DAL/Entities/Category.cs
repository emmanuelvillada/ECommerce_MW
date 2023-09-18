using System.ComponentModel.DataAnnotations;

namespace ECommerce_MW.DAL.Entities
{
    public class Category : Entity
    {
        [Display(Name = "Categoría")]
        [MaxLength(50, ErrorMessage = "El campo {0} debe ser de {1} caracteres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Name { get; set; }

        [Display(Name = "Descripción")]
        [MaxLength(500, ErrorMessage = "El campo {0} debe ser de {1} caracteres.")]
        public string? Description { get; set; }

        public ICollection<ProductCategory> ProductCategories { get; set; }

        [Display(Name = "Productos")]
        public int ProductsNumber => ProductCategories == null ? 0 : ProductCategories.Count;
    }
}
