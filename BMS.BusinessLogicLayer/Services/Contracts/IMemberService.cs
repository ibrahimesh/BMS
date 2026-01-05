using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMS.BusinessLogicLayer.Dtos;

namespace BMS.BusinessLogicLayer.Services.Contracts
{
    public interface IMemberService
    {
        List<MemberDto> GetAllMembers();
        MemberDto GetMemberById(int id);
        void CreateMember(MemberCreateDto memberCreateDto);
        void UpdateMember(MemberUpdateDto memberUpdateDto);
        void DeleteMember(int id);
        List<MemberDto> SearchMembers(MemberSearchDto memberSearchDto);
    }
}
