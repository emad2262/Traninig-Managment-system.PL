
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
        public DbSet<Courses> courses { get; set; }
        public DbSet<Lesson> lessons { get; set; }
        public DbSet<EmployeeCourse> EmployeeCourses { get; set; }
        public DbSet<Plan> plans { get; set; }
        public DbSet<CourseCategory> CourseCategories { get; set; }
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

            modelBuilder.Entity<CourseCategory>()
            .HasOne(c => c.Company)
            .WithMany(c => c.CoursesCategories)
            .HasForeignKey(c => c.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

            // Instructor -> Course : NO ACTION
            modelBuilder.Entity<Courses>()
                .HasOne(c => c.Instructor)
                .WithMany(i => i.Courses)
                .HasForeignKey(c => c.InstructorId)
                .OnDelete(DeleteBehavior.NoAction);


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
