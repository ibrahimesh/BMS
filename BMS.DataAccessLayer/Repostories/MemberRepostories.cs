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
    public class MemberRepostories : IMember
    {
        public void Add(Member entity)
        {
            BMSDataBase.Members.Add(entity);
        }

        public void Delete(int id)
        {
            var member = GetById(id);
            if (member != null)
            {
                BMSDataBase.Members.Remove(member);
                return;
            }
            throw new Exception("Member not found");
        }

        public List<Member> GetAll()
        {
            return BMSDataBase.Members;
        }

        public Member? GetById(int id)  
        {
            return BMSDataBase.Members.FirstOrDefault(m => m.Id == id);
        }

        public List<Member> Search(string keyword)
        {
            return BMSDataBase.Members
                .Where(m => m.FullName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                            m.Email.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                            m.PhoneNumber.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public void Update(Member entity)
        {
            var member = GetById(entity.Id);
            if (member != null)
            {
                member.FullName = entity.FullName;
                member.Email = entity.Email;
                member.PhoneNumber = entity.PhoneNumber;
                member.MembershipDate = entity.MembershipDate;
                return;
            }
            throw new Exception("Member not found");
        }
    }
}