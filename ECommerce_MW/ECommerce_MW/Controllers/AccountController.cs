using ECommerce_MW.DAL;
using ECommerce_MW.DAL.Entities;
using ECommerce_MW.Enums;
using ECommerce_MW.Helpers;
using ECommerce_MW.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce_MW.Controllers
{
	public class AccountController : Controller
	{
		private readonly IUserHelper _userHelper;
		private readonly DatabaseContext _context;
		private readonly IDropDownListsHelper _dropDownListsHelper;
		private readonly IAzureBlobHelper _azureBlobHelper;

		public AccountController(IUserHelper userHelper, DatabaseContext context, IDropDownListsHelper dropDownListsHelper, IAzureBlobHelper azureBlobHelper)
		{
			_userHelper = userHelper;
			_context = context;
			_dropDownListsHelper = dropDownListsHelper;
			_azureBlobHelper = azureBlobHelper;
		}

		[HttpGet]
		public IActionResult Login()
		{
			if (User.Identity.IsAuthenticated)
			{
				return RedirectToAction("Index", "Home");
			}

			return View(new LoginViewModel());
		}

		[HttpPost]
		public async Task<IActionResult> Login(LoginViewModel loginViewModel)
		{
			if (ModelState.IsValid)
			{
				Microsoft.AspNetCore.Identity.SignInResult result = await _userHelper.LoginAsync(loginViewModel);
				if (result.Succeeded)
				{
					return RedirectToAction("Index", "Home");
				}

				ModelState.AddModelError(string.Empty, "Email o contraseña incorrectos.");
			}
			return View(loginViewModel);
		}

		public async Task<IActionResult> Logout()
		{
			await _userHelper.LogoutAsync();
			return RedirectToAction("Index", "Home");
		}

		public IActionResult Unauthorized()
		{
			return View();
		}

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            Guid emptyGuid = new Guid();
            AddUserViewModel addUserViewModel = new()
            {
                Id = Guid.Empty,
                Countries = await _dropDownListsHelper.GetDDLCountriesAsync(),
                States = await _dropDownListsHelper.GetDDLStatesAsync(emptyGuid),
                Cities = await _dropDownListsHelper.GetDDLCitiesAsync(emptyGuid),
                UserType = UserType.User,
            };

            return View(addUserViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(AddUserViewModel addUserViewModel)
        {
            if (ModelState.IsValid)
            {
                Guid imageId = Guid.Empty;

                if (addUserViewModel.ImageFile != null)
                    imageId = await _azureBlobHelper.UploadAzureBlobAsync(addUserViewModel.ImageFile, "users");

                addUserViewModel.ImageId = imageId;
                User user = await _userHelper.AddUserAsync(addUserViewModel);

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Este correo ya está siendo usado.");
                    return View(addUserViewModel);
                }

                //Autologeamos al nuevo usuario que se registra
                LoginViewModel loginViewModel = new()
                {
                    Password = addUserViewModel.Password,
                    RememberMe = false,
                    Username = addUserViewModel.Username
                };

                var login = await _userHelper.LoginAsync(loginViewModel);

                if (login.Succeeded) return RedirectToAction("Index", "Home");
            }

            return View(addUserViewModel);
        }

        public JsonResult GetStates(Guid countryId)
        {
            Country country = _context.Countries
                .Include(c => c.States)
                .FirstOrDefault(c => c.Id == countryId);

            if (country == null) return null;

            var list = country.States.OrderBy(d => d.Name);
            return Json(list);
        }

        public JsonResult GetCities(Guid stateId)
        {
            State state = _context.States
                .Include(s => s.Cities)
                .FirstOrDefault(s => s.Id == stateId);
            
            if (state == null) return null;

            return Json(state.Cities.OrderBy(c => c.Name));
        }

    }
}
