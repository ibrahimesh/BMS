using BMS.DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BMS.DataAccessLayer.DataContex
{
    public class BMSDataBase
    {
        public static List<Book> Books { get; set; } = [];
        public static List<Member> Members { get; set; } = [];
        public static List<Category> Categories { get; set; } = [];



    }
}
