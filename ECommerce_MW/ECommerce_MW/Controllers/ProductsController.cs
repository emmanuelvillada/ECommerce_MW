using ECommerce_MW.DAL;
using ECommerce_MW.DAL.Entities;
using ECommerce_MW.Helpers;
using ECommerce_MW.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace ECommerce_MW.Controllers
{
    [Authorize(Roles = "Admin")]
    [AllowAnonymous]
    public class ProductsController : Controller
    {
        private readonly DatabaseContext _context;
        private readonly IDropDownListsHelper _dropDownListsHelper;
        private readonly IAzureBlobHelper _azureBlobHelper;

        public ProductsController(DatabaseContext context, IDropDownListsHelper dropDownListsHelper, IAzureBlobHelper azureBlobHelper)
        {
            _context = context;
            _dropDownListsHelper = dropDownListsHelper;
            _azureBlobHelper = azureBlobHelper;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
                .ToListAsync());
        }

        public async Task<IActionResult> Create()
        {
            AddProductViewModel addProductViewModel = new()
            {
                Categories = await _dropDownListsHelper.GetDDLCategoriesAsync(),
            };

            return View(addProductViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddProductViewModel addProductViewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Guid imageId = Guid.Empty;

                    if (addProductViewModel.ImageFile != null)
                        imageId = await _azureBlobHelper.UploadAzureBlobAsync(addProductViewModel.ImageFile, "products");

                    Product product = new()
                    {
                        Description = addProductViewModel.Description,
                        Name = addProductViewModel.Name,
                        Price = addProductViewModel.Price,
                        Stock = addProductViewModel.Stock,
                    };

                    //Estoy capturando la categoría del prod para luego guardar esa relación en la tabla ProductCategories
                    product.ProductCategories = new List<ProductCategory>()
                    {
                        new ProductCategory
                        {
                            Category = await _context.Categories.FindAsync(addProductViewModel.CategoryId)
                        }
                    };

                    if (imageId != Guid.Empty)
                    {
                        product.ProductImages = new List<ProductImage>()
                        {
                            new ProductImage { ImageId = imageId }
                        };
                    }

                    _context.Add(product);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException dbUpdateException)
                {
                    if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, "Ya existe un producto con el mismo nombre.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, dbUpdateException.InnerException.Message);
                    }
                }
                catch (Exception exception)
                {
                    ModelState.AddModelError(string.Empty, exception.Message);
                }
            }

            addProductViewModel.Categories = await _dropDownListsHelper.GetDDLCategoriesAsync();
            return View(addProductViewModel);
        }
    }
}
