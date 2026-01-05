using BMS.BusinessLogicLayer.Dtos;
using BMS.BusinessLogicLayer.Services.Contracts;
using BMS.DataAccessLayer.DataContex;
using BMS.DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMS.BusinessLogicLayer.Services
{
    public class BookManager : IBookService
    {
        public void CreateBook(BookCreateDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new Exception("Kitabın adı boş ola bilməz");

            if (string.IsNullOrWhiteSpace(dto.Author))
                throw new Exception("Müəllif adı boş ola bilməz");

            if (string.IsNullOrWhiteSpace(dto.ISBN))
                throw new Exception("ISBN boş ola bilməz");

            if (BMSDataBase.Books.Any(b => b.ISBN == dto.ISBN))
                throw new Exception("Bu ISBN artıq mövcuddur");

           
            int newId = BMSDataBase.Books.Count == 0
                ? 1
                : BMSDataBase.Books.Max(b => b.Id) + 1;

            Book book = new Book
            {
                Id = newId,  
                Title = dto.Title.Trim(),
                Author = dto.Author.Trim(),
                ISBN = dto.ISBN.Trim(),
                PublishedDate = new DateTime(dto.PublishedDate, 1, 1),
                CategoryId = dto.CategoryId,
                IsAvailable = true
            };

            BMSDataBase.Books.Add(book);
        }


        public void DeleteBook(int id)
        {
            var book = BMSDataBase.Books.FirstOrDefault(b => b.Id == id);
            if (book != null)
            {
                BMSDataBase.Books.Remove(book);
            }
            else
            {
                throw new Exception("Book not found");
            }
        }

        public List<BookDto> GetAllBooks()
        {
            return BMSDataBase.Books.Select(b => new BookDto
            {
                Id = b.Id,
                Title = b.Title,
                Author = b.Author,
                PublishedDate = b.PublishedDate.Year,
                ISBN = b.ISBN,
                CategoryId = b.CategoryId,
                IsAvailable = b.IsAvailable
            }).ToList();
        }

        public List<BookDto> GetAvailableBooks()
        {
            return BMSDataBase.Books
                .Where(b => b.IsAvailable)
                .Select(b => new BookDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Author = b.Author,
                    PublishedDate = b.PublishedDate.Year,
                    ISBN = b.ISBN,
                    CategoryId = b.CategoryId,
                    IsAvailable = b.IsAvailable
                }).ToList();
        }

     

        public BookDto GetBookById(int id)
        {
            var book = BMSDataBase.Books.FirstOrDefault(b => b.Id == id);
            if (book != null)
            {
                return new BookDto
                {
                    Id = book.Id,
                    Title = book.Title,
                    Author = book.Author,
                    PublishedDate = book.PublishedDate.Year,
                    ISBN = book.ISBN,
                    CategoryId = book.CategoryId,
                    IsAvailable = book.IsAvailable
                };
            }
            else
            {
                throw new Exception("Book not found");

            }
                

        }

        public List<BookDto> SearchBooks(BookSearchDto bookSearchDto)
        {
            var query = BMSDataBase.Books.AsQueryable();
            if (!string.IsNullOrEmpty(bookSearchDto.Title))
            {
                query = query.Where(b => b.Title != null && b.Title.Contains(bookSearchDto.Title, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrEmpty(bookSearchDto.Author))
            {
                query = query.Where(b => b.Author != null && b.Author.Contains(bookSearchDto.Author, StringComparison.OrdinalIgnoreCase));
            }
            if (bookSearchDto.CategoryId != 0)
            {
                query = query.Where(b => b.CategoryId == bookSearchDto.CategoryId);
            }
            return query.Select(b => new BookDto
            {
                Id = b.Id,
                Title = b.Title,
                Author = b.Author,
                PublishedDate = b.PublishedDate.Year,
                ISBN = b.ISBN,
                CategoryId = b.CategoryId,
                IsAvailable = b.IsAvailable
            }).ToList();

        }

        public void UpdateBook(BookUpdateDto dto)
        {
            Book? book = BMSDataBase.Books.FirstOrDefault(b => b.Id == dto.Id);  

            if (book == null)
                throw new Exception("Kitab tapılmadı");

            if (!string.IsNullOrWhiteSpace(dto.Title))
                book.Title = dto.Title.Trim();

            if (!string.IsNullOrWhiteSpace(dto.Author))
                book.Author = dto.Author.Trim();

            if (!string.IsNullOrWhiteSpace(dto.ISBN))
            {
                if (BMSDataBase.Books.Any(b => b.ISBN == dto.ISBN && b.Id != dto.Id))
                    throw new Exception("Bu ISBN başqa kitabda istifadə olunur");

                book.ISBN = dto.ISBN.Trim();
            }

            book.IsAvailable = dto.IsAvailable;
        }

        public static string GenerateIsbn()
        {
            return Random.Shared.Next(1000000000, 1999999999).ToString() +
                   Random.Shared.Next(1000, 9999).ToString(); 
        }












    }
}
