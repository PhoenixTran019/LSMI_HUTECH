// LmsMini.WebApp/ApiClients/SubmissionApiClient.cs

using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection; // Cần cho IHttpClientFactory

namespace LmsMini.WebApp.ApiClients
{
    public class SubmissionApiClient : ISubmissionApiClient
    {
        private readonly HttpClient _httpClient;

        public SubmissionApiClient(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("LmsApi");
        }

        public async Task<bool> UploadSubmissionAsync(
            string assignmentId,
            string studentId,
            string? contentPayload,
            IFormFile? file)
        {
            using var formData = new MultipartFormDataContent();

            // 1. Thêm các trường dữ liệu văn bản
            formData.Add(new StringContent(studentId), "StudentId");
            formData.Add(new StringContent(assignmentId), "AssignId");

            // Thêm nội dung nếu có (sử dụng contentPayload)
            if (!string.IsNullOrWhiteSpace(contentPayload))
            {
                formData.Add(new StringContent(contentPayload), "Content");
            }

            // 2. Thêm file (nếu có)
            if (file != null && file.Length > 0)
            {
                var streamContent = new StreamContent(file.OpenReadStream());
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                formData.Add(streamContent, "File", file.FileName);
            }

            // Endpoint giả định: POST api/Submission/submit
            var response = await _httpClient.PostAsync("api/Submission/submit", formData);

            return response.IsSuccessStatusCode;
        }
    }
}