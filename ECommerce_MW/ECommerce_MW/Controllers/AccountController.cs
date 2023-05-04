﻿using ECommerce_MW.DAL;
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
		private readonly IDropDownListsHelper _ddlHelper;
		private readonly IAzureBlobHelper _azureBlobHelper;

		public AccountController(IUserHelper userHelper, DatabaseContext context, IDropDownListsHelper dropDownListsHelper, IAzureBlobHelper azureBlobHelper)
		{
			_userHelper = userHelper;
			_context = context;
            _ddlHelper = dropDownListsHelper;
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
            Guid emptyGuid = new Guid(); //New Guid for example: 1515fsaf-1215gas-1ga15-a41ga

            AddUserViewModel addUserViewModel = new()
            {
                Id = Guid.Empty,
                Countries = await _ddlHelper.GetDDLCountriesAsync(),
                States = await _ddlHelper.GetDDLStatesAsync(emptyGuid),
                Cities = await _ddlHelper.GetDDLCitiesAsync(emptyGuid),
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
                addUserViewModel.CreatedDate = DateTime.Now;

                User user = await _userHelper.AddUserAsync(addUserViewModel);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Este correo ya está siendo usado.");
                    await FillDropDownListLocation(addUserViewModel);
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

            await FillDropDownListLocation(addUserViewModel);
            return View(addUserViewModel);
        }

        private async Task FillDropDownListLocation(AddUserViewModel addUserViewModel)
        {
            addUserViewModel.Countries = await _ddlHelper.GetDDLCountriesAsync();
            addUserViewModel.States = await _ddlHelper.GetDDLStatesAsync(addUserViewModel.CountryId);
            addUserViewModel.Cities = await _ddlHelper.GetDDLCitiesAsync(addUserViewModel.StateId);
        }

        public async Task<IActionResult> EditUser()
        {
            User user = await _userHelper.GetUserAsync(User.Identity.Name);
            if (user == null) return NotFound();

            EditUserViewModel editUserViewModel = new()
            {
                Address = user.Address,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                ImageId = user.ImageId,
                Cities = await _ddlHelper.GetDDLCitiesAsync(user.City.State.Id),
                CityId = user.City.Id,
                Countries = await _ddlHelper.GetDDLCountriesAsync(),
                CountryId = user.City.State.Country.Id,
                StateId = user.City.State.Id,
                States = await _ddlHelper.GetDDLStatesAsync(user.City.State.Country.Id),
                Id = Guid.Parse(user.Id),
                Document = user.Document
            };

            return View(editUserViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(EditUserViewModel editUserViewModel)
        {
            if (ModelState.IsValid)
            {
                Guid imageId = editUserViewModel.ImageId;

                if (editUserViewModel.ImageFile != null)
                    imageId = await _azureBlobHelper.UploadAzureBlobAsync(editUserViewModel.ImageFile, "users");

                User user = await _userHelper.GetUserAsync(User.Identity.Name);

                user.FirstName = editUserViewModel.FirstName;
                user.LastName = editUserViewModel.LastName;
                user.Address = editUserViewModel.Address;
                user.PhoneNumber = editUserViewModel.PhoneNumber;
                user.ImageId = imageId;
                user.City = await _context.Cities.FindAsync(editUserViewModel.CityId);
                user.Document = editUserViewModel.Document;

                await _userHelper.UpdateUserAsync(user);
                return RedirectToAction("Index", "Home");
            }

            editUserViewModel.Countries = await _ddlHelper.GetDDLCountriesAsync();
            editUserViewModel.States = await _ddlHelper.GetDDLStatesAsync(editUserViewModel.CountryId);
            editUserViewModel.Cities = await _ddlHelper.GetDDLCitiesAsync(editUserViewModel.StateId);

            return View(editUserViewModel);
        }


        [HttpGet]
        public JsonResult GetStates(Guid countryId)
        {
            Country country = _context.Countries
                .Include(c => c.States)
                .FirstOrDefault(c => c.Id == countryId);

            if (country == null) return null;

            var jsonn = country.States.OrderBy(d => d.Name);

            return Json(jsonn);
        }

        [HttpGet]
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
