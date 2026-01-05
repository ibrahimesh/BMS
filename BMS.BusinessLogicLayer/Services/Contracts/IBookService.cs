using BMS.BusinessLogicLayer.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMS.BusinessLogicLayer.Services.Contracts
{
    public interface IBookService
    {
        List<BookDto> GetAllBooks();
        BookDto GetBookById(int id);
        void CreateBook(BookCreateDto bookCreateDto);
        void UpdateBook(BookUpdateDto bookUpdateDto);
        void DeleteBook(int id);
        List<BookDto> SearchBooks(BookSearchDto bookSearchDto);
        List<BookDto> GetAvailableBooks();
    }
}
