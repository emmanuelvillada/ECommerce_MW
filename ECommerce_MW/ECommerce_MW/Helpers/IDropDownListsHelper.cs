using Microsoft.AspNetCore.Mvc.Rendering;

namespace ECommerce_MW.Helpers
{
    public interface IDropDownListsHelper
    {
        Task<IEnumerable<SelectListItem>> GetDDLCategoriesAsync();

        Task<IEnumerable<SelectListItem>> GetDDLCountriesAsync();

        Task<IEnumerable<SelectListItem>> GetDDLStatesAsync(Guid countryId);

        Task<IEnumerable<SelectListItem>> GetDDLCitiesAsync(Guid stateId);

    }
}
