using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BMS.ProgramConsoleUI
{
    public static class Validators
    {

        public static bool IsValidAzerbaijaniPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;


            string cleaned = phone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");



            var patterns = new[]
            {
                @"^\+994(50|51|55|70|77|99)\d{7}$",
                @"^0(50|51|55|70|77|99)\d{7}$"
            };

            return patterns.Any(pattern => Regex.IsMatch(cleaned, pattern));
        }


        public static string FormatPhone(string phone)
        {
            string cleaned = phone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

            if (cleaned.StartsWith("+994"))
            {

                return $"+994 {cleaned.Substring(4, 2)} {cleaned.Substring(6, 3)} {cleaned.Substring(9, 2)} {cleaned.Substring(11, 2)}";
            }
            else if (cleaned.StartsWith("0"))
            {

                return $"{cleaned.Substring(0, 3)} {cleaned.Substring(3, 3)} {cleaned.Substring(6, 2)} {cleaned.Substring(8, 2)}";
            }

            return phone;
        }


        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {

                var regex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }


        public static bool IsValidISBN(string isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn))
                return false;

            string cleaned = isbn.Replace("-", "").Replace(" ", "");
            return cleaned.Length == 13 && cleaned.All(char.IsDigit);
        }


        public static string GenerateISBN()
        {
            Random rand = Random.Shared;
            return $"978-{rand.Next(0, 10)}-{rand.Next(10, 100)}-{rand.Next(100000, 1000000)}-{rand.Next(0, 10)}";
        }


        public static string FormatISBN(string isbn)
        {
            string cleaned = isbn.Replace("-", "").Replace(" ", "");

            if (cleaned.Length == 13)
            {

                return $"{cleaned.Substring(0, 3)}-{cleaned[3]}-{cleaned.Substring(4, 2)}-{cleaned.Substring(6, 6)}-{cleaned[12]}";
            }

            return isbn;
        }


        public static bool ContainsIgnoreCase(string source, string search)
        {
            if (string.IsNullOrEmpty(search))
                return true;

            if (string.IsNullOrEmpty(source))
                return false;

            return source.Contains(search, StringComparison.OrdinalIgnoreCase);
        }


    }
}