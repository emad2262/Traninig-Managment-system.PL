using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Traninig_Managment_system.BLL.Dtos.Instructor;
using Traninig_Managment_system.BLL.Helper;
using Traninig_Managment_system.BLL.Services;

namespace Traninig_Managment_system.PL.Areas.Company.Controllers
{
    [Area("Company")]
    [Authorize(Roles = SD.Company)]   // ← عدّل الاسم لو الرول عندك اسمه CompanyAdmin مثلاً
    public class InstructorController : Controller
    {
        private readonly IInstructorServices _instructorServices;
        private readonly IFileService _fileService;

        private const string ImagesFolder = "images/instructors";

        public InstructorController(IInstructorServices instructorServices, IFileService fileService)
        {
            _instructorServices = instructorServices;
            _fileService = fileService;
        }

        // الـ CompanyId جاي من الـ Claims — الـ Service هو اللي بيتحقق منه على كل query
        private int CompanyId =>
            int.Parse(User.FindFirstValue("CompanyId") ?? "0");

        // ── INDEX ────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var dtos = (await _instructorServices.GetListInstructorAsync(CompanyId)).ToList();
            var busiest = dtos.Count > 0 ? dtos.Max(d => d.CoursesCount) : 0;

            var vm = new InstructorIndexViewModel
            {
                Instructors = dtos.Select(d => new InstructorCardViewModel
                {
                    Id = d.Id,
                    FullName = d.FullName,
                    Email = d.Email,
                    Specialization = d.Specialization,
                    Image = d.Image,
                    IsActive = d.IsActive,
                    CoursesCount = d.CoursesCount,
                    LoadPercent = busiest == 0 ? 0 : (int)Math.Round(d.CoursesCount * 100d / busiest)
                }).ToList()
            };

            return View(vm);
        }

        // ── DETAILS ──────────────────────────────────────────
        public async Task<IActionResult> Details(int id)
        {
            var dto = await _instructorServices.GetInstructorDetailsAsync(CompanyId, id);
            if (dto is null) return NotFound();

            var vm = new InstructorDetailsViewModel
            {
                Id = dto.Id,
                FullName = dto.FullName,
                Email = dto.Email,
                Specialization = dto.Specialization,
                ProfileImage = dto.ProfileImage,
                IsActive = dto.IsActive,
                CreateAt = dto.CreateAt,
                TotalCourses = dto.TotalCourses,
                RunningCourses = dto.RunningCourses,
                TotalStudents = dto.TotalStudents,
                Courses = dto.Courses.Select(c => new InstructorCourseViewModel
                {
                    Id = c.Id,
                    Title = c.Title,
                    CategoryName = c.CategoryName,
                    Image = c.Image,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    EnrolledCount = c.EnrolledCount,
                    Timeline = c.Timeline
                }).ToList()
            };

            return View(vm);
        }

        // ── CREATE ───────────────────────────────────────────
        [HttpGet]
        public IActionResult Create() => View(new CreateInstructorViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateInstructorViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            string? imagePath = null;
            if (vm.ImageFile is not null)
            {
                imagePath = await _fileService.UploadFileAsync(vm.ImageFile, ImagesFolder);
                if (imagePath is null)
                {
                    ModelState.AddModelError(nameof(vm.ImageFile), "Use a JPG or PNG under 2 MB");
                    return View(vm);
                }
            }

            var result = await _instructorServices.CreateInstructorAsync(CompanyId, new CreateInstructorDto
            {
                Name = vm.FullName,
                Email = vm.Email,
                Password = vm.Password,
                Specialization = vm.Specialization,
                Image = imagePath
            });

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(vm);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Details), new { id = result.Data });
        }

        // ── EDIT ─────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var dto = await _instructorServices.GetInstructorForUpdate(CompanyId, id);
            if (dto is null) return NotFound();

            return View(new EditInstructorViewModel
            {
                Id = dto.Id,
                FullName = dto.FullName,
                Specialization = dto.Specialization,
                IsActive = dto.IsActive,
                CurrentImage = dto.Image
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditInstructorViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            string? imagePath = null;
            if (vm.ImageFile is not null)
            {
                imagePath = await _fileService.UpdateFileAsync(vm.ImageFile, vm.CurrentImage, ImagesFolder);
                if (imagePath is null)
                {
                    ModelState.AddModelError(nameof(vm.ImageFile), "Use a JPG or PNG under 2 MB");
                    return View(vm);
                }
            }

            var result = await _instructorServices.EditInstructorAsync(CompanyId, new EditInstructorDto
            {
                Id = vm.Id,
                FullName = vm.FullName,
                Specialization = vm.Specialization,
                IsActive = vm.IsActive,
                Image = imagePath
            });

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(vm);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Details), new { id = vm.Id });
        }

        // ── DELETE ───────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _instructorServices.DeleteInstructorAsync(CompanyId, id);

            if (result.IsSuccess) TempData["Success"] = result.Message;
            else TempData["Error"] = result.Message;

            return RedirectToAction(nameof(Index));
        }
    }
}