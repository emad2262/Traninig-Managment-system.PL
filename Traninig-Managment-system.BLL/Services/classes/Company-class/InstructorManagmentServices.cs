
using Traninig_Managment_system.BLL.Dtos;
using Traninig_Managment_system.BLL.Dtos.Instructor;

namespace Traninig_Managment_system.BLL.Services.classes
{
    public class InstructorManagmentServices : IInstructorServices
    {
        private readonly IInstructorRepo _instructorRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public InstructorManagmentServices(
            IInstructorRepo instructorRepo,
            UserManager<ApplicationUser> userManager
            )
        {
            _instructorRepo = instructorRepo;
            _userManager = userManager;
        }

        public async Task<IEnumerable<ListInstructorVm>> GetListInstructorAsync(int companyId)
        {
            var Instructor = await _instructorRepo.GetAllAsync(i => i.CompanyId == companyId);

            return Instructor.Select(i => new ListInstructorVm
            {
                Id = i.Id,
                FullName = i.FullName,
                Specialization = i.Specialization??"",
            }).ToList();

        }
        public async Task<InstructorDetailsDto> GetInstructorDetailsAsync(int companyId, int id)
        {
            var instructor = await _instructorRepo.GetOneAsync(i => i.Id == id && i.CompanyId == companyId);

            if (instructor == null)
            {
                return null;
            }

            return new InstructorDetailsDto
            {
                Id = instructor.Id,
                FullName = instructor.FullName,
                Email = instructor.Email,
                IsActive = instructor.IsActive,
                CreateAt = instructor.CreatedAt,
                Specialization = instructor.Specialization??"",
                TotalCourses = instructor.Courses?.Count ?? 0,
                ProfileImage=instructor.Image,
            };

        }

        public async Task<ServiceResult<int>> CreateInstructorAsync(int companyId, CreateInstructorDto model)
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


            try
            {
                var result = await _userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                {
                    return new ServiceResult<int>
                    {
                        Message = string.Join(" | ", result.Errors.Select(e => e.Description)),
                        IsSuccess = false
                    };
                }

                var roleResult = await _userManager.AddToRoleAsync(user, SD.Instructor);
                if (!roleResult.Succeeded)
                {
                    return new ServiceResult<int>
                    {
                        Message = string.Join(" | ", roleResult.Errors.Select(e => e.Description)),
                        IsSuccess = false
                    };
                }

                var instructor = new Instructor
                {
                    FullName = model.Name,
                    Email = model.Email,
                    CompanyId = companyId,
                    UserId = user.Id,
                    IsActive = true
                };

                await _instructorRepo.CreateAsync(instructor);
                await _instructorRepo.SaveChangesAsync();

                return new ServiceResult<int>
                {
                    Data = instructor.Id,
                    Message = "Instructor registered successfully",
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<int>
                {
                    Message = $"{ex.Message}Instructor registration failed",
                    IsSuccess = false
                };
            }

        }

        public async Task<bool> EditInstructorAsync(int companyId, EditInstructorDto model)
        {
            var instructor = await _instructorRepo.GetOneAsync(i => i.Id == model.Id && i.CompanyId == companyId);

            if (instructor == null)
            {

            }
            instructor.Id = model.Id;
            instructor.FullName = model.FullName;
            instructor.CompanyId = companyId;
            await _instructorRepo.Update(instructor);
            await _instructorRepo.SaveChangesAsync();
            return true;
        }
        public async Task<bool> DeleteInstructorAsync(int companyId, int id)
        {
            var instructor = await _instructorRepo.GetOneAsync(
                i => i.Id == id && i.CompanyId == companyId);

            var user = await _userManager.FindByIdAsync(instructor.UserId);

                if (user != null)
                {
                    var userResult = await _userManager.DeleteAsync(user);

                    if (!userResult.Succeeded)
                    {
                        return false;
                    }
                }
                await _instructorRepo.Delete(instructor);

            return true;
        }
    }
}
