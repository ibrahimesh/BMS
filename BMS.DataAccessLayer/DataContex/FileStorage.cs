using BMS.DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BMS.DataAccessLayer.DataContex
{
    public static class FileStorage
    {
        private static readonly string BaseFolder =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BMS Info");

        private static string GetPath(string fileName)
        {
            if (!Directory.Exists(BaseFolder))
                Directory.CreateDirectory(BaseFolder);

            return Path.Combine(BaseFolder, fileName);
        }

       

        public static void SaveBooks(string fileName, List<Book> books)
        {
            string path = GetPath(fileName);  

            using StreamWriter sw = new(path, false, Encoding.UTF8);

            foreach (var b in books)
            {
                string line =
                    b.Id.ToString("D5") + "|" +
                    b.Title.PadRight(30).Substring(0, 30) + "|" +
                    b.Author.PadRight(25).Substring(0, 25) + "|" +
                    b.ISBN.PadRight(13).Substring(0, 13) + "|" +
                    b.PublishedDate.Year.ToString("D4") + "|" +
                    b.CategoryId.ToString("D5") + "|" +
                    (b.IsAvailable ? "1" : "0");

                sw.WriteLine(line);
            }
        }

        public static List<Book> LoadBooks(string fileName)
        {
            string path = GetPath(fileName); 

            if (!File.Exists(path))
                return new List<Book>();

            List<Book> books = new();

            using StreamReader sr = new(path, Encoding.UTF8);

            string? line;
            while ((line = sr.ReadLine()) != null)
            {
                try
                {
                    string[] parts = line.Split('|');

                    if (parts.Length != 7)
                        continue;

                    Book book = new()
                    {
                        Id = int.Parse(parts[0].Trim()),
                        Title = parts[1].Trim(),
                        Author = parts[2].Trim(),
                        ISBN = parts[3].Trim(),
                        PublishedDate = new DateTime(int.Parse(parts[4].Trim()), 1, 1),
                        CategoryId = int.Parse(parts[5].Trim()),
                        IsAvailable = parts[6].Trim() == "1"
                    };

                    books.Add(book);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }

            return books;
        }

       

        public static void SaveCategories(string fileName, List<Category> categories)
        {
            string path = GetPath(fileName);  

            using StreamWriter sw = new(path, false, Encoding.UTF8);

            foreach (var c in categories)
            {
                string line =
                    c.Id.ToString("D5") + "|" +
                    c.Name.PadRight(30).Substring(0, 30) + "|" +
                    c.Description.PadRight(50).Substring(0, 50);

                sw.WriteLine(line);
            }
        }

        public static List<Category> LoadCategories(string fileName)
        {
            List<Category> categories = new();
            string path = GetPath(fileName);

            if (!File.Exists(path))
                return categories;

            foreach (var line in File.ReadAllLines(path, Encoding.UTF8))
            {
                try
                {
                    string[] parts = line.Split('|');

                    if (parts.Length != 3)
                        continue;

                    categories.Add(new Category
                    {
                        Id = int.Parse(parts[0].Trim()),
                        Name = parts[1].Trim(),
                        Description = parts[2].Trim()
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }

            return categories;
        }



        public static void SaveMembers(string fileName, List<Member> members)
        {
            string path = GetPath(fileName);

            using StreamWriter sw = new(path, false, Encoding.UTF8);

            foreach (var m in members)
            {
                string borrowedBookStr = m.BorrowedBookId.HasValue
                    ? m.BorrowedBookId.Value.ToString("D5")
                    : "".PadRight(5);

                string line =
                    m.Id.ToString("D5") + "|" +
                    m.FullName.PadRight(30).Substring(0, 30) + "|" +
                    m.Email.PadRight(30).Substring(0, 30) + "|" +
                    m.PhoneNumber.PadRight(15).Substring(0, 15) + "|" +
                    (m.IsActive ? "Active" : "Not Active").PadRight(11) + "|" +
                    m.MembershipDate.ToString("dd.MM.yyyy") + "|" +  
                    borrowedBookStr;

                sw.WriteLine(line);
            }
        }


        public static List<Member> LoadMembers(string fileName)
        {
            List<Member> members = new();
            string path = GetPath(fileName);

            if (!File.Exists(path))
                return members;

            foreach (var line in File.ReadAllLines(path, Encoding.UTF8))
            {
                try
                {
                    string[] parts = line.Split('|');
                    if (parts.Length != 7)
                        continue;

                    int? borrowedId = null;
                    if (int.TryParse(parts[6].Trim(), out var parsed))
                        borrowedId = parsed;

                    DateTime membershipDate;

                    
                    if (DateTime.TryParseExact(
                        parts[5].Trim(),
                        "dd.MM.yyyy",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None,
                        out membershipDate))
                    {
                    }
                   
                    else if (int.TryParse(parts[5].Trim(), out int year))
                    {
                        membershipDate = new DateTime(year, 1, 1);
                    }
                    else
                    {
                        membershipDate = DateTime.Now;
                    }

                    members.Add(new Member
                    {
                        Id = int.Parse(parts[0].Trim()),
                        FullName = parts[1].Trim(),
                        Email = parts[2].Trim(),
                        PhoneNumber = parts[3].Trim(),
                        IsActive = parts[4].Trim() == "Active",
                        MembershipDate = membershipDate,
                        BorrowedBookId = borrowedId
                    });
                }
                catch
                {
                   
                }
            }

            return members;
        }


    }
}