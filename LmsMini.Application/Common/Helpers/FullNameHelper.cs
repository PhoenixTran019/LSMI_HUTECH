using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Application.Common.Helpers
{
    public static class FullNameHelper
    {
        public static string GetFullName(string firstName, string lastName)
        {
            return $"{firstName} {lastName}".Trim();
        }
    }
}
