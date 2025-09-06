using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ShipmentTrackingSystem.Data;
using ShipmentTrackingSystem.Hubs;
using ShipmentTrackingSystem.Models;
using ShipmentTrackingSystem.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IShipmentService, ShipmentService>();
builder.Services.AddSingleton<IGeoService, GeoService>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

app.MapControllerRoute(
    name: "shortTrack",
    pattern: "t/{id}",
    defaults: new { controller = "Tracking", action = "Index" });

app.MapHub<TrackingHub>("/hubs/tracking");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed default user
// داخل Program.cs قبل app.Run()
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    const string email = "user@demo.local";
    const string password = "User@123";

    var hash = BitConverter.ToString(SHA256.HashData(Encoding.UTF8.GetBytes(password)))
               .Replace("-", "").ToLowerInvariant();

    var existing = db.Users.SingleOrDefault(u => u.Email == email);
    if (existing == null)
    {
        db.Users.Add(new AppUser { Email = email, PasswordHash = hash, FullName = "Demo User" });
    }
    else if (existing.PasswordHash == "seed-hash")
    {
        existing.PasswordHash = hash;
    }
    db.SaveChanges();
}


app.Run();
