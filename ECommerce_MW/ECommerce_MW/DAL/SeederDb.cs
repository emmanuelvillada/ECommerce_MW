using ECommerce_MW.DAL.Entities;
using ECommerce_MW.Enums;
using ECommerce_MW.Helpers;

namespace ECommerce_MW.DAL
{
    public class SeederDb
    {
        private readonly DatabaseContext _context;
        private readonly IUserHelper _userHelper;

        public SeederDb(DatabaseContext context, IUserHelper userHelper)
        {
            _context = context;
            _userHelper = userHelper;
        }

        public async Task SeederAsync()
        {
            var email = "docente_carlos@yopmail.com";

            await _context.Database.EnsureCreatedAsync(); // me reemplaza el comando update-database
            await PopulateCategoriesAsync();
            await PopulateCountriesAsync();
            await PopulateRolesAsync();
            await PopulateUserAsync(email);

            await _context.SaveChangesAsync();
        }

        private async Task PopulateCategoriesAsync()
        {
            if (!_context.Categories.Any())
            {
                _context.Categories.Add(new Category { Name = "Tecnología", Description = "Elementos tech" , CreatedDate = DateTime.Now });
                _context.Categories.Add(new Category { Name = "Implementos de Aseo", Description = "Detergente, jabón, etc.", CreatedDate = DateTime.Now });
                _context.Categories.Add(new Category { Name = "Ropa interior", Description = "Tanguitas, narizonas", CreatedDate = DateTime.Now });
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

        private async Task PopulateUserAsync(string email)
        {
            User user = await _userHelper.GetUserAsync(email);

            if (user == null)
            {
                user = new User
                {
                    CreatedDate = DateTime.Now,
                    FirstName = "Docente Carlos",
                    LastName = "Diaz",
                    Email = email,
                    UserName = email,
                    PhoneNumber = "3002002002",
                    Address = "Street Fighter",
                    Document = "102030",
                    City = _context.Cities.FirstOrDefault(),
                    UserType = UserType.Admin,
                };

                await _userHelper.AddUserAsync(user, "123456");
                //await _userHelper.AddUserToRoleAsync(user, user.UserType.ToString());
                await _userHelper.AddUserToRoleAsync(user, UserType.Admin.ToString());
            }
        }
    }
}
