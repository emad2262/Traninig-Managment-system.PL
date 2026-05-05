using Traninig_Managment_system.BLL.Helper;
using Traninig_Managment_system.BLL.Services;
using Traninig_Managment_system.BLL.Services.classes;
using Traninig_Managment_system.BLL.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>
    (option=>option.UseSqlServer(builder.Configuration.GetConnectionString("defaultconnection")));

// identity role
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
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
// add azure blob storage options
builder.Services.Configure<AzureBlobStorageOptions>(
    builder.Configuration.GetSection("AzureBlobStorage")
);
//scoped services
builder.Services.AddScoped<ICompanyRepo, CompanyRepo>();
builder.Services.AddScoped<IPlanRepo, PlanRepo>();
builder.Services.AddScoped<IPlanFeatureRepo, PlanFeatureRepo>();
builder.Services.AddScoped<ICompanyNotificationRepo, CompanyNotificationRepo>();
builder.Services.AddScoped<ICompanyDashboardService, CompanyDashboardService>();
builder.Services.AddScoped<IManagerAreaService, ManagerAreaService>();
builder.Services.AddScoped<ICourseRepo, CoursesRepo>();
builder.Services.AddScoped<ICourseServices, CourseServices>();
builder.Services.AddScoped<IEmployeeRepo, EmployeeRepo>();
builder.Services.AddScoped<IEmployeeCourseRepo, EmployeeCourseRepo>();
builder.Services.AddScoped<IEmployeeLessonRepo, EmployeeLessonRepo>();
builder.Services.AddScoped<IEmployeeExamAttemptRepo, EmployeeExamAttemptRepo>();
builder.Services.AddScoped<ILessonRepo, LessonRepo>();
builder.Services.AddScoped<IExamRepo, ExamRepo>();
builder.Services.AddScoped<IBadgeRepo, BadgeRepo>();
builder.Services.AddScoped<IEmployeeBadgeRepo, EmployeeBadgeRepo>();
builder.Services.AddScoped<IEmployeeManagementService, EmployeeManagementService>();
builder.Services.AddScoped<IEmployeeWorkspaceService, EmployeeWorkspaceService>();
builder.Services.AddScoped<ICategoryService, CategoryService>(); 
builder.Services.AddScoped<ICategoryRepo, CategoryRepo>();
builder.Services.AddScoped<IInstructorRepo, InstructorRepo>();
builder.Services.AddScoped<IInstructorServices, InstructorServices>();
builder.Services.AddScoped<IInstructorWorkspaceService, InstructorWorkspaceService>();

//////////utility
builder.Services.AddScoped<IDBInitializer, DBInitializer>();
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddScoped<IFileService, FileService>();

var app = builder.Build();

// 🔥 RUN DB INITIALIZER
using (var scope = app.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider.GetRequiredService<IDBInitializer>();
    dbInitializer.Initialize();
}
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
