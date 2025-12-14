using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Application.DTOs.Common
{
    public class FileDownloadInfo
    {
        public string PhysicalPath { get; set; } = default!;
        public string ContentType { get; set; } = "application/octet-stream";
        public string DownloadName { get; set; } = default!;
    }
}
