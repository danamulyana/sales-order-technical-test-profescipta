using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using TechnicalTest_Profescipta.Common.ConfigurationModel;
using TechnicalTest_Profescipta.Common.Interface;
using TechnicalTest_Profescipta.Common.Library;
using TechnicalTest_Profescipta.DAL.Context;
using TechnicalTest_Profescipta.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors();
builder.Services.AddControllers();

// Config Connection
AppConnectionString connection = builder.Configuration.GetSection("ConnectionStrings").Get<AppConnectionString>();
builder.Services.Configure<AppConnectionString>(builder.Configuration.GetSection("ConnectionStrings"));

// configure DI for application services  
builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

builder.Services.AddSingleton<ICustomerServices, CustomerServices>();
builder.Services.AddSingleton<IOrderServices, OrderServices>();

// Config Database 
builder.Services.AddDbContext<DBContext>(o =>
{
    o.UseSqlServer(connection.DBConnection);
    o.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.UseMemberCasing();
    options.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.Cookie.IsEssential = true;
});

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

//Injection
AppServicesHelper.Services = app.Services;

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
