using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using LicenseStat24.Data;
using LicenseStat24.Areas.Identity.Data;
using LicenseStat24.BDRepos;

var builder = WebApplication.CreateBuilder(args);
var identityConnectionString = builder.Configuration.GetConnectionString("IdentityContextConnection");
var dataConnectionString = builder.Configuration.GetConnectionString("DataContextConnection");
SqlNew.dataConnectionString = dataConnectionString;
builder.Services.AddDbContext<IdentityContext>(options => options.UseSqlServer(identityConnectionString));

builder.Services.AddDefaultIdentity<LicenseStat24User>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<IdentityContext>();


builder.Services.AddRazorPages()
            .AddRazorPagesOptions(options =>
            {
                options.Conventions.AddPageRoute("/Account/Register", "/Identity/Account/Register");
                options.Conventions.AddPageRoute("/Account/Register", "/Identity/Account/Register/{userId?}");
                options.Conventions.AddPageRoute("/Account/Register", "/Identity/Account/Register/{userId?}/{handler?}");
            });

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(90);
});


var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var dbContext1 = services.GetRequiredService<IdentityContext>();
        dbContext1.Database.Migrate();

    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка при применении миграций: {ex.Message}");
    }

}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
