using BMS.DataAccessLayer.DataContex;
using BMS.DataAccessLayer.Models;
using BMS.DataAccessLayer.Repostories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMS.DataAccessLayer.Repostories
{
    public class CategoryRepostories : ICategory
    {
        public void Add(Category entity)
        {
            BMSDataBase.Categories.Add(entity);
        }

        public void Delete(int id)
        {
            var category = GetById(id);
            if (category != null)
            {
                BMSDataBase.Categories.Remove(category);
            }
            else
            {
                throw new Exception("Category not found");
            }
        }

        public List<Category> GetAll()
        {
            return BMSDataBase.Categories;
        }

        public Category? GetById(int id)  
        {
            return BMSDataBase.Categories.FirstOrDefault(c => c.Id == id);
        }

        public List<Category> Search(string keyword)
        {
            return BMSDataBase.Categories
                .Where(c => c.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                            c.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public void Update(Category entity)
        {
            var category = GetById(entity.Id);
            if (category != null)
            {
                category.Name = entity.Name;
                category.Description = entity.Description;
                return;
            }
            throw new Exception("Category not found");
        }
    }
}