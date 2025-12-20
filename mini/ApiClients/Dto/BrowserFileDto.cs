using System.IO;

namespace LmsMini.WebApp.ApiClients.Dto
{
    public class BrowserFileDto
    {
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public Stream Content { get; set; } = Stream.Null;
    }
}