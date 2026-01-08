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
    public class CategoryManager : ICategoryService
    {
        public void CreateCategory(CategoryCreateDto categoryCreateDto)
        {
            if (categoryCreateDto.Name.Trim().Length < 3)
                throw new Exception("Kateqoriya adı çox qısadır (min. 3 simvol).");

            int newId = BMSDataBase.Categories.Count == 0
                ? 1
                : BMSDataBase.Categories.Max(c => c.Id) + 1;

            BMSDataBase.Categories.Add(new Category
            {
                Id = newId,
                Name = categoryCreateDto.Name,
                Description = categoryCreateDto.Description
            });
        }

        public void DeleteCategory(int id)
        {
            var category = BMSDataBase.Categories.FirstOrDefault(c => c.Id == id);
            if (category != null)
            {
               
                var booksInCategory = BMSDataBase.Books.Count(b => b.CategoryId == id);
                if (booksInCategory > 0)
                {
                    Console.Write($"Bu kateqoriyada {booksInCategory} kitab var. Hamısı silinsin? (y/n): ");
                    string? answer = Console.ReadLine();

                    if (answer?.ToLower() != "y")
                        return;

                    var booksToRemove = BMSDataBase.Books
                        .Where(b => b.CategoryId == id)
                        .ToList();

                    foreach (var book in booksToRemove)
                    {
                        BMSDataBase.Books.Remove(book);
                    }
                }


                BMSDataBase.Categories.Remove(category);
            }
            else
            {
                throw new Exception("Category not found");
            }
        }

        public List<CategoryDto> GetAllCategories()
        {
            return BMSDataBase.Categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description
            }).ToList();
        }

        public CategoryDto GetCategoryById(int id)
        {
            var category = BMSDataBase.Categories.FirstOrDefault(c => c.Id == id);
            if (category != null)
            {
                return new CategoryDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description
                };
            }
            else
            {
                throw new Exception("Category not found");
            }
        }

        
        public CategoryDto GetCategoryWithBooks(int categoryId)
        {
            var category = BMSDataBase.Categories.FirstOrDefault(c => c.Id == categoryId);

            if (category == null)
                throw new Exception("Kateqoriya tapılmadı!");

            var booksInCategory = BMSDataBase.Books
                .Where(b => b.CategoryId == categoryId)
                .Select(b => new BookDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Author = b.Author,
                    PublishedDate = b.PublishedDate.Year,
                    ISBN = b.ISBN,
                    CategoryId = b.CategoryId,
                    IsAvailable = b.IsAvailable
                })
                .ToList();

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                Books = booksInCategory
            };
        }

        public List<CategoryDto> SearchCategories(CategorySearchDto categorySearchDto)
        {
            return BMSDataBase.Categories
                .Where(c => c.Name != null && c.Name.Contains(categorySearchDto.Name ?? string.Empty, StringComparison.OrdinalIgnoreCase))
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description
                }).ToList();
        }

        public void UpdateCategory(CategoryUpdateDto categoryUpdateDto)
        {
            var category = BMSDataBase.Categories.FirstOrDefault(c => c.Id == categoryUpdateDto.Id);
            if (category != null)
            {
                category.Name = categoryUpdateDto.Name;
                category.Description = categoryUpdateDto.Description;
            }
            else
            {
                throw new Exception("Category not found");
            }
        }

        
        public int GetOrCreateCategory(string categoryName)
        {
            var category = BMSDataBase.Categories
                .FirstOrDefault(c => c.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase));

            if (category != null)
                return category.Id;

            
            int newId = BMSDataBase.Categories.Count == 0
                ? 1
                : BMSDataBase.Categories.Max(c => c.Id) + 1;

            var newCategory = new Category
            {
                Id = newId,
                Name = categoryName,
                Description = $"Avtomatik yaradılıb: {categoryName}"
            };

            BMSDataBase.Categories.Add(newCategory);
            FileStorage.SaveCategories("Categories.txt", BMSDataBase.Categories);

            return newId;
        }
    }
}