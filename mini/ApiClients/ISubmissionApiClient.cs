// LmsMini.WebApp/ApiClients/ISubmissionApiClient.cs

using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace LmsMini.WebApp.ApiClients
{
    public interface ISubmissionApiClient
    {
        Task<bool> UploadSubmissionAsync(string assignmentId, string studentId, string? content, IFormFile? file);
    }
}