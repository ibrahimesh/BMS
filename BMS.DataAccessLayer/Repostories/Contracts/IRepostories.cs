using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMS.DataAccessLayer.Models;

namespace BMS.DataAccessLayer.Repostories.Contracts
{
    public interface IRepostories<T>
    {
        void Add(T entity);

        T? GetById(int id);  

        List<T> GetAll();

        void Update(T entity);

        void Delete(int id);

        List<T> Search(string keyword);
    }
}