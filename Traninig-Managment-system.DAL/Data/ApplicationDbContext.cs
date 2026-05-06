
namespace Traninig_Managment_system.DAL.Data
{
    public class ApplicationDbContext :  IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> dbContextOptions): base(dbContextOptions) 
        { 

        }
      

        public DbSet<Instructor> instructors { get; set; }
        public DbSet<Company> companies { get; set; }
        public DbSet<Employee> employees { get; set; }
        public DbSet<Badge> Badges { get; set; }
        public DbSet<EmployeeBadge> EmployeeBadges { get; set; }
        public DbSet<Course> courses { get; set; }
        public DbSet<CourseChapter> CourseChapters { get; set; }
        public DbSet<Lesson> lessons { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<ExamQuestion> ExamQuestions { get; set; }
        public DbSet<EmployeeExamAttempt> EmployeeExamAttempts { get; set; }
        public DbSet<EmployeeCourse> EmployeeCourses { get; set; }
        public DbSet<Plan> plans { get; set; }
        public DbSet<PlanFeature> PlanFeatures { get; set; }
        public DbSet<CompanyNotification> CompanyNotifications { get; set; }
        public DbSet<EmployeeCertificate> EmployeeCertificates { get; set; }
        public DbSet<Category> CourseCategories { get; set; }
        public DbSet<EmployeeLesson> EmployeeLessons { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EmployeeCourse>(entity =>
            {
                entity.HasKey(ec => new { ec.EmployeeId, ec.CourseId });

                // Employee -> EmployeeCourse
                entity.HasOne(ec => ec.Employee)
                      .WithMany(e => e.EmployeeCourses)
                      .HasForeignKey(ec => ec.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade); // مسموح

                // Course -> EmployeeCourse
                entity.HasOne(ec => ec.Course)
                      .WithMany(c => c.EmployeeCourses)
                      .HasForeignKey(ec => ec.CourseId)
                      .OnDelete(DeleteBehavior.Restrict); // ❗ مهم
            });
            //////////////////
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(a => a.Instructor)
                .WithOne(c => c.User)
                .HasForeignKey<Instructor>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            ////////////////////

            modelBuilder.Entity<Category>()
            .HasOne(c => c.Company)
            .WithMany(c => c.CoursesCategories)
            .HasForeignKey(c => c.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PlanFeature>()
                .HasOne(pf => pf.Plan)
                .WithMany(p => p.Features)
                .HasForeignKey(pf => pf.PlanId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CompanyNotification>()
                .HasOne(n => n.Company)
                .WithMany(c => c.Notifications)
                .HasForeignKey(n => n.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmployeeCertificate>(entity =>
            {
                entity.HasIndex(c => new { c.EmployeeId, c.CourseId }).IsUnique();
                entity.HasIndex(c => new { c.CompanyId, c.Status });

                entity.HasOne(c => c.Employee)
                    .WithMany()
                    .HasForeignKey(c => c.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.Course)
                    .WithMany()
                    .HasForeignKey(c => c.CourseId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Instructor -> Course : NO ACTION
            modelBuilder.Entity<Course>()
                .HasOne(c => c.Instructor)
                .WithMany(i => i.Courses)
                .HasForeignKey(c => c.InstructorId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<CourseChapter>()
                .HasOne(ch => ch.Course)
                .WithMany(c => c.Chapters)
                .HasForeignKey(ch => ch.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Lesson>()
                .HasOne(l => l.Chapter)
                .WithMany(ch => ch.Lessons)
                .HasForeignKey(l => l.ChapterId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Exam>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Exams)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Exam>()
                .HasOne(e => e.Chapter)
                .WithMany(ch => ch.Exams)
                .HasForeignKey(e => e.ChapterId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ExamQuestion>()
                .HasOne(q => q.Exam)
                .WithMany(e => e.Questions)
                .HasForeignKey(q => q.ExamId)
                .OnDelete(DeleteBehavior.Cascade);


            //////////////////////employeelesson
            modelBuilder.Entity<EmployeeLesson>()
               .HasOne(el => el.Employee)
               .WithMany(e => e.EmployeeLessons)
               .HasForeignKey(el => el.EmployeeId)
               .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmployeeLesson>() 
         .HasOne(el => el.Lesson)
         .WithMany(l => l.EmployeeLessons)
         .HasForeignKey(el => el.LessonId)
         .OnDelete(DeleteBehavior.Restrict); 
            modelBuilder.Entity<EmployeeLesson>()
                .HasIndex(el => new { el.EmployeeId, el.LessonId })
                .IsUnique();

            modelBuilder.Entity<EmployeeExamAttempt>()
                .HasOne(ea => ea.Employee)
                .WithMany(e => e.EmployeeExamAttempts)
                .HasForeignKey(ea => ea.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmployeeExamAttempt>()
                .HasOne(ea => ea.Exam)
                .WithMany(e => e.EmployeeExamAttempts)
                .HasForeignKey(ea => ea.ExamId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmployeeExamAttempt>()
                .HasIndex(ea => new { ea.EmployeeId, ea.ExamId, ea.SubmittedAt });

            ////plans 
            ///
            modelBuilder.Entity<Plan>().HasData(
            new Plan
            {
                Id = 1,
                Name = "Basic",
                Price = 199,
                DurationInDays = 30,
                MaxEmployees = 20,
                MaxCourses = 5,
                IsActive = true
            },
            new Plan
            {
                Id = 2,
                Name = "Pro",
                Price = 399,
                DurationInDays = 30,
                MaxEmployees = 50,
                MaxCourses = 15,
                IsActive = true
            },
            new Plan
            {
                Id = 3,
                Name = "Premium",
                Price = 699,
                DurationInDays = 30,
                MaxEmployees = 200,
                MaxCourses = 50,
                IsActive = true
            }
            );

            base.OnModelCreating(modelBuilder);
        }
    }
}
