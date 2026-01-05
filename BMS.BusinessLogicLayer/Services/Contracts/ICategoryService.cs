using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMS.BusinessLogicLayer.Dtos;

namespace BMS.BusinessLogicLayer.Services.Contracts
{
    public interface ICategoryService

    {
        List<CategoryDto> GetAllCategories();
        CategoryDto GetCategoryById(int id);
        void CreateCategory(CategoryCreateDto categoryCreateDto);
        void UpdateCategory(CategoryUpdateDto categoryUpdateDto);
        void DeleteCategory(int id);
        List<CategoryDto> SearchCategories(CategorySearchDto categorySearchDto);
        CategoryDto GetCategoryWithBooks(int categoryId);
    }
}
