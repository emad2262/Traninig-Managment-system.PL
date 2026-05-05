using Traninig_Managment_system.BLL.Services.Interfaces;

namespace Traninig_Managment_system.BLL.Services
{
    public class EmployeeManagementService : IEmployeeManagementService
    {
        private const double LessonCompletionPoints = 10;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmployeeRepo _employeeRepo;
        private readonly ICourseRepo _courseRepo;
        private readonly IEmployeeCourseRepo _employeeCourseRepo;
        private readonly ICategoryRepo _categoryRepo;
        private readonly IEmployeeLessonRepo _employeeLessonRepo;
        private readonly IEmployeeExamAttemptRepo _employeeExamAttemptRepo;
        private readonly IEmployeeBadgeRepo _employeeBadgeRepo;

        public EmployeeManagementService(
            UserManager<ApplicationUser> userManager,
            IEmployeeRepo employeeRepo,
            ICourseRepo courseRepo,
            IEmployeeCourseRepo employeeCourseRepo,
            ICategoryRepo categoryRepo,
            IEmployeeLessonRepo employeeLessonRepo,
            IEmployeeExamAttemptRepo employeeExamAttemptRepo,
            IEmployeeBadgeRepo employeeBadgeRepo)
        {
            _userManager = userManager;
            _employeeRepo = employeeRepo;
            _courseRepo = courseRepo;
            _employeeCourseRepo = employeeCourseRepo;
            _categoryRepo = categoryRepo;
            _employeeLessonRepo = employeeLessonRepo;
            _employeeExamAttemptRepo = employeeExamAttemptRepo;
            _employeeBadgeRepo = employeeBadgeRepo;
        }

        public async Task<ServiceResult<int>> AddEmployeeAsync(AddEmployeeVm model, int companyId)
        {
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return new ServiceResult<int>
                {
                    Message = "Email already exists.",
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
            if (!result.Succeeded)
            {
                return new ServiceResult<int>
                {
                    Message = string.Join(", ", result.Errors.Select(e => e.Description)),
                    IsSuccess = false
                };
            }

            await _userManager.AddToRoleAsync(user, SD.Employee);

            var employee = new Employee
            {
                Name = model.Name,
                Email = model.Email,
                JobTitle = model.JobTitle ?? string.Empty,
                IsActive = model.IsActive,
                Points = 0,
                CompanyId = companyId,
                UserId = user.Id
            };

            try
            {
                await _employeeRepo.CreateAsync(employee);
                return new ServiceResult<int>
                {
                    Data = employee.Id,
                    Message = "Employee added successfully.",
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                await _userManager.DeleteAsync(user);
                return new ServiceResult<int>
                {
                    Message = "Failed to add employee: " + ex.Message,
                    IsSuccess = false
                };
            }
        }

        public async Task<IEnumerable<ListEmployeeVm>> GetEmployeesWithCoursesCountAsync(int companyId)
        {
            var employees = await _employeeRepo.GetAllAsync(
                e => e.CompanyId == companyId,
                e => e.EmployeeCourses);

            return employees.Select(e => new ListEmployeeVm
            {
                Id = e.Id,
                Name = e.Name,
                Email = e.Email,
                JobTitle = e.JobTitle,
                IsActive = e.IsActive,
                Points = e.Points,
                CoursesCount = e.EmployeeCourses.Count()
            }).ToList();
        }

        public async Task<EmployeeDetailsVm?> GetEmployeeByIdAsync(int companyId, int employeeId)
        {
            var employee = await _employeeRepo.GetEmployeeWithCoursesAsync(companyId, employeeId);
            if (employee == null)
            {
                return null;
            }

            return new EmployeeDetailsVm
            {
                Id = employee.Id,
                Name = employee.Name,
                Email = employee.Email,
                JobTitle = employee.JobTitle,
                IsActive = employee.IsActive,
                Points = employee.Points,
                courses = await BuildCourseCatalogAsync(companyId, employee)
            };
        }

        public async Task<AssignCourseVm?> GetAssignCourseDataAsync(int companyId, int employeeId, string? search = null, int? categoryId = null)
        {
            var employee = await _employeeRepo.GetEmployeeWithCoursesAsync(companyId, employeeId);
            if (employee == null)
            {
                return null;
            }

            var categories = await _categoryRepo.GetAllAsync(c => c.CompanyId == companyId);
            var courses = await BuildCourseCatalogAsync(companyId, employee, search, categoryId);

            return new AssignCourseVm
            {
                EmployeeId = employee.Id,
                EmployeeName = employee.Name,
                EmployeeEmail = employee.Email,
                JobTitle = employee.JobTitle,
                Search = search ?? string.Empty,
                CategoryId = categoryId,
                Categories = categories.Select(c => new CategoryDisplayVM
                {
                    Id = c.Id,
                    Name = c.Name,
                    CompanyId = c.CompanyId
                }).ToList(),
                Courses = courses,
                AssignedCoursesCount = courses.Count(c => c.IsAssigned)
            };
        }

        public async Task<ServiceResult<int>> AssignCoursesToEmployeeAsync(int companyId, int employeeId, IEnumerable<int> courseIds)
        {
            if (courseIds == null || !courseIds.Any())
            {
                return new ServiceResult<int> { IsSuccess = false, Message = "No courses selected." };
            }

            var employee = await _employeeRepo.GetEmployeeWithCoursesAsync(companyId, employeeId);
            if (employee == null)
            {
                return new ServiceResult<int> { IsSuccess = false, Message = "Employee not found." };
            }

            var alreadyAssigned = employee.EmployeeCourses.Select(ec => ec.CourseId).ToHashSet();

            var validCourses = await _courseRepo.GetAllAsync(
                c => courseIds.Contains(c.Id) && c.Category.CompanyId == companyId && c.IsPublished);

            var toAssign = validCourses
                .Where(c => !alreadyAssigned.Contains(c.Id))
                .ToList();

            if (!toAssign.Any())
            {
                return new ServiceResult<int>
                {
                    IsSuccess = false,
                    Message = "Selected courses are not available or already assigned."
                };
            }

            var added = 0;
            foreach (var course in toAssign)
            {
                var record = new EmployeeCourse
                {
                    EmployeeId = employeeId,
                    CourseId = course.Id,
                    AssignedAt = DateTime.UtcNow,
                    Status = CourseStatus.NotStarted,
                    Progress = 0
                };

                if (await _employeeCourseRepo.CreateAsync(record))
                {
                    added++;
                }
            }

            if (added == 0)
            {
                return new ServiceResult<int> { IsSuccess = false, Message = "Failed to assign courses." };
            }

            return new ServiceResult<int>
            {
                IsSuccess = true,
                Data = added,
                Message = added == 1
                    ? "Course assigned successfully."
                    : $"{added} courses assigned successfully."
            };
        }

        public async Task<ServiceResult<bool>> RemoveCourseAssignmentAsync(int companyId, int employeeId, int courseId)
        {
            var employee = await _employeeRepo.GetOneAsync(e => e.Id == employeeId && e.CompanyId == companyId);
            if (employee == null)
            {
                return new ServiceResult<bool> { IsSuccess = false, Data = false, Message = "Employee not found." };
            }

            var assignment = await _employeeCourseRepo.GetOneAsync(ec => ec.EmployeeId == employeeId && ec.CourseId == courseId);
            if (assignment == null)
            {
                return new ServiceResult<bool> { IsSuccess = false, Data = false, Message = "This course is not assigned to the employee." };
            }

            var course = await _courseRepo.GetOneAsync(
                c => c.Id == courseId && c.Category.CompanyId == companyId,
                c => c.Lessons,
                c => c.Exams);

            if (course == null)
            {
                return new ServiceResult<bool> { IsSuccess = false, Data = false, Message = "Course not found." };
            }

            var lessonIds = course.Lessons.Select(l => l.Id).ToHashSet();
            var examIds = course.Exams.Select(e => e.Id).ToHashSet();

            var lessonProgress = lessonIds.Any()
                ? (await _employeeLessonRepo.GetAllAsync(
                    el => el.EmployeeId == employeeId && lessonIds.Contains(el.LessonId))).ToList()
                : new List<EmployeeLesson>();

            var examAttempts = examIds.Any()
                ? (await _employeeExamAttemptRepo.GetAllAsync(
                    ea => ea.EmployeeId == employeeId && examIds.Contains(ea.ExamId))).ToList()
                : new List<EmployeeExamAttempt>();

            var allEmployeeBadges = (await _employeeBadgeRepo.GetAllAsync(
                eb => eb.EmployeeId == employeeId,
                eb => eb.Badge)).ToList();

            var earnedReasons = examIds.Select(examId => $"chapter-exam:{examId}")
                .Append($"course-completion:{courseId}")
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var relatedBadges = allEmployeeBadges
                .Where(eb => !string.IsNullOrWhiteSpace(eb.EarnedReason) &&
                             earnedReasons.Contains(eb.EarnedReason))
                .ToList();

            var pointsToRemove =
                lessonProgress.Count(lp => lp.IsCompleted) * LessonCompletionPoints +
                relatedBadges.Sum(b => b.Badge?.Points ?? 0);

            foreach (var badge in relatedBadges)
            {
                await _employeeBadgeRepo.Delete(badge);
            }

            foreach (var attempt in examAttempts)
            {
                await _employeeExamAttemptRepo.Delete(attempt);
            }

            foreach (var progress in lessonProgress)
            {
                await _employeeLessonRepo.Delete(progress);
            }

            await _employeeCourseRepo.Delete(assignment);

            if (pointsToRemove > 0)
            {
                employee.Points = Math.Max(0, employee.Points - pointsToRemove);
                await _employeeRepo.UpdateAsync(employee);
            }

            return new ServiceResult<bool>
            {
                IsSuccess = true,
                Data = true,
                Message = "Course assignment removed successfully."
            };
        }

        public async Task<ServiceResult<bool>> DeleteEmployeeAsync(int companyId, int employeeId)
        {
            var employee = await _employeeRepo.GetOneAsync(e => e.Id == employeeId && e.CompanyId == companyId);
            if (employee == null)
            {
                return new ServiceResult<bool>
                {
                    IsSuccess = false,
                    Data = false,
                    Message = "Employee not found."
                };
            }

            var user = await _userManager.FindByIdAsync(employee.UserId);

            try
            {
                if (user != null)
                {
                    var userResult = await _userManager.DeleteAsync(user);
                    if (!userResult.Succeeded)
                    {
                        return new ServiceResult<bool>
                        {
                            IsSuccess = false,
                            Data = false,
                            Message = string.Join(", ", userResult.Errors.Select(e => e.Description))
                        };
                    }
                }
                else
                {
                    var deleted = await _employeeRepo.Delete(employee);
                    if (!deleted)
                    {
                        return new ServiceResult<bool>
                        {
                            IsSuccess = false,
                            Data = false,
                            Message = "Employee could not be deleted."
                        };
                    }
                }

                return new ServiceResult<bool>
                {
                    IsSuccess = true,
                    Data = true,
                    Message = "Employee deleted successfully."
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<bool>
                {
                    IsSuccess = false,
                    Data = false,
                    Message = $"Delete operation failed: {ex.Message}"
                };
            }
        }

        private async Task<List<EmployeeCourseVm>> BuildCourseCatalogAsync(int companyId, Employee employee, string? search = null, int? categoryId = null)
        {
            var assignedMap = employee.EmployeeCourses.ToDictionary(ec => ec.CourseId);

            var companyCourses = await _courseRepo.GetAllAsync(
                c => c.Category.CompanyId == companyId,
                c => c.Category,
                c => c.Instructor!);

            var filteredCourses = companyCourses.AsEnumerable();

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                filteredCourses = filteredCourses.Where(c => c.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                filteredCourses = filteredCourses.Where(c =>
                    c.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    c.Description.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (c.Category != null && c.Category.Name.Contains(search, StringComparison.OrdinalIgnoreCase)));
            }

            return filteredCourses
                .Select(course =>
                {
                    var isAssigned = assignedMap.TryGetValue(course.Id, out var assignment);

                    return new EmployeeCourseVm
                    {
                        CourseId = course.Id,
                        CourseName = course.Title,
                        Logo = string.IsNullOrWhiteSpace(course.logo) ? null : course.logo,
                        InstructorName = course.Instructor?.FullName ?? string.Empty,
                        CategoryName = course.Category?.Name ?? string.Empty,
                        Description = course.Description,
                        DurationInHours = course.DurationInHours,
                        IsPublished = course.IsPublished,
                        IsAssigned = isAssigned,
                        Status = assignment?.Status ?? CourseStatus.NotStarted,
                        Progress = assignment?.Progress ?? 0,
                        FinalScore = assignment?.FinalScore,
                        AssignedAt = assignment?.AssignedAt,
                        CompletedAt = assignment?.CompletedAt,
                        StartDate = course.StartDate,
                        EndDate = course.EndDate
                    };
                })
                .OrderByDescending(c => c.IsAssigned)
                .ThenBy(c => c.CourseName)
                .ToList();
        }
    }
}
