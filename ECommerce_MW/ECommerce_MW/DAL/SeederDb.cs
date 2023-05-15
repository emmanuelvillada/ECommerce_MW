using ECommerce_MW.DAL.Entities;
using ECommerce_MW.Enums;
using ECommerce_MW.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ECommerce_MW.DAL
{
    public class SeederDb
    {
        private readonly DatabaseContext _context;
        private readonly IUserHelper _userHelper;
        private readonly IAzureBlobHelper _azureBlobHelper;

        public SeederDb(DatabaseContext context, IUserHelper userHelper, IAzureBlobHelper azureBlobHelper)
        {
            _context = context;
            _userHelper = userHelper;
            _azureBlobHelper = azureBlobHelper;
        }

        public async Task SeederAsync()
        {
            await _context.Database.EnsureCreatedAsync(); // me reemplaza el comando update-database
            await PopulateCategoriesAsync();
            await PopulateCountriesAsync();
            await PopulateRolesAsync();
            await PopulateUserAsync("Steve", "Jobs", "steve_jobs_admin@yopmail.com", "3002323232", "Street Apple", "102030", "SteveJobs.png", UserType.Admin);
            await PopulateUserAsync("Bill", "Gates", "bill_gates_admin@yopmail.com", "4005656656", "Street Microsoft", "405060", "BillGates.png", UserType.User);
            await PopulateProductsAsync();

			await _context.SaveChangesAsync();
        }

        private async Task PopulateCategoriesAsync()
        {
            if (!_context.Categories.Any())
            {
                _context.Categories.Add(new Category { Name = "Tecnología", Description = "Elementos tech" , CreatedDate = DateTime.Now });
                _context.Categories.Add(new Category { Name = "Implementos de Aseo", Description = "Detergente, jabón, etc.", CreatedDate = DateTime.Now });
                _context.Categories.Add(new Category { Name = "Ropa interior", Description = "Tanguitas, narizonas", CreatedDate = DateTime.Now });
                _context.Categories.Add(new Category { Name = "Calzado", Description = "Calcetines, zapatos, etc.", CreatedDate = DateTime.Now });
                _context.Categories.Add(new Category { Name = "Gamers", Description = "PS5, XBOX SERIES", CreatedDate = DateTime.Now });
                _context.Categories.Add(new Category { Name = "Mascotas", Description = "Concentrado, jabón para pulgas.", CreatedDate = DateTime.Now });
            }
        }
        private async Task PopulateCountriesAsync()
        {
            if (!_context.Countries.Any())
            {
                _context.Countries.Add(new Country
                {
                    CreatedDate = DateTime.Now,
                    Name = "Colombia",
                    States = new List<State>()
                    {
                        new State
                        {
                            CreatedDate = DateTime.Now,
                            Name = "Antioquia",
                            Cities = new List<City>()
                            {
                                new City { Name = "Medellín", CreatedDate = DateTime.Now },
                                new City { Name = "Envigado", CreatedDate = DateTime.Now },
                                new City { Name = "Bello", CreatedDate = DateTime.Now },
                                new City { Name = "Itagüí", CreatedDate = DateTime.Now },
                                new City { Name = "Barbosa", CreatedDate = DateTime.Now },
                                new City { Name = "Copacabana", CreatedDate = DateTime.Now },
                                new City { Name = "Girardota", CreatedDate = DateTime.Now },
                                new City { Name = "Sabaneta", CreatedDate = DateTime.Now },
                            }
                        },

                        new State
                        {
                            CreatedDate = DateTime.Now,
                            Name = "Cundinamarca",
                            Cities = new List<City>()
                            {
                                new City { Name = "Bogotá", CreatedDate = DateTime.Now },
                                new City { Name = "Engativá", CreatedDate = DateTime.Now },
                                new City { Name = "Fusagasugá", CreatedDate = DateTime.Now },
                                new City { Name = "Villeta", CreatedDate = DateTime.Now },
                            }
                        }
                    }
                });

                _context.Countries.Add(new Country
                {
                    CreatedDate = DateTime.Now,
                    Name = "Argentina",
                    States = new List<State>()
                    {
                        new State
                        {
                            CreatedDate = DateTime.Now,
                            Name = "Buenos Aires",
                            Cities = new List<City>()
                            {
                                new City { Name = "Alberti", CreatedDate = DateTime.Now },
                                new City { Name = "Avellaneda", CreatedDate = DateTime.Now },
                                new City { Name = "Bahía Blanca", CreatedDate = DateTime.Now },
                                new City { Name = "Ezeiza", CreatedDate = DateTime.Now },
                            }
                        },

                        new State
                        {
                            Name = "La Pampa",
                            Cities = new List<City>()
                            {
                                new City { Name = "Parera", CreatedDate = DateTime.Now },
                                new City { Name = "Santa Isabel", CreatedDate = DateTime.Now },
                                new City { Name = "Puelches", CreatedDate = DateTime.Now },
                                new City { Name = "La Adela", CreatedDate = DateTime.Now },
                            }
                        }
                    }
                });
            }
        }

        private async Task PopulateRolesAsync()
        {
            await _userHelper.CheckRoleAsync(UserType.Admin.ToString());
            await _userHelper.CheckRoleAsync(UserType.User.ToString());
        }

        private async Task PopulateUserAsync(
            string firstName,
            string lastName,
            string email,
            string phone,
            string address,
            string document,
            string image,
            UserType userType)
        {
            User user = await _userHelper.GetUserAsync(email);

            if (user == null)
            {
                Guid imageId = await _azureBlobHelper.UploadAzureBlobAsync
                    ($"{Environment.CurrentDirectory}\\wwwroot\\images\\users\\{image}", "users");
                
                user = new User
                {
                    CreatedDate = DateTime.Now,
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    UserName = email,
                    PhoneNumber = phone,
                    Address = address,
                    Document = document,
                    City = _context.Cities.FirstOrDefault(),
                    UserType = userType,
                    ImageId = imageId
                };

                await _userHelper.AddUserAsync(user, "123456");
                await _userHelper.AddUserToRoleAsync(user, userType.ToString());
            }
        }

        private async Task PopulateProductsAsync()
        {
            if (!_context.Products.Any())
            {
                await AddProductAsync("Medias Grises", 270000M, 12F, new List<string>() { "Ropa Interior", "Calzado" }, new List<string>() { "Medias1.png" });
                await AddProductAsync("Medias Negras", 300000M, 12F, new List<string>() { "Ropa Interior", "Calzado" }, new List<string>() { "Medias2.png" });
                await AddProductAsync("TV Samsung OLED", 5000000M, 12F, new List<string>() { "Tecnología", "Gamers" }, new List<string>() { "TvOled.png", "TvOled2.png" });
                await AddProductAsync("Play Station 5", 5000000M, 12F, new List<string>() { "Gamers" }, new List<string>() { "PS5.png", "PS52.png" });
                await AddProductAsync("Bull Dog Francés", 10000000M, 12F, new List<string>() { "Mascotas" }, new List<string>() { "Frenchie1.png", "Frenchie2.png", "Frenchie3.png" });
                await AddProductAsync("Cepillo de dientes", 5000M, 12F, new List<string>() { "Implementos de Aseo" }, new List<string>() { "CepilloDientes.png" });
                await AddProductAsync("Crema dental Pro Alivio", 25000M, 12F, new List<string>() { "Implementos de Aseo" }, new List<string>() { "CremaDental1.png", "CremaDental2.png" });
            }
        }

        private async Task AddProductAsync(string name, decimal price, float stock, List<string> categories, List<string> images)
        {
            Product product = new()
            {
                Description = name,
                Name = name,
                Price = price,
                Stock = stock,
                ProductCategories = new List<ProductCategory>(),
                ProductImages = new List<ProductImage>()
            };

            foreach (string? category in categories)
            {
                product.ProductCategories.Add(new ProductCategory { Category = await _context.Categories.FirstOrDefaultAsync(c => c.Name == category) });
            }


            foreach (string? image in images)
            {
                Guid imageId = await _azureBlobHelper.UploadAzureBlobAsync($"{Environment.CurrentDirectory}\\wwwroot\\images\\products\\{image}", "products");
                product.ProductImages.Add(new ProductImage { ImageId = imageId });
            }

            _context.Products.Add(product);
        }
    }
}
