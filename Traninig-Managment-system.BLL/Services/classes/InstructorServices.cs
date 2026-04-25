using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Traninig_Managment_system.BLL.Services.Interfaces;

namespace Traninig_Managment_system.BLL.Services.classes
{
    public class InstructorServices : IInstructorServices
    {
        private readonly IInstructorRepo _instructorRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public InstructorServices(IInstructorRepo instructorRepo, UserManager<ApplicationUser> userManager)
        {
            _instructorRepo = instructorRepo;
            _userManager = userManager;
        }

        public async Task<InstructorDetails> GetInstructorDetailsAsync(int companyId, int id)
        {
            var instructor = await _instructorRepo.GetOneAsync(i => i.Id == id && i.CompanyId == companyId, i => i.Courses);

            if (instructor == null)
            {
                throw new Exception("Instructor not found");
            }

            return new InstructorDetails
            {
                FullName = instructor.FullName,
                CreatedAt = instructor.CreatedAt,
                Specialization = instructor.Specialization,
                Courses = instructor.Courses?.Select(c => new CourseDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate
                }).ToList() ?? new List<CourseDto>()

            };

        }

        public async Task<IEnumerable<ListInstructorVm>> GetListInstructorAsync(int companyId)
        {
            var Instructor = await _instructorRepo.GetAllAsync(i => i.CompanyId == companyId);

            return Instructor.Select(i => new ListInstructorVm
            {
                Id = i.Id,
                FullName = i.FullName,
                Specialization = i.Specialization,
                CreatedAt = i.CreatedAt
            }).ToList();

        }
        public async Task<ServiceResult<int>> CreateInstructorAsync(int companyId, CreateInstructorVm model)
        {
            var existingUser = await _userManager.FindByEmailAsync(model.Email);

            if (existingUser != null)
            {
                return new ServiceResult<int>
                {
                    Message = "Email already exists",
                    IsSuccess = false
                };
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                CompanyId = companyId
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                var roleResult = await _userManager.AddToRoleAsync(user, SD.Instructor);

                if (!roleResult.Succeeded)
                {
                    await _userManager.DeleteAsync(user);

                    return new ServiceResult<int>
                    {
                        Message = "Failed to assign role",
                        IsSuccess = false
                    };
                }
            }
            else
            {
                return new ServiceResult<int>
                {
                    Message = "email didnt register",
                    IsSuccess = false,
                };

            }
            var instructor = new Instructor
            {
                FullName = model.Name,
                Email = model.Email,
                Specialization = model.Specialization,
                CompanyId = companyId,
                UserId = user.Id,
                IsActive = true
            };
            try
            {
                await _instructorRepo.CreateAsync(instructor);

                return new ServiceResult<int>
                {
                    Data = instructor.Id,
                    Message = "instructor is register ",
                    IsSuccess = true,
                };
            }
            catch (Exception ex)
            {
                // لو حصلت مشكلة في جدول الموظفين، امسح اليوزر اللي اتكريت في الـ Identity عشان الداتا متبقاش "يتيمة"
                await _userManager.DeleteAsync(user);
                return new ServiceResult<int>
                {
                    Message = "instructor didnt register",
                    IsSuccess = false,
                };
            }

        }



        public async Task<ServiceResult<bool>> EditInstructorAsync(int companyId, EditInstructorVm model)
        {
            var instructor = await _instructorRepo.GetOneAsync(i => i.Id == model.Id && i.CompanyId == companyId);
            if (instructor == null)
            {
                return new ServiceResult<bool>
                {
                    Message = "Instructor not found",
                    IsSuccess = false,
                    Data = false
                };
            }
            instructor.FullName = model.Name;
            instructor.Specialization = model.Specialization;
            instructor.IsActive = model.Isactive;
            

            try
            {
                await _instructorRepo.UpdateAsync(instructor);

                return new ServiceResult<bool>
                {
                    Message = "Instructor updated successfully",
                    IsSuccess = true,
                    Data = true
                };
            }
            catch
            {
                return new ServiceResult<bool>
                {
                    Message = "Instructor update failed",
                    IsSuccess = false,
                    Data = false
                };
            }
        }
        public async Task<ServiceResult<bool>> DeleteInstructorAsync(int companyId, int id)
        {
            var instructor = await _instructorRepo.GetOneAsync(
                i => i.Id == id && i.CompanyId == companyId);

            if (instructor == null)
            {
                return new ServiceResult<bool>
                {
                    Message = "Instructor not found",
                    IsSuccess = false,
                    Data = false
                };
            }

            var user = await _userManager.FindByIdAsync(instructor.UserId);

            try
            {
                if (user != null)
                {
                    var userResult = await _userManager.DeleteAsync(user);

                    if (!userResult.Succeeded)
                    {
                        return new ServiceResult<bool>
                        {
                            Message = "Failed to delete user",
                            IsSuccess = false,
                            Data = false
                        };
                    }
                }

                await _instructorRepo.Delete(instructor);

                return new ServiceResult<bool>
                {
                    Message = "Instructor deleted successfully",
                    IsSuccess = true,
                    Data = true
                };
            }
            catch
            {
                return new ServiceResult<bool>
                {
                    Message = "Delete operation failed",
                    IsSuccess = false,
                    Data = false
                };
            }
        }
    }
}
