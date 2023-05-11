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

        public async Task<IActionResult> Edit(Guid? productId)
        {
            if (productId == null) return NotFound();

            Product product = await _context.Products.FindAsync(productId);
            if (product == null) return NotFound();

            EditProductViewModel editProductViewModel = new()
            {
                Description = product.Description,
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Stock = product.Stock,
            };

            return View(editProductViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid? Id, EditProductViewModel editProductViewModel)
        {
            if (Id != editProductViewModel.Id) return NotFound();

            try
            {
                Product product = await _context.Products.FindAsync(editProductViewModel.Id);
                product.Description = editProductViewModel.Description;
                product.Name = editProductViewModel.Name;
                product.Price = editProductViewModel.Price;
                product.Stock = editProductViewModel.Stock;
                _context.Update(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException dbUpdateException)
            {
                if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                    ModelState.AddModelError(string.Empty, "Ya existe un producto con el mismo nombre.");
                else
                    ModelState.AddModelError(string.Empty, dbUpdateException.InnerException.Message);
            }
            catch (Exception exception)
            {
                ModelState.AddModelError(string.Empty, exception.Message);
            }

            return View(editProductViewModel);
        }

        public async Task<IActionResult> Details(Guid? productId)
        {
            if (productId == null) return NotFound();

            Product product = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
                .FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null) return NotFound();

            return View(product);
        }

        public async Task<IActionResult> AddImage(Guid? productId)
        {
            if (productId == null) return NotFound();

            Product product = await _context.Products.FindAsync(productId);
            if (product == null) return NotFound();

            AddProductImageViewModel addProductImageViewModel = new()
            {
                ProductId = product.Id,
            };

            return View(addProductImageViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddImage(AddProductImageViewModel addProductImageViewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Guid imageId = await _azureBlobHelper.UploadAzureBlobAsync(addProductImageViewModel.ImageFile, "products");

                    Product product = await _context.Products.FindAsync(addProductImageViewModel.ProductId);
                
                    ProductImage productImage = new()
                    {
                        Product = product,
                        ImageId = imageId,
                    };

                    _context.Add(productImage);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Details), new { productId = product.Id });
                }
                catch (Exception exception)
                {
                    ModelState.AddModelError(string.Empty, exception.Message);
                }
            }

            return View(addProductImageViewModel);
        }

        public async Task<IActionResult> DeleteImage(Guid? imageId)
        {
            if (imageId == null) return NotFound();

            ProductImage productImage = await _context.ProductImages
                .Include(pi => pi.Product)
                .FirstOrDefaultAsync(pi => pi.Id == imageId);
            
            if (productImage == null) return NotFound();

            await _azureBlobHelper.DeleteAzureBlobAsync(productImage.ImageId, "products");
            _context.ProductImages.Remove(productImage);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { productId = productImage.Product.Id });
        }

        public async Task<IActionResult> AddCategory(Guid? productId)
        {
            if (productId == null) return NotFound();

            Product product = await _context.Products.FindAsync(productId);
            if (product == null) return NotFound();

            AddProductCategoryViewModel addCategoryProductViewModel = new()
            {
                ProductId = product.Id,
                Categories = await _dropDownListsHelper.GetDDLCategoriesAsync(),
            };

            return View(addCategoryProductViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCategory(AddProductCategoryViewModel addCategoryProductViewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Product product = await _context.Products.FindAsync(addCategoryProductViewModel.ProductId);
                    Category category = await _context.Categories.FindAsync(addCategoryProductViewModel.CategoryId);
                
                    if (category == null || product == null) return NotFound();

                    ProductCategory productCategory = new()
                    {
                        Category = category,
                        Product = product
                    };

                    _context.Add(productCategory);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Details), new { productId = product.Id });
                }
                catch (Exception exception)
                {
                    addCategoryProductViewModel.Categories = await _dropDownListsHelper.GetDDLCategoriesAsync();
                    ModelState.AddModelError(string.Empty, exception.Message);
                }
            }

            addCategoryProductViewModel.Categories = await _dropDownListsHelper.GetDDLCategoriesAsync();
            return View(addCategoryProductViewModel);
        }

        public async Task<IActionResult> DeleteCategory(Guid? categoryId)
        {
            if (categoryId == null) return NotFound();

            ProductCategory productCategory = await _context.ProductCategories
                .Include(pc => pc.Product)
                .FirstOrDefaultAsync(pc => pc.Id == categoryId);
            if (productCategory == null) return NotFound();

            _context.ProductCategories.Remove(productCategory);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { productId = productCategory.Product.Id });
        }



    }
}
