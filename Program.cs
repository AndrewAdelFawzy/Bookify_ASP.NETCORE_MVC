using Boookify.Web.Core.Mapping;
using Boookify.Web.Seeds;
using System.Reflection;
using UoN.ExpressiveAnnotations.NetCore.DependencyInjection;
using Boookify.Web.Settings;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.DataProtection;
using WhatsAppCloudApi.Extensions;
using Hangfire;
using Serilog;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.Configure<SecurityStampValidatorOptions>(options =>
    options.ValidationInterval = TimeSpan.Zero);


builder.Services.AddDatabaseDeveloperPageExceptionFilter();

//builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
//    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();

builder.Services.AddDataProtection().SetApplicationName(nameof(Bookify));
builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>,ApplicationUserClaimsPrincipalFactory>();

builder.Services.AddTransient<IImageService,ImageService>();
builder.Services.AddTransient<IEmailSender,EmailSender>();
builder.Services.AddTransient<IEmailBodyBuilder, EmailBodyBuilder>();

builder.Services.AddControllersWithViews();

builder.Services.AddAutoMapper(Assembly.GetAssembly(typeof(MappingProfile)));
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection(nameof(MailSettings)));
builder.Services.AddWhatsAppApiClient(builder.Configuration);

builder.Services.AddExpressiveAnnotations();

builder.Services.AddHangfire(x => x.UseSqlServerStorage(connectionString));
builder.Services.AddHangfireServer();

//Add Serilog
Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();
builder.Host.UseSerilog();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
using var scope = scopeFactory.CreateScope();

var RoleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
var UserManager = scope.ServiceProvider.GetService<UserManager<ApplicationUser>>();

await DefaultRoles.SeedRoles(RoleManager);
await DefaultUsers.SeedAdminUser(UserManager);

app.UseHangfireDashboard("/hangfire");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
