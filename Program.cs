using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using DuAnThuongMaiDienTu.Models;
using DuAnThuongMaiDienTu.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext với connection string từ appsettings.json
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// THÊM IDENTITY - Xác thực và phân quyền
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

// Cấu hình Cookie Authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// Add services to the container.
builder.Services.AddControllersWithViews();

// Đăng ký repositories
builder.Services.AddScoped<IProductRepository, EFProductRepository>();
builder.Services.AddScoped<ICategoryRepository, EFCategoryRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Quan trọng: để có thể truy cập file ảnh trong wwwroot

app.UseRouting();

// THÊM Authentication - PHẢI CÓ để Identity hoạt động
app.UseAuthentication(); // Xác thực
app.UseAuthorization();  // Phân quyền

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Map Razor Pages của Identity (cho các trang Login, Register, v.v.)
app.MapRazorPages();

// Tạo Roles và Admin User khi chạy lần đầu
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // Tạo Roles "Admin" và "Member"
    string[] roles = new[] { "Admin", "Member" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // Tạo tài khoản Admin mặc định
    var adminEmail = "admin@example.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = "admin",
            Email = adminEmail,
            FullName = "Administrator",
            Address = "Hà Nội",
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(adminUser, "Admin@123");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }

    // Tạo tài khoản Member mặc định
    var memberEmail = "member@example.com";
    var memberUser = await userManager.FindByEmailAsync(memberEmail);
    if (memberUser == null)
    {
        memberUser = new ApplicationUser
        {
            UserName = "member",
            Email = memberEmail,
            FullName = "Normal Member",
            Address = "TP. Hồ Chí Minh",
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(memberUser, "Member@123");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(memberUser, "Member");
        }
    }
}

app.Run();
