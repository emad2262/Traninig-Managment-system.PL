using Traninig_Managment_system.BLL.Helper;
using Traninig_Managment_system.BLL.Services;
using Traninig_Managment_system.BLL.Services.classes;

namespace Traninig_Managment_system.Dependancy_Injection
{
    public static class ApplicationServiceExtensions

    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            #region add coonection with database
            services.AddDbContext<ApplicationDbContext>(option =>{
                option.UseSqlServer(configuration.GetConnectionString("defaultconnection"));
            });
            #endregion

            #region  inject repo
            services.AddScoped<ICategoryRepo, CategoryRepo>();
            services.AddScoped<IEmployeeRepo, EmployeeRepo>();
            services.AddScoped<IInstructorRepo, InstructorRepo>();
            services.AddScoped<ICourseRepo, CourseRepo>();
            services.AddScoped<ICompanyRepo, CompanyRepo>();
            services.AddScoped<IPlanRepo, PlanRepo>();
            #endregion
            #region inject services
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IEmployeeManagementService, EmployeeManagementService>();
            services.AddScoped<IInstructorServices, InstructorManagmentServices>();
            services.AddScoped<ICourseServices, CourseServices>();
            //services.AddScoped<ICompanyService, CompanyService>();
            //services.AddScoped<Iplan, PlanRepo>();
            #endregion
            #region dependancy injection 
            services.AddScoped<IDBInitializer, DBInitializer>();
            #endregion
            #region identity
            // identity role
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
                {
                    options.SignIn.RequireConfirmedEmail = false;
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequiredLength = 6;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders(); // مهم
            #endregion
            #region email sender
            services.AddScoped<IEmailSender,EmailSender>() ;
            #endregion

            return services;
        }
    }
}
