using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMS.BusinessLogicLayer.Dtos
{
    public class BookDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Author { get; set; }
        public int PublishedDate { get; set; }
        public string? ISBN { get; set; }
        public int CategoryId { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class BookCreateDto
    {
        public required string Title { get; set; }
        public required string Author { get; set; }

        public required int PublishedDate { get; set; }


        public required string ISBN { get; set; }
        public int CategoryId { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class BookUpdateDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;  
        public string Author { get; set; } = string.Empty;  
        public int PublishedDate { get; set; }

        public string ISBN { get; set; } = string.Empty;  
        public int CategoryId { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class BookDeleteDto
    {
        public int Id { get; set; }
    }

    public class BookSearchDto
    {
        public string? Title { get; set; } = "";
        public string? Author { get; set; } = "";
        public int CategoryId { get; set; }
    }
}