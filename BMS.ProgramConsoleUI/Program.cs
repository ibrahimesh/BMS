using BMS.BusinessLogicLayer.Dtos;
using BMS.BusinessLogicLayer.Services;
using BMS.DataAccessLayer.DataContex;
using BMS.DataAccessLayer.Models;
using BMS.ProgramConsoleUI;
using System;
using System.IO;
using System.Linq;
using System.Text;
using UI;

namespace BMS.ConsoleUI
{
    class Program
    {
        static BookManager bookManager = new();
        static CategoryManager categoryManager = new();
        static MemberManager memberManager = new();

        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            LoadData();

            while (true)
            {
                try
                {
                    Console.Clear();
                    ConsoleHelper.Header("📚 Kitabxana İdarəetmə Sistemi");

                    ConsoleHelper.MenuItem("1", "Kitablar");
                    ConsoleHelper.MenuItem("2", "Kateqoriyalar");
                    ConsoleHelper.MenuItem("3", "Üzvlər");
                    ConsoleHelper.MenuItem("0", "Çıxış");

                    Console.Write("\nSeçim: ");
                    string? choice = Console.ReadLine();

                    switch (choice)
                    {
                        case "1": BookMenu(); break;
                        case "2": CategoryMenu(); break;
                        case "3": MemberMenu(); break;
                        case "0":
                            SaveData();
                            ConsoleHelper.Success("Proqramdan çıxılır...");
                            return;
                        default:
                            ConsoleHelper.Error("Yanlış seçim! Yenidən cəhd edin.");
                            ConsoleHelper.Pause();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    ConsoleHelper.Error($"Xəta baş verdi: {ex.Message}");
                    ConsoleHelper.Pause();
                }
            }
        }

        #region Book Menu
        static void BookMenu()
        {
            ConsoleHelper.Header("📘 Kitablar");
            ConsoleHelper.MenuItem("1", "Kitab əlavə et");
            ConsoleHelper.MenuItem("2", "Bütün kitablar");
            ConsoleHelper.MenuItem("3", "Kitab axtar");
            ConsoleHelper.MenuItem("4", "Kitab güncəllə");
            ConsoleHelper.MenuItem("5", "Kitab sil");
            ConsoleHelper.MenuItem("0", "Geri");
            Console.Write("Seçim: ");
            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1": CreateBookUI(); break;
                case "2": ShowBooks(); break;
                case "3": SearchBooksUI(); break;
                case "4": UpdateBookUI(); break;
                case "5": DeleteBookUI(); break;
                case "0": break;
                default: ConsoleHelper.Error("Yanlış seçim"); ConsoleHelper.Pause(); break;
            }
        }

        static void CreateBookUI()
        {
            try
            {
                ConsoleHelper.Header("Yeni Kitab");

                string title = InputText("Adı: ");
                string author = InputString("Müəllif: ");
                int year = InputInt("Nəşr ili: ");
                if (year < 0 || year > DateTime.Now.Year)
                    throw new Exception("Nəşr ili düzgün deyil!");

                Console.WriteLine("\n📂 Mövcud kateqoriyalar:");
                var categories = categoryManager.GetAllCategories();

                if (categories.Any())
                {
                    foreach (var cat in categories)
                    {
                        Console.WriteLine($"  {cat.Id}. {cat.Name}");
                    }
                }
                else
                {
                    Console.WriteLine("  Heç bir kateqoriya yoxdur.");
                }

                Console.Write("\nKateqoriya adı (yeni və ya mövcud): ");
                string? categoryName = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(categoryName))
                    categoryName = "Ümumi";

                int categoryId = categoryManager.GetOrCreateCategory(categoryName);

               
                string isbn = Validators.GenerateISBN();

                bookManager.CreateBook(new BookCreateDto
                {
                    Title = title,
                    Author = author,
                    ISBN = isbn,  
                    PublishedDate = year,
                    CategoryId = categoryId,
                    IsAvailable = true
                });

                ConsoleHelper.Success($"Kitab əlavə edildi");
                Console.WriteLine($"📖 ISBN: {isbn}");
                Console.WriteLine($"📂 Kateqoriya: {categoryName}");
                SaveBooksToTxt();
            }
            catch (Exception ex)
            {
                ConsoleHelper.Error($"Xəta: {ex.Message}");
            }
            ConsoleHelper.Pause();
        }

        static void ShowBooks()
        {
            ConsoleHelper.Header("Kitab Siyahısı");
            var books = bookManager.GetAllBooks();

            if (!books.Any())
            {
                ConsoleHelper.Error("Kitablar siyahısı boşdur!");
            }
            else
            {
                Console.WriteLine($"\n{"ID",-5} {"Kitab Adı",-35} {"Müəllif",-25} {"ISBN",-20} {"Kateqoriya",-20} {"Status",-12}");
                Console.WriteLine(new string('─', 120));

                foreach (var b in books)
                {
                    var category = categoryManager.GetCategoryById(b.CategoryId);
                    string categoryName = category?.Name ?? "N/A";
                    string status = b.IsAvailable ? "✓ Mövcud" : "✗ Verilib";
                    string isbn = Validators.FormatISBN(b.ISBN ?? "");

                    Console.WriteLine(
                        $"{b.Id,-5} " +
                        $"{(b.Title ?? "N/A").Substring(0, Math.Min(34, b.Title?.Length ?? 0)),-35} " +
                        $"{(b.Author ?? "N/A").Substring(0, Math.Min(24, b.Author?.Length ?? 0)),-25} " +
                        $"{isbn,-20} " +
                        $"{categoryName.Substring(0, Math.Min(19, categoryName.Length)),-20} " +
                        $"{status,-12}"
                    );
                }

                Console.WriteLine($"\n📊 Cəmi: {books.Count} kitab");
            }

            ConsoleHelper.Pause();
        }

        static void SearchBooksUI()
        {
            ConsoleHelper.Header("Kitab Axtarışı");

            Console.WriteLine("🔍 Axtarış parametrləri (boş buraxsanız, hamısı göstəriləcək):");
            string title = InputText("Kitab adı: ", true);
            string author = InputString("Müəllif: ", true);

            var allBooks = bookManager.GetAllBooks();

            
            var results = allBooks.Where(b =>
                (string.IsNullOrEmpty(title) || Validators.ContainsIgnoreCase(b.Title ?? "", title)) &&
                (string.IsNullOrEmpty(author) || Validators.ContainsIgnoreCase(b.Author ?? "", author))
            ).ToList();

            if (!results.Any())
            {
                ConsoleHelper.Error("Nəticə tapılmadı!");
            }
            else
            {
                Console.WriteLine($"\n✓ {results.Count} nəticə tapıldı:\n");
                Console.WriteLine($"{"ID",-5} {"Kitab Adı",-35} {"Müəllif",-25} {"ISBN",-20}");
                Console.WriteLine(new string('─', 90));

                foreach (var b in results)
                {
                    Console.WriteLine(
                        $"{b.Id,-5} " +
                        $"{(b.Title ?? "N/A").Substring(0, Math.Min(34, b.Title?.Length ?? 0)),-35} " +
                        $"{(b.Author ?? "N/A").Substring(0, Math.Min(24, b.Author?.Length ?? 0)),-25} " +
                        $"{Validators.FormatISBN(b.ISBN ?? ""),-20}"
                    );
                }
            }

            ConsoleHelper.Pause();
        }

        static void UpdateBookUI()
        {
            try
            {
                ConsoleHelper.Header("Kitab Güncəllə");
                int id = InputInt("Güncəllənəcək kitabın ID-si: ");
                var book = bookManager.GetBookById(id);

                if (book == null)
                {
                    ConsoleHelper.Error("Kitab tapılmadı!");
                    ConsoleHelper.Pause();
                    return;
                }

                string title = InputText($"Adı ({book.Title}): ", true);
                string author = InputString($"Müəllif ({book.Author}): ", true);

               
                Console.Write($"Nəşr ili ({book.PublishedDate}): ");
                string? yearInput = Console.ReadLine();
                int year = string.IsNullOrWhiteSpace(yearInput)
                    ? book.PublishedDate
                    : int.Parse(yearInput);

                Console.Write("Kitab mövcuddur? (1 - Bəli, 0 - Xeyr): ");
                string? availableInput = Console.ReadLine();
                bool isAvailable = availableInput == "1";

                bookManager.UpdateBook(new BookUpdateDto
                {
                    Id = id,
                    Title = string.IsNullOrWhiteSpace(title) ? (book.Title ?? string.Empty) : title,
                    Author = string.IsNullOrWhiteSpace(author) ? (book.Author ?? string.Empty) : author,
                    PublishedDate = year,  
                    ISBN = book.ISBN ?? string.Empty,
                    CategoryId = book.CategoryId,
                    IsAvailable = isAvailable
                });

                ConsoleHelper.Success("Kitab güncəlləndi");
                SaveBooksToTxt();
            }
            catch (Exception ex)
            {
                ConsoleHelper.Error($"Xəta: {ex.Message}");
            }
            ConsoleHelper.Pause();
        }

        static void DeleteBookUI()
        {
            try
            {
                ConsoleHelper.Header("Kitab Sil");
                int id = InputInt("Silinəcək kitabın ID-si: ");
                var book = bookManager.GetBookById(id);

                if (book == null)  
                {
                    ConsoleHelper.Error("Kitab tapılmadı!");
                    ConsoleHelper.Pause();
                    return;
                }

                Console.WriteLine($"\n{book.Title} - {book.Author}");
                Console.Write("\nƏminsiniz? (y/n): ");

                if (Console.ReadLine()?.ToLower() == "y")
                {
                    bookManager.DeleteBook(id);
                    ConsoleHelper.Success("Kitab silindi");
                    SaveBooksToTxt();
                }
                else
                {
                    ConsoleHelper.Success("Əməliyyat ləğv edildi");
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.Error($"Xəta: {ex.Message}");
            }
            ConsoleHelper.Pause();
        }
        #endregion

        #region Category Menu
        static void CategoryMenu()
        {
            ConsoleHelper.Header("📂 Kateqoriyalar");
            ConsoleHelper.MenuItem("1", "Kateqoriya əlavə et");
            ConsoleHelper.MenuItem("2", "Bütün kateqoriyalar");
            ConsoleHelper.MenuItem("3", "Kateqoriya axtar");
            ConsoleHelper.MenuItem("4", "Kateqoriya güncəllə");
            ConsoleHelper.MenuItem("5", "Kateqoriya sil");
            ConsoleHelper.MenuItem("6", "Kateqoriya və kitabları göstər"); 
            ConsoleHelper.MenuItem("0", "Geri");
            Console.Write("Seçim: ");
            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1": CreateCategoryUI(); break;
                case "2": ShowCategories(); break;
                case "3": SearchCategoriesUI(); break;
                case "4": UpdateCategoryUI(); break;
                case "5": DeleteCategoryUI(); break;
                case "6": ShowCategoryWithBooks(); break;  
                case "0": break;
                default: ConsoleHelper.Error("Yanlış seçim"); ConsoleHelper.Pause(); break;
            }
        }

       
        static void ShowCategoryWithBooks()
        {
            try
            {
                ConsoleHelper.Header("Kateqoriya və Kitabları");

               
                var categories = categoryManager.GetAllCategories();

                if (!categories.Any())
                {
                    ConsoleHelper.Error("Kateqoriyalar siyahısı boşdur!");
                    ConsoleHelper.Pause();
                    return;
                }

                Console.WriteLine("\n📂 Mövcud kateqoriyalar:");
                foreach (var cat in categories)
                {
                    var booksCount = BMSDataBase.Books.Count(b => b.CategoryId == cat.Id);
                    Console.WriteLine($"  {cat.Id}. {cat.Name} ({booksCount} kitab)");
                }

                int categoryId = InputInt("\nKateqoriya ID-si: ");

                var categoryWithBooks = categoryManager.GetCategoryWithBooks(categoryId);

                Console.WriteLine($"\n📂 Kateqoriya: {categoryWithBooks.Name}");
                Console.WriteLine($"📝 Təsvir: {categoryWithBooks.Description}");
                Console.WriteLine($"\n📚 Kitablar ({categoryWithBooks.Books?.Count ?? 0}):");
                Console.WriteLine(new string('─', 80));

                if (categoryWithBooks.Books == null || !categoryWithBooks.Books.Any())
                {
                    ConsoleHelper.Error("Bu kateqoriyada kitab yoxdur.");
                }
                else
                {
                    foreach (var book in categoryWithBooks.Books)
                    {
                        string status = book.IsAvailable ? "✓ Mövcuddur" : "✗ Verilib";
                        Console.WriteLine(
                            $"{book.Id}. {(book.Title ?? "N/A").PadRight(35)} " +
                            $"{(book.Author ?? "N/A").PadRight(25)} " +
                            $"({book.PublishedDate}) [{status}]"
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.Error($"Xəta: {ex.Message}");
            }
            ConsoleHelper.Pause();
        }

        static void CreateCategoryUI()
        {
            try
            {
                ConsoleHelper.Header("Yeni Kateqoriya");
                
                string name = InputText("Adı: ");
                string desc = InputText("Təsviri: ");

                categoryManager.CreateCategory(new CategoryCreateDto
                {
                 
                    Name = name,
                    Description = desc
                });

                ConsoleHelper.Success("Kateqoriya əlavə edildi");
                SaveCategoriesToTxt();
            }
            catch (Exception ex)
            {
                ConsoleHelper.Error($"Xəta: {ex.Message}");
            }
            ConsoleHelper.Pause();
        }

        static void ShowCategories()
        {
            ConsoleHelper.Header("Kateqoriyalar");
            var categories = categoryManager.GetAllCategories();

            if (!categories.Any())
                ConsoleHelper.Error("Kateqoriyalar siyahısı boşdur!");
            else
            {
                foreach (var c in categories)
                    Console.WriteLine(
                        $"{c.Id}. {(c.Name ?? "N/A").PadRight(30)} {c.Description ?? "N/A"}"
                    );
            }

            ConsoleHelper.Pause();
        }
        

        static void SearchCategoriesUI()
        {
            ConsoleHelper.Header("Kateqoriya Axtarışı");
            string name = InputString("Kateqoriya adı: ", true);

            var results = categoryManager.SearchCategories(new CategorySearchDto { Name = name });
            if (!results.Any()) ConsoleHelper.Error("Nəticə tapılmadı!");
            else
            {
                foreach (var c in results)
                    Console.WriteLine($"{c.Id}. {(c.Name ?? "N/A").PadRight(30)}   {c.Description ?? "N/A"}");
            }

            ConsoleHelper.Pause();
        }

        static void UpdateCategoryUI()
        {
            try
            {
                ConsoleHelper.Header("Kateqoriya Güncəllə");
                int id = InputInt("Güncəllənəcək kateqoriya ID-si: ");
                var category = categoryManager.GetCategoryById(id);

                if (category == null)
                {
                    ConsoleHelper.Error("Kateqoriya tapılmadı!");
                    ConsoleHelper.Pause();
                    return;
                }

                string name = InputText($"Adı ({category.Name}): ", true);
                string desc = InputText($"Təsviri ({category.Description}): ", true);

                categoryManager.UpdateCategory(new CategoryUpdateDto
                {
                    Id = id,
                    Name = string.IsNullOrWhiteSpace(name) ? (category.Name ?? string.Empty) : name, 
                    Description = string.IsNullOrWhiteSpace(desc) ? (category.Description ?? string.Empty) : desc  
                });

                ConsoleHelper.Success("Kateqoriya güncəlləndi");
                SaveCategoriesToTxt();
            }
            catch (Exception ex)
            {
                ConsoleHelper.Error($"Xəta: {ex.Message}");
            }
            ConsoleHelper.Pause();
        }

        static void DeleteCategoryUI()
        {
            try
            {
                ConsoleHelper.Header("Kateqoriya Sil");
                int id = InputInt("Silinəcək kateqoriya ID-si: ");
                categoryManager.DeleteCategory(id);
                ConsoleHelper.Success("Kateqoriya silindi");
                SaveCategoriesToTxt();
            }
            catch (Exception ex)
            {
                ConsoleHelper.Error($"Xəta: {ex.Message}");
            }
            ConsoleHelper.Pause();
        }
        #endregion

        #region Member Menu
        static void MemberMenu()
        {
            ConsoleHelper.Header("👤 Üzvlər");
            ConsoleHelper.MenuItem("1", "Üzv əlavə et");
            ConsoleHelper.MenuItem("2", "Bütün üzvlər");
            ConsoleHelper.MenuItem("3", "Üzv axtar");
            ConsoleHelper.MenuItem("4", "Üzv güncəllə");
            ConsoleHelper.MenuItem("5", "Üzv sil");
            ConsoleHelper.MenuItem("0", "Geri");
            Console.Write("Seçim: ");
            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1": CreateMemberUI(); break;
                case "2": ShowMembers(); break;
                case "3": SearchMembersUI(); break;
                case "4": UpdateMemberUI(); break;
                case "5": DeleteMemberUI(); break;
                case "0": break;
                default: ConsoleHelper.Error("Yanlış seçim"); ConsoleHelper.Pause(); break;
            }
        }

        static void CreateMemberUI()
        {
            try
            {
                ConsoleHelper.Header("Yeni Üzv");
             
                string name = InputString("Ad, Soyad: ");
                string email = InputEmail("Email: ");
                string phone = InputPhone("Telefon: ");

                memberManager.CreateMember(new MemberCreateDto
                {
                  
                    FullName = name,
                    Email = email,
                    PhoneNumber = phone,
                    IsActive = true
                });

                ConsoleHelper.Success("Üzv əlavə edildi");
                SaveMembersToTxt();
            }
            catch (Exception ex)
            {
                ConsoleHelper.Error($"Xəta: {ex.Message}");
            }
            ConsoleHelper.Pause();
        }

        static void ShowMembers()
        {
            ConsoleHelper.Header("Üzvlər");
            var members = memberManager.GetAllMembers();

            if (!members.Any())
            {
                ConsoleHelper.Error("Üzvlər siyahısı boşdur!");
            }
            else
            {
                Console.WriteLine($"\n{"ID",-5} {"Ad, Soyad",-30} {"Email",-35} {"Telefon",-20} {"Status",-10}");
                Console.WriteLine(new string('─', 105));

                foreach (var m in members)
                {
                    string status = m.IsActive ? "✓ Aktiv" : "✗ Passiv";

                    Console.WriteLine(
                        $"{m.Id,-5} " +
                        $"{(m.FullName ?? "N/A").Substring(0, Math.Min(29, m.FullName?.Length ?? 0)),-30} " +
                        $"{(m.Email ?? "N/A").Substring(0, Math.Min(34, m.Email?.Length ?? 0)),-35} " +
                        $"{(m.PhoneNumber ?? "N/A"),-20} " +
                        $"{status,-10}"
                    );
                }

                Console.WriteLine($"\n📊 Cəmi: {members.Count} üzv");
            }

            ConsoleHelper.Pause();
        }

        static void SearchMembersUI()
        {
            ConsoleHelper.Header("Üzv Axtarışı");

            Console.WriteLine("🔍 Axtarış parametrləri (boş buraxsanız, hamısı göstəriləcək):");
            string name = InputString("Ad, Soyad: ", true);
            string email = InputString("Email: ", true);

            var allMembers = memberManager.GetAllMembers();

          
            var results = allMembers.Where(m =>
                (string.IsNullOrEmpty(name) || Validators.ContainsIgnoreCase(m.FullName ?? "", name)) &&
                (string.IsNullOrEmpty(email) || Validators.ContainsIgnoreCase(m.Email ?? "", email))
            ).ToList();

            if (!results.Any())
            {
                ConsoleHelper.Error("Nəticə tapılmadı!");
            }
            else
            {
                Console.WriteLine($"\n✓ {results.Count} nəticə tapıldı:\n");
                Console.WriteLine($"{"ID",-5} {"Ad, Soyad",-30} {"Email",-35} {"Telefon",-20}");
                Console.WriteLine(new string('─', 95));

                foreach (var m in results)
                {
                    Console.WriteLine(
                        $"{m.Id,-5} " +
                        $"{(m.FullName ?? "N/A"),-30} " +
                        $"{(m.Email ?? "N/A"),-35} " +
                        $"{(m.PhoneNumber ?? "N/A"),-20}"
                    );
                }
            }

            ConsoleHelper.Pause();
        }

        static void UpdateMemberUI()
        {
            try
            {
                ConsoleHelper.Header("Üzv Güncəllə");
                int id = InputInt("Güncəllənəcək üzv ID-si: ");
                var member = memberManager.GetMemberById(id);

                if (member == null)
                {
                    ConsoleHelper.Error("Üzv tapılmadı!");
                    ConsoleHelper.Pause();
                    return;
                }

                string name = InputString($"Ad, Soyad ({member.FullName}): ", true);
                string email = InputEmail($"Email ({member.Email}): ", true);
                string phone = InputPhone($"Telefon ({member.PhoneNumber}): ", true);
                Console.Write("Aktiv olsun? (1 - Bəli, 0 - Xeyr): ");
                bool isActive = Console.ReadLine() == "1";

                memberManager.UpdateMember(new MemberUpdateDto
                {
                    Id = id,
                    FullName = string.IsNullOrWhiteSpace(name) ? (member.FullName ?? string.Empty) : name,  
                    Email = string.IsNullOrWhiteSpace(email) ? (member.Email ?? string.Empty) : email,  
                    PhoneNumber = string.IsNullOrWhiteSpace(phone) ? (member.PhoneNumber ?? string.Empty) : phone,  
                    IsActive = isActive,
                    MembershipDate = member.MembershipDate
                });

                ConsoleHelper.Success("Üzv güncəlləndi");
                SaveMembersToTxt();
            }
            catch (Exception ex)
            {
                ConsoleHelper.Error($"Xəta: {ex.Message}");
            }
            ConsoleHelper.Pause();
        }

        static void DeleteMemberUI()
        {
            try
            {
                ConsoleHelper.Header("Üzv Sil");
                int id = InputInt("Silinəcək üzv ID-si: ");
                memberManager.DeleteMember(id);
                ConsoleHelper.Success("Üzv silindi");
                SaveMembersToTxt();
            }
            catch (Exception ex)
            {
                ConsoleHelper.Error($"Xəta: {ex.Message}");
            }
            ConsoleHelper.Pause();
        }
        #endregion

        #region Helpers
        static int InputInt(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), out int val)) return val;
                ConsoleHelper.Error("Yalnız ədəd daxil edin!");
            }
        }

        static string InputString(string prompt, bool allowEmpty = false)
        {
            while (true)
            {
                Console.Write(prompt);
                string? val = Console.ReadLine();

                if (allowEmpty && string.IsNullOrEmpty(val))
                    return "";

                if (string.IsNullOrWhiteSpace(val))
                {
                    ConsoleHelper.Error("Boş ola bilməz!");
                    continue;
                }

                
                if (val.All(c => char.IsLetter(c) || c == ' ' || c == '-' || c == '\'' || c == '.'))
                    return val.Trim();

                ConsoleHelper.Error("Yalnız hərflər, boşluq və defis istifadə edin!");
            }
        }

       
        static string InputText(string prompt, bool allowEmpty = false)
        {
            while (true)
            {
                Console.Write(prompt);
                string? val = Console.ReadLine();

                if (allowEmpty && string.IsNullOrEmpty(val))
                    return "";

                if (string.IsNullOrWhiteSpace(val))
                {
                    ConsoleHelper.Error("Boş ola bilməz!");
                    continue;
                }

               
                if (val.All(c => char.IsLetterOrDigit(c) || c == ' ' || c == '-' || c == '\'' || c == '.' || c == ',' || c == ':'))
                    return val.Trim();

                ConsoleHelper.Error("Yalnız hərflər, rəqəmlər və əsas durğu işarələri istifadə edin!");
            }
        }

        static string InputEmail(string prompt, bool allowEmpty = false)
        {
            while (true)
            {
                Console.Write(prompt);
                string? val = Console.ReadLine();

                if (allowEmpty && string.IsNullOrEmpty(val))
                    return "";

                if (string.IsNullOrWhiteSpace(val))
                {
                    ConsoleHelper.Error("Email boş ola bilməz!");
                    continue;
                }

                if (Validators.IsValidEmail(val))
                    return val.Trim().ToLower();

                ConsoleHelper.Error("Email düzgün deyil! Nümunə: user@example.com");
                Console.WriteLine("📧 Düzgün format: username@domain.com");
            }
        }

        static string InputPhone(string prompt, bool allowEmpty = false)
        {
            while (true)
            {
                Console.Write(prompt);
                string? val = Console.ReadLine();

                if (allowEmpty && string.IsNullOrEmpty(val))
                    return "";

                if (string.IsNullOrWhiteSpace(val))
                {
                    ConsoleHelper.Error("Telefon boş ola bilməz!");
                    continue;
                }

                if (Validators.IsValidAzerbaijaniPhone(val))
                {
                    return Validators.FormatPhone(val);
                }

                ConsoleHelper.Error("Telefon nömrəsi düzgün deyil!");
                Console.WriteLine("📱 Düzgün formatlar:");
                Console.WriteLine("   +994501234567  (mütləq +994 ilə başlamalı)");
                Console.WriteLine("   0501234567     (və ya 0 ilə başlamalı)");
                Console.WriteLine("🔢 Operatorlar: 50, 51, 55, 70, 77, 99");
            }
        }

        static void LoadData()
        {
            BMSDataBase.Books = FileStorage.LoadBooks("Books.txt");
            BMSDataBase.Categories = FileStorage.LoadCategories("Categories.txt");
            BMSDataBase.Members = FileStorage.LoadMembers("Members.txt");
        }

        static void SaveData()
        {
            SaveBooksToTxt();
            SaveCategoriesToTxt();
            SaveMembersToTxt();
        }

        static void SaveBooksToTxt() => FileStorage.SaveBooks("Books.txt", BMSDataBase.Books);
        static void SaveCategoriesToTxt() => FileStorage.SaveCategories("Categories.txt", BMSDataBase.Categories);
        static void SaveMembersToTxt() => FileStorage.SaveMembers("Members.txt", BMSDataBase.Members);
        #endregion
    }
}

