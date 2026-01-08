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
            ConsoleHelper.MenuItem("6", "Müəllif üzrə axtarış");
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
                case "6": SearchAuthorUI(); break;
                case "0": break;
                default: ConsoleHelper.Error("Yanlış seçim"); ConsoleHelper.Pause(); break;
            }
        }

        static void CreateBookUI()
        {
            while (true)
            {
                try
                {
                    Console.Clear();
                    ConsoleHelper.Header("Yeni Kitab");

                    string title = InputText("Adı: ");
                    string author = InputString("Müəllif: ");

                    int year;
                    while (true)
                    {
                        year = InputInt("Nəşr ili: ");
                        if (year >= 0 && year <= DateTime.Now.Year)
                            break;

                        ConsoleHelper.Error("Nəşr ili gələcəkdə və ya mənfi ola bilməz!");
                    }

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

                    ConsoleHelper.Success("Kitab əlavə edildi");
                    Console.WriteLine($"📖 ISBN: {isbn}");
                    Console.WriteLine($"📂 Kateqoriya: {categoryName}");
                    SaveBooksToTxt();

                    ConsoleHelper.Pause();
                    break;
                }
                catch (Exception ex)
                {
                    ConsoleHelper.Error($"Xəta: {ex.Message}");
                    ConsoleHelper.Pause();
                }
            }
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
                ShowBooks();
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
                ShowBooks();
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
                ShowCategories();
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
            ConsoleHelper.MenuItem("6", "Üzv + seçdiyi kitab");
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
                case "6": ShowMembersWithBooks(); break;
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
                Console.Write("\nKitab seçmək istəyirsiniz? (y/n): ");
                string? choose = Console.ReadLine();

                int? selectedBookId = null;

                if (choose?.ToLower() == "y")
                {
                    ShowAvailableBooksForMember();
                    selectedBookId = InputInt("Kitab ID: ");
                }


                memberManager.CreateMember(new MemberCreateDto
                {

                    FullName = name,
                    Email = email,
                    PhoneNumber = phone,
                    IsActive = true,
                    BorrowedBookId = selectedBookId,
                    MembershipDate = DateTime.Now


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
                Console.WriteLine($"\n{"ID",-5} {"Ad, Soyad",-30} {"Email",-35} {"Telefon",-20} {"Üzvlük tarixi",-16} {"Status",-10}");
                Console.WriteLine(new string('─', 120));

                foreach (var m in members)
                {
                    string status = m.IsActive ? "✓ Aktiv" : "✗ Passiv";

                    Console.WriteLine(
                        $"{m.Id,-5} " +
                        $"{(m.FullName ?? "N/A").Substring(0, Math.Min(29, m.FullName?.Length ?? 0)),-30} " +
                        $"{(m.Email ?? "N/A").Substring(0, Math.Min(34, m.Email?.Length ?? 0)),-35} " +
                        $"{(m.PhoneNumber ?? "N/A"),-20} " +
                        $"{m.MembershipDate.ToString("yyyy-MM-dd"),-16} " +
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
                ShowMembers();
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
                Console.Write("\nYeni kitab seçilsin? (y/n): ");
                string? chooseBook = Console.ReadLine();

                int? newBookId = null;

                if (chooseBook?.ToLower() == "y")
                {
                    ShowAvailableBooksForMember();
                    newBookId = InputInt("Yeni kitab ID: ");
                }

                bool isActive = Console.ReadLine() == "1";

                memberManager.UpdateMember(new MemberUpdateDto
                {
                    Id = id,
                    FullName = string.IsNullOrWhiteSpace(name) ? (member.FullName ?? string.Empty) : name,
                    Email = string.IsNullOrWhiteSpace(email) ? (member.Email ?? string.Empty) : email,
                    PhoneNumber = string.IsNullOrWhiteSpace(phone) ? (member.PhoneNumber ?? string.Empty) : phone,
                    IsActive = isActive,
                    MembershipDate = member.MembershipDate,
                    BorrowedBookId = newBookId
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
                ShowMembers();
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
        static void ShowMembersWithBooks()
        {
            ConsoleHelper.Header("Üzvlər və seçilən kitablar");

            var members = memberManager.GetAllMembersWithBooks();

            foreach (var m in members)
            {
                Console.WriteLine(
                    $"{m.Id}. {m.FullName} → {m.BookTitle}"
                );
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
           
            string baseFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BMS Info");
            string booksPath = Path.Combine(baseFolder, "Books.txt");
            string categoriesPath = Path.Combine(baseFolder, "Categories.txt");
            string membersPath = Path.Combine(baseFolder, "Members.txt");

            BMSDataBase.Categories = File.Exists(categoriesPath)
                ? FileStorage.LoadCategories("Categories.txt")
                : new List<Category>();

            BMSDataBase.Books = File.Exists(booksPath)
                ? FileStorage.LoadBooks("Books.txt")
                : new List<Book>();

            BMSDataBase.Members = File.Exists(membersPath)
                ? FileStorage.LoadMembers("Members.txt")
                : new List<Member>();

            bool isSeeded = false;

            if (BMSDataBase.Categories.Count == 0)
            {
                SeedCategories();
                isSeeded = true;
            }

            if (BMSDataBase.Books.Count == 0)
            {
                SeedBooks();
                isSeeded = true;
            }

            if (BMSDataBase.Members.Count == 0)
            {
                SeedMembers();
                isSeeded = true;
            }

            if (isSeeded)
            {
                SaveData();
                ConsoleHelper.Success("✔ Məlumatlar TXT fayllara yazıldı");
            }
            else
            {
               
                 
            }
        }


        static void SeedCategories()
        {
            var categories = new List<Category>
    {
        new Category { Id = 1, Name = "Bədii ədəbiyyat", Description = "Romanlar, povest və hekayələr" },
        new Category { Id = 2, Name = "Elmi-kütləvi", Description = "Elmi və texniki kitablar" },
        new Category { Id = 3, Name = "Psixologiya", Description = "Psixologiya və özünü inkişaf" },
        new Category { Id = 4, Name = "Biznes", Description = "Biznes və idarəetmə" },
        new Category { Id = 5, Name = "Tarixi", Description = "Tarixi kitablar və xatirələr" },
        new Category { Id = 6, Name = "Uşaq ədəbiyyatı", Description = "Uşaqlar üçün nağıllar və əhvalat" },
        new Category { Id = 7, Name = "Fəlsəfə", Description = "Fəlsəfi əsərlər" },
        new Category { Id = 8, Name = "Texnologiya", Description = "Proqramlaşdırma və IT" }
    };

            BMSDataBase.Categories = categories;
            ConsoleHelper.Success($"✓ {categories.Count} kateqoriya yükləndi");
        }

        static void SeedBooks()
        {
            var books = new List<Book>
    {

        new Book { Id = 1, Title = "Koroğlu", Author = "Xalq dastanı", ISBN = "978-9-95-233001-1", PublishedDate = new DateTime(2015, 1, 1), CategoryId = 1, IsAvailable = true },
        new Book { Id = 2, Title = "Arşın mal alan", Author = "Üzeyir Hacıbəyov", ISBN = "978-9-95-233002-8", PublishedDate = new DateTime(2012, 1, 1), CategoryId = 1, IsAvailable = true },
        new Book { Id = 3, Title = "Ali və Nino", Author = "Qurban Səid", ISBN = "978-9-95-233003-5", PublishedDate = new DateTime(2018, 1, 1), CategoryId = 1, IsAvailable = true },
        new Book { Id = 4, Title = "Qanon", Author = "Əbdürrəhim bəy Haqverdiyev", ISBN = "978-9-95-233004-2", PublishedDate = new DateTime(2010, 1, 1), CategoryId = 1, IsAvailable = false },
        new Book { Id = 5, Title = "Dədə Qorqud", Author = "Xalq dastanı", ISBN = "978-9-95-233005-9", PublishedDate = new DateTime(2016, 1, 1), CategoryId = 1, IsAvailable = true },


        new Book { Id = 6, Title = "Kainatın tarixi", Author = "Stephen Hawking", ISBN = "978-0-55-338016-3", PublishedDate = new DateTime(2020, 1, 1), CategoryId = 2, IsAvailable = true },
        new Book { Id = 7, Title = "Sapiens", Author = "Yuval Noah Harari", ISBN = "978-0-09-959008-8", PublishedDate = new DateTime(2019, 1, 1), CategoryId = 2, IsAvailable = true },
        new Book { Id = 8, Title = "Homo Deus", Author = "Yuval Noah Harari", ISBN = "978-1-78-470400-6", PublishedDate = new DateTime(2021, 1, 1), CategoryId = 2, IsAvailable = false },


        new Book { Id = 9, Title = "Düşünün və zəngin olun", Author = "Napoleon Hill", ISBN = "978-1-58-542433-7", PublishedDate = new DateTime(2017, 1, 1), CategoryId = 3, IsAvailable = true },
        new Book { Id = 10, Title = "Emosional intellekt", Author = "Daniel Goleman", ISBN = "978-0-55-338371-3", PublishedDate = new DateTime(2018, 1, 1), CategoryId = 3, IsAvailable = true },
        new Book { Id = 11, Title = "Psixologiya ensiklopediyası", Author = "Elmi kollektiv", ISBN = "978-9-95-233011-0", PublishedDate = new DateTime(2019, 1, 1), CategoryId = 3, IsAvailable = true },


        new Book { Id = 12, Title = "Zəngin ata, kasıb ata", Author = "Robert Kiyosaki", ISBN = "978-1-61-268011-4", PublishedDate = new DateTime(2020, 1, 1), CategoryId = 4, IsAvailable = true },
        new Book { Id = 13, Title = "0-dan 1-ə", Author = "Peter Thiel", ISBN = "978-0-80-413929-7", PublishedDate = new DateTime(2021, 1, 1), CategoryId = 4, IsAvailable = false },
        new Book { Id = 14, Title = "Startap", Author = "Eric Ries", ISBN = "978-0-30-788791-7", PublishedDate = new DateTime(2019, 1, 1), CategoryId = 4, IsAvailable = true },


        new Book { Id = 15, Title = "Azərbaycan tarixi", Author = "Ziya Bünyadov", ISBN = "978-9-95-233015-8", PublishedDate = new DateTime(2014, 1, 1), CategoryId = 5, IsAvailable = true },
        new Book { Id = 16, Title = "Qarабağ tarixi", Author = "Mirza Jamal Javanshir", ISBN = "978-9-95-233016-5", PublishedDate = new DateTime(2013, 1, 1), CategoryId = 5, IsAvailable = true },


        new Book { Id = 17, Title = "Kiçik Şahzadə", Author = "Antoine de Saint-Exupéry", ISBN = "978-0-15-602501-8", PublishedDate = new DateTime(2015, 1, 1), CategoryId = 6, IsAvailable = true },
        new Book { Id = 18, Title = "Cırtdan", Author = "Səməd Vurğun", ISBN = "978-9-95-233018-9", PublishedDate = new DateTime(2011, 1, 1), CategoryId = 6, IsAvailable = true },
        new Book { Id = 19, Title = "Harry Potter", Author = "J.K. Rowling", ISBN = "978-0-74-754615-3", PublishedDate = new DateTime(2020, 1, 1), CategoryId = 6, IsAvailable = false },


        new Book { Id = 20, Title = "Meditations", Author = "Marcus Aurelius", ISBN = "978-0-14-044933-1", PublishedDate = new DateTime(2016, 1, 1), CategoryId = 7, IsAvailable = true },
        new Book { Id = 21, Title = "İnsan niyə yaşayır", Author = "Lev Tolstoy", ISBN = "978-5-17-098652-3", PublishedDate = new DateTime(2017, 1, 1), CategoryId = 7, IsAvailable = true },


        new Book { Id = 22, Title = "Clean Code", Author = "Robert C. Martin", ISBN = "978-0-13-235088-4", PublishedDate = new DateTime(2022, 1, 1), CategoryId = 8, IsAvailable = true },
        new Book { Id = 23, Title = "C# 12 və .NET 8", Author = "Mark J. Price", ISBN = "978-1-80-323767-2", PublishedDate = new DateTime(2023, 1, 1), CategoryId = 8, IsAvailable = true },
        new Book { Id = 24, Title = "Design Patterns", Author = "Gang of Four", ISBN = "978-0-20-163361-0", PublishedDate = new DateTime(2021, 1, 1), CategoryId = 8, IsAvailable = true },
        new Book { Id = 25, Title = "Python proqramlaşdırma", Author = "Eric Matthes", ISBN = "978-1-59-327928-8", PublishedDate = new DateTime(2022, 1, 1), CategoryId = 8, IsAvailable = false }
    };

            BMSDataBase.Books = books;
            ConsoleHelper.Success($"✓ {books.Count} kitab yükləndi");
        }

        static void SeedMembers()
        {
            var members = new List<Member>
    {
        new Member
        {
            Id = 1,
            FullName = "Əli Məmmədov",
            Email = "ali.mammadov@mail.az",
            PhoneNumber = "+994 50 123 45 67",
            MembershipDate = new DateTime(2023, 1, 15),
            IsActive = false,
            BorrowedBookId = 4
        },
        new Member
        {
            Id = 2,
            FullName = "Aynur Həsənova",
            Email = "aynur.hasanli@gmail.com",
            PhoneNumber = "+994 51 234 56 78",
            MembershipDate = new DateTime(2023, 3, 20),
            IsActive = true,
            BorrowedBookId = null
        },
        new Member
        {
            Id = 3,
            FullName = "Rəşad Quliyev",
            Email = "rashad.guliyev@yahoo.com",
            PhoneNumber = "+994 55 345 67 89",
            MembershipDate = new DateTime(2023, 5, 10),
            IsActive = false,
            BorrowedBookId = 8
        },
        new Member
        {
            Id = 4,
            FullName = "Səbinə Əliyeva",
            Email = "sabina.aliyeva@inbox.ru",
            PhoneNumber = "+994 70 456 78 90",
            MembershipDate = new DateTime(2023, 7, 5),
            IsActive = true,
            BorrowedBookId = null
        },
        new Member
        {
            Id = 5,
            FullName = "Orxan Nəbiyev",
            Email = "orkhan.nabiyev@hotmail.com",
            PhoneNumber = "+994 77 567 89 01",
            MembershipDate = new DateTime(2023, 9, 12),
            IsActive = false,
            BorrowedBookId = 13
        },
        new Member
        {
            Id = 6,
            FullName = "Günel İbrahimova",
            Email = "gunel.ibrahimova@bk.ru",
            PhoneNumber = "+994 99 678 90 12",
            MembershipDate = new DateTime(2023, 11, 25),
            IsActive = true,
            BorrowedBookId = null
        },
        new Member
        {
            Id = 7,
            FullName = "Turan Məmmədov",
            Email = "turan.mammadov@edu.az",
            PhoneNumber = "+994 50 789 01 23",
            MembershipDate = new DateTime(2024, 1, 8),
            IsActive = false,
            BorrowedBookId = 19
        },
        new Member
        {
            Id = 8,
            FullName = "Leyla Mustafayeva",
            Email = "leyla.mustafayeva@mail.ru",
            PhoneNumber = "+994 51 890 12 34",
            MembershipDate = new DateTime(2024, 2, 14),
            IsActive = true,
            BorrowedBookId = null
        },
        new Member
        {
            Id = 9,
            FullName = "Elvin Həsənov",
            Email = "elvin.hasanov@code.az",
            PhoneNumber = "+994 55 901 23 45",
            MembershipDate = new DateTime(2024, 4, 22),
            IsActive = false,
            BorrowedBookId = 25
        },
        new Member
        {
            Id = 10,
            FullName = "Nigar Əhmədova",
            Email = "nigar.ahmadova@unec.edu.az",
            PhoneNumber = "+994 70 012 34 56",
            MembershipDate = new DateTime(2024, 6, 30),
            IsActive = true,
            BorrowedBookId = null
        }
            };

            BMSDataBase.Members = members;
            ConsoleHelper.Success($"✓ {members.Count} üzv yükləndi");
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


        static void SearchAuthorUI()
        {
            ConsoleHelper.Header("Müəllif üzrə axtarış");

            Console.Write("Müəllif (1 hərf də olar): ");
            string key = Console.ReadLine() ?? "";

            var info = bookManager.GetAuthorInfo(key);

            if (!info.Books.Any())
            {
                ConsoleHelper.Error("Nəticə tapılmadı!");
                ConsoleHelper.Pause();
                return;
            }

            Console.WriteLine($"\n👤 Müəllif: {key}");
            Console.WriteLine($"📚 Kitab sayı: {info.BookCount}");
            Console.WriteLine($"📂 Janr sayı: {info.CategoryCount}");
            Console.WriteLine(new string('─', 80));
            Console.WriteLine("\n📂 Kateqoriyalar:");
            foreach (var c in info.Categories)
            {
                Console.WriteLine($" - {c}");
            }


            foreach (var b in info.Books)
            {
                Console.WriteLine($"{b.Id}. {b.Title} ({b.PublishedDate})");
            }

            ConsoleHelper.Pause();
        }

        static void ShowAvailableBooksForMember()
        {
            Console.WriteLine("\n📚 Mövcud kitablar:");

            var books = bookManager.GetAvailableBooks();

            if (!books.Any())
            {
                ConsoleHelper.Error("Hazırda mövcud kitab yoxdur!");
                return;
            }

            foreach (var b in books)
            {
                Console.WriteLine($"{b.Id}. {b.Title} - {b.Author}");
            }
        }


    }

}

