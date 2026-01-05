using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMS.BusinessLogicLayer.Dtos
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }

        public List<BookDto>? Books { get; set; }
    }

    public class CategoryCreateDto
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
    }

    public class CategoryUpdateDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; 
        public string Description { get; set; } = string.Empty;  
    }

    public class CategoryDeleteDto
    {
        public int Id { get; set; }
    }

    public class CategorySearchDto
    {
        public string? Name { get; set; } = "";
    }
}