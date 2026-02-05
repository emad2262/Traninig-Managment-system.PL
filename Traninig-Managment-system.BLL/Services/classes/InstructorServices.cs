using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Traninig_Managment_system.BLL.Helper;
using Traninig_Managment_system.DAL.Model;
using Traninig_Managment_system.DAL.Repo;
using Traninig_Managment_system.DAL.Repo.Irepo;

namespace Traninig_Managment_system.BLL.Services.classes
{
    public class InstructorServices : IInstructorServices
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IInstructorRepo _instructorRepo;

        public InstructorServices(UserManager<ApplicationUser> userManager,IInstructorRepo instructorRepo) 
        {
            _userManager = userManager;
            _instructorRepo = instructorRepo;
        }
        public async Task<bool> AddInstructor(CreaeInstructorVm model, int CompanyId)
        {
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                throw new Exception("Email already exists");
            }
            var applicationUser = new ApplicationUser
            {
                UserName = model.FullName,
                Email = model.Email,
                CompanyId = CompanyId,
            };
            var result =await _userManager.CreateAsync(applicationUser, model.Password);
            if (!result.Succeeded)
            {
                return false; 
            }
            await _userManager.AddToRoleAsync(applicationUser, SD.Instructor);

            var instructor = new Instructor
            {
                FullName = model.FullName,
                Email = model.Email,
                Specialization = model.Specialization,
                IsActive = model.IsActive,
                CreatedAt = DateTime.Now,
                UserId = applicationUser.Id, 
                CompanyId = CompanyId


            };
           return  await _instructorRepo.CreateAsync(instructor);
        }


        public async Task<EditInstructorVm> GetinsturactorByIdAsync(int InstructorId, int companyId)
        {
            var instructor = await _instructorRepo.GetOneAsync(e=>e.Id==InstructorId && e.CompanyId==companyId);
            if (instructor == null)
            {
                throw new Exception("Instructor not found");
            }
            return new EditInstructorVm
            {
                Id=instructor.Id,
                FullName=instructor.FullName,
                Email=instructor.Email,
                IsActive=instructor.IsActive
            };
        }
        public async Task<bool> EditinsturactorByIdAsync(EditInstructorVm model, int CompanyId)
        {
            var instructor = await _instructorRepo.GetOneAsync(e => e.Id == model.Id && e.CompanyId == CompanyId);
            if (instructor == null)
            {
                throw new Exception("Instructor not found");
            }

            instructor.FullName = model.FullName;
            instructor.Email = model.Email;
            instructor.IsActive = model.IsActive;
            return await _instructorRepo.UdateAsync(instructor);
        }


        public async Task<IEnumerable<ListInstructorVm>> GetListInstructorAsync(int CompanyId)
        {
            var instructorlist= await _instructorRepo.GetAllAsync(i => i.CompanyId == CompanyId);
            var instructorVmList = instructorlist.Select(instructor => new ListInstructorVm
            {
                Id = instructor.Id,
                FullName = instructor.FullName,
                Email = instructor.Email,
                Specialization = instructor.Specialization,
                IsActive = instructor.IsActive,
                CreatedAt = instructor.CreatedAt
            });
            return instructorVmList;

        }
        public async Task<bool> Delete(int Instructorid, int companyId)
        {
            var instructordelet = await _instructorRepo.GetOneAsync(
                e => e.Id == Instructorid && e.CompanyId == companyId
            );

            if (instructordelet == null)
                return false;

            // 1️⃣ احذف اليوزر من Identity
            var user = await _userManager.FindByIdAsync(instructordelet.UserId);
            if (user != null)
            {
                var identityResult = await _userManager.DeleteAsync(user);
                if (!identityResult.Succeeded)
                    return false;
            }


            await _instructorRepo.Delete(instructordelet);
            return true;
        }
    }
}
