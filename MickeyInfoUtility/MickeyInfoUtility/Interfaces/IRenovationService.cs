using MickeyInfoUtility.Models;
using MickeyInfoUtility.Models.Shared;

namespace MickeyInfoUtility.Interfaces
{
    public interface IRenovationService
    {
        Task<List<RenovationItem>> GetRenovationItems(string accessKey);
        Task<List<KeyListItem>> GetAvailableKeys();
    }

}
