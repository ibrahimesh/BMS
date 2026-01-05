using BMS.DataAccessLayer.Repostories.Contracts;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMS.DataAccessLayer.Models;
using BMS.DataAccessLayer.DataContex;

namespace BMS.DataAccessLayer.Repostories
{
    public class BookRepostories : IBook
    {
        public void Add(Book entity)
        {
            BMSDataBase.Books.Add(entity);
        }

        public List<Book> GetAll()
        {
            return BMSDataBase.Books;
        }

        public Book? GetById(int id)  
        {
            return BMSDataBase.Books.FirstOrDefault(b => b.Id == id);
        }

        public void Update(Book entity)
        {
            var book = GetById(entity.Id);
            if (book != null)
            {
                book.Title = entity.Title;
                book.Author = entity.Author;
                book.CategoryId = entity.CategoryId;
                book.PublishedDate = entity.PublishedDate;
                return;
            }
            throw new Exception("Book not found");
        }

        public List<Book> Search(string keyword)
        {
            return BMSDataBase.Books
                .Where(b => b.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                            b.Author.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                            b.ISBN.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public void Delete(int id)
        {
            var book = GetById(id);
            if (book != null)
            {
                BMSDataBase.Books.Remove(book);
            }
            else
            {
                throw new Exception("Book not found");
            }
        }
    }
}