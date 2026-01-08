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
    public class MemberManager : IMemberService
    {
        public void CreateMember(MemberCreateDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));
            var nameRegex = new System.Text.RegularExpressions.Regex(@"^[a-zA-ZƏəİiÖöĞğŞşÇç\s]{3,30}$");

            if (!nameRegex.IsMatch(dto.FullName))
                throw new Exception("Ad və Soyad ən azı 3 hərfdən ibarət olmalı və düzgün yazılmalıdır.");

            if (string.IsNullOrWhiteSpace(dto.FullName))
                throw new Exception("Ad boş ola bilməz");

            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new Exception("Email boş ola bilməz");

            if (BMSDataBase.Members.Any(m => m.Email == dto.Email))
                throw new Exception("Bu email artıq mövcuddur");

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber) &&
                BMSDataBase.Members.Any(m => m.PhoneNumber == dto.PhoneNumber))
                throw new Exception("Bu telefon nömrəsi artıq mövcuddur");

            int newId = BMSDataBase.Members.Count == 0
                ? 1
                : BMSDataBase.Members.Max(m => m.Id) + 1;

            Member member = new()
            {
                Id = newId,
                FullName = dto.FullName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                IsActive = dto.IsActive,
                MembershipDate = DateTime.Now

            };


            if (dto.BorrowedBookId.HasValue)
            {
                var book = BMSDataBase.Books
                    .FirstOrDefault(b => b.Id == dto.BorrowedBookId.Value);

                if (book == null)
                    throw new Exception("Seçilən kitab tapılmadı");

                if (!book.IsAvailable)
                    throw new Exception("Kitab artıq götürülüb");

                book.IsAvailable = false;
                member.IsActive = false;
                member.BorrowedBookId = book.Id;
            }

            BMSDataBase.Members.Add(member);
        }






        public void DeleteMember(int id)
        {
            var member = BMSDataBase.Members.FirstOrDefault(m => m.Id == id);
            if (member != null)
            {
                BMSDataBase.Members.Remove(member);
            }
            else
            {
                throw new Exception("Member not found");
            }

        }

        public List<MemberDto> GetAllMembers()
        {
            return BMSDataBase.Members.Select(m => new MemberDto
            {
                Id = m.Id,
                FullName = m.FullName,
                Email = m.Email,
                PhoneNumber = m.PhoneNumber,
                IsActive = m.IsActive,
                MembershipDate = m.MembershipDate
            }).ToList();

        }

        public MemberDto GetMemberById(int id)
        {
            var member = BMSDataBase.Members.FirstOrDefault(m => m.Id == id);
            if (member != null)
            {
                return new MemberDto
                {
                    Id = member.Id,
                    FullName = member.FullName,
                    Email = member.Email,
                    PhoneNumber = member.PhoneNumber,
                    IsActive = member.IsActive,
                    MembershipDate = member.MembershipDate
                };
            }
            else
            {
                throw new Exception("Member not found");
            }


        }

        public List<MemberDto> SearchMembers(MemberSearchDto memberSearchDto)
        {
            var query = BMSDataBase.Members.AsQueryable();


            if (!string.IsNullOrWhiteSpace(memberSearchDto.FullName))
            {
                query = query.Where(m => m.FullName.Contains(memberSearchDto.FullName, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(memberSearchDto.Email))
            {
                query = query.Where(m => m.Email.Contains(memberSearchDto.Email, StringComparison.OrdinalIgnoreCase));
            }

            return query.Select(m => new MemberDto
            {
                Id = m.Id,
                FullName = m.FullName,
                Email = m.Email,
                PhoneNumber = m.PhoneNumber,
                IsActive = m.IsActive,
                MembershipDate = m.MembershipDate
            }).ToList();
        }

        public void UpdateMember(MemberUpdateDto memberUpdateDto)
        {
            if (memberUpdateDto == null)
                throw new ArgumentNullException(nameof(memberUpdateDto));

            var member = BMSDataBase.Members
                .FirstOrDefault(m => m.Id == memberUpdateDto.Id);

            if (member == null)
                throw new Exception("Üzv tapılmadı");

            if (!string.IsNullOrWhiteSpace(memberUpdateDto.Email))
            {
                if (BMSDataBase.Members.Any(m =>
                    m.Id != memberUpdateDto.Id &&
                    m.Email == memberUpdateDto.Email))
                {
                    throw new Exception("Bu Email artıq mövcuddur!");
                }

                member.Email = memberUpdateDto.Email.Trim();
            }

            if (!string.IsNullOrWhiteSpace(memberUpdateDto.PhoneNumber))
            {
                if (BMSDataBase.Members.Any(m =>
                    m.Id != memberUpdateDto.Id &&
                    m.PhoneNumber == memberUpdateDto.PhoneNumber))
                {
                    throw new Exception("Bu telefon nömrəsi artıq mövcuddur!");
                }

                member.PhoneNumber = memberUpdateDto.PhoneNumber.Trim();
            }

            if (!string.IsNullOrWhiteSpace(memberUpdateDto.FullName))
                member.FullName = memberUpdateDto.FullName.Trim();


            if (memberUpdateDto.BorrowedBookId.HasValue)
            {
                var book = BMSDataBase.Books.FirstOrDefault(b => b.Id == memberUpdateDto.BorrowedBookId.Value);
                if (book == null)
                    throw new Exception("Seçilən kitab tapılmadı");


                book.IsAvailable = false;
                member.IsActive = false;
                member.BorrowedBookId = book.Id;
            }
            else
            {

                member.IsActive = memberUpdateDto.IsActive;
            }
        }







        public List<MemberDto> GetAllMembersWithBooks()
        {
            return BMSDataBase.Members.Select(m => new MemberDto
            {
                Id = m.Id,
                FullName = m.FullName,
                Email = m.Email,
                PhoneNumber = m.PhoneNumber,
                IsActive = m.IsActive,
                MembershipDate = m.MembershipDate,
                BorrowedBookId = m.BorrowedBookId,

                BookTitle = m.BorrowedBookId.HasValue
                    ? BMSDataBase.Books.FirstOrDefault(b => b.Id == m.BorrowedBookId)?.Title
                    : "Kitab seçilməyib"
            }).ToList();
        }



        private static string Normalize(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            return s.Trim().ToLowerInvariant();
        }

        public List<BookDto> SearchBooksFuzzy(string query)
        {
            string q = Normalize(query);
            return BMSDataBase.Books
                .Where(b =>
                    Normalize(b.Title).Contains(q) ||
                    Normalize(b.Author).Contains(q) ||
                    Normalize(b.ISBN).Contains(q))
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
        }

        public void ShowAuthorsWithBookCount()
        {
            var authors = BMSDataBase.Books
                .GroupBy(b => b.Author)
                .Select(g => new { Author = g.Key, Count = g.Count() })
                .ToList();

            foreach (var a in authors)
            {
                Console.WriteLine($"Müəllif: {a.Author}, Kitab sayı: {a.Count}");
            }
        }

        public void ShowAllBooks()
        {
            Console.WriteLine("📚 Bütün kitabların siyahısı:");
            foreach (var b in BMSDataBase.Books)
            {
                Console.WriteLine($"{b.Id} - {b.Title} | {b.Author} | ISBN: {b.ISBN}");
            }
        }



    }
}