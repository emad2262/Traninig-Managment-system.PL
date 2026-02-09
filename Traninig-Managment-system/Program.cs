using Traninig_Managment_system.BLL.Services;
using Traninig_Managment_system.Utality.DBInitializer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>
    (option=>option.UseSqlServer(builder.Configuration.GetConnectionString("defaultconnection")));

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


///////////////
/// services and rrpository
/// 
builder.Services.AddScoped<ICompanyRepo, CompanyRepo>();
builder.Services.AddScoped<ICompanyServices, CompanyServices>();
builder.Services.AddScoped<IplanRepo, PlansRepo>();
builder.Services.AddScoped<IPlanService, PlanServices>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ICategoryRepo, CategoryRepo>();
builder.Services.AddScoped<ICategoryServices, CategoryServices>();
builder.Services.AddScoped<IEmployeeRepo, EmployeeRepo>();
builder.Services.AddScoped<IEmployeeServices, EmployeeServices>();
builder.Services.AddScoped<IInstructorRepo, InstructorRepo>();
builder.Services.AddScoped<IInstructorServices,InstructorServices>();
builder.Services.AddScoped<ICourseRepo,CoursesRepo>();
builder.Services.AddScoped<ICourseServices,CourseServices>();
builder.Services.AddScoped<ILessonRepo,LessonsRepo>();
builder.Services.AddScoped<ILessonServices, LessonServices>();

builder.Services.AddScoped<IMainPage, MainPageServices>();
builder.Services.AddScoped<StatisticsManager>();
//////////utility
builder.Services.AddScoped<IDBInitializer, DBInitializer>();
builder.Services.AddTransient<IEmailSender, EmailSender>();

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

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
