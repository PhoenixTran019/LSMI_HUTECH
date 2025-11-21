using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LmsMini.Application.Common.Helpers
{

    public static class SlugHelper
    {

        public static string Sluggify(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "unknown";

            // Loại ký tự không hợp lệ cho file system
            foreach (var c in Path.GetInvalidFileNameChars())
                input = input.Replace(c, '-');

            // Loại ký tự đặc biệt
            input = Regex.Replace(input, @"[^a-zA-Z0-9\s-]", "");

            // Replace spaces bằng dấu '-'
            input = input.Trim().Replace(" ", "-");

            // Đưa về lowercase
            return input.ToLower();
        }

    }
}
