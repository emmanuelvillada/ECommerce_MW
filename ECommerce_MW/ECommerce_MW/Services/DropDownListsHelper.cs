using ECommerce_MW.DAL;
using ECommerce_MW.Helpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ECommerce_MW.Services
{
    public class DropDownListsHelper : IDropDownListsHelper
    {
        private readonly DatabaseContext _context;

        public DropDownListsHelper(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SelectListItem>> GetDDLCategoriesAsync()
        {
            List<SelectListItem> listCategories = await _context.Categories
                .Select(c => new SelectListItem
                {
                    Text = c.Name, //Col
                    Value = c.Id.ToString(), //Guid                    
                })
                .OrderBy(c => c.Text)
                .ToListAsync();

            listCategories.Insert(0, new SelectListItem
            {
                Text = "Selecione una categoría...",
                Value = Guid.Empty.ToString(), //Cambio el 0 por Guid.Empty ya que debo manejar el mismo tipo de dato en todo el DDL
                Selected = true //Le coloco esta propiedad para que me salga seleccionada por defecto desde la UI
            });

            return listCategories;
        }

        public async Task<IEnumerable<SelectListItem>> GetDDLCountriesAsync()
        {
            List<SelectListItem> listCountries = await _context.Countries
                .Select(c => new SelectListItem
                {
                    Text = c.Name, //Col
                    Value = c.Id.ToString(), //Guid                    
                })
                .OrderBy(c => c.Text)
                .ToListAsync();

            listCountries.Insert(0, new SelectListItem
            {
                Text = "Selecione un país...",
                Value = Guid.Empty.ToString(),
                Selected = true
            });

            return listCountries;
        }

        public async Task<IEnumerable<SelectListItem>> GetDDLStatesAsync(Guid countryId)
        {
            List<SelectListItem> listStatesByCountryId = await _context.States
                .Where(s => s.Country.Id == countryId)
                .Select(s => new SelectListItem
                {
                    Text = s.Name,
                    Value = s.Id.ToString(),
                })
                .OrderBy(s => s.Text)
                .ToListAsync();

            listStatesByCountryId.Insert(0, new SelectListItem
            {
                Text = "Selecione un estado...",
                Value = Guid.Empty.ToString(),
                Selected = true
            });

            return listStatesByCountryId;
        }

        public async Task<IEnumerable<SelectListItem>> GetDDLCitiesAsync(Guid stateId)
        {
            List<SelectListItem> listCitiesByStateId = await _context.Cities
                .Where(c => c.State.Id == stateId)
                .Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString(),
                })
                .OrderBy(c => c.Text)
                .ToListAsync();

            listCitiesByStateId.Insert(0, new SelectListItem
            {
                Text = "Selecione una ciudad...",
                Value = Guid.Empty.ToString(),
                Selected = true //Le coloco esta propiedad para que me salga seleccionada por defecto desde la UI
            });

            return listCitiesByStateId;
        }
    }
}
