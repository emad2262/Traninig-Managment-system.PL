
namespace Traninig_Managment_system.DAL.Data
{
    public class ApplicationDbContext :  IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> dbContextOptions): base(dbContextOptions) 
        { 

        }
      

        public DbSet<AdminCompany> adminCompanies { get; set; }
        public DbSet<AdminPlatform> adminPlatforms { get; set; }
        public DbSet<Company> companies { get; set; }
        public DbSet<Employee> employees { get; set; }
        public DbSet<Courses> courses { get; set; }
        public DbSet<Lesson> lessons { get; set; }
        public DbSet<EmployeeCourse> EmployeeCourses { get; set; }
        public DbSet<Feature> features { get; set; }
        public DbSet<Plan> plans { get; set; }
        public DbSet<PlanFeature> PlanFeatures { get; set; }

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

            modelBuilder.Entity<PlanFeature>(entity =>
            {
                // Composite PK
                entity.HasKey(pf => new { pf.PlanId, pf.FeatureId });

                // Plan -> PlanFeature
                entity.HasOne(pf => pf.Plan)
                      .WithMany(p => p.PlanFeatures)
                      .HasForeignKey(pf => pf.PlanId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Feature -> PlanFeature
                entity.HasOne(pf => pf.Feature)
                      .WithMany(f => f.PlanFeatures)
                      .HasForeignKey(pf => pf.FeatureId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(pf => pf.IsEnabled)
                      .HasDefaultValue(true);
            });
            base.OnModelCreating(modelBuilder);
        }
    }
}
