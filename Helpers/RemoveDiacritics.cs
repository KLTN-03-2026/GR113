using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;


namespace demomvc.Helpers
{
    public class RemoveDiacritics
    {
        public static string RemoveDiacritic(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "";

            string normalized = text.Normalize(NormalizationForm.FormD);

            StringBuilder sb = new StringBuilder();

            foreach (char c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c)
                    != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            return sb.ToString()
                     .Normalize(NormalizationForm.FormC)
                     .Replace("đ", "d")
                     .Replace("Đ", "D")
                     .Replace(" ", "")
                     .Trim()
                     .ToLower();
        }
    }
}