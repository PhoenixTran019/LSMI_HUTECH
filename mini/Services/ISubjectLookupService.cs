using LmsMini.WebApp.ApiClients.Dto;

namespace LmsMini.WebApp.Services
{
    public interface ISubjectLookupService
    {
        Task<List<LookupItemDto>> GetSubjectsAsync();
    }
}
