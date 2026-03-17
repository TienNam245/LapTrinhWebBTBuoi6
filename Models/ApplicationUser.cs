using Microsoft.AspNetCore.Identity;

namespace DuAnThuongMaiDienTu.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Mở rộng thêm các trường
        public string? FullName { get; set; }
        public string? Address { get; set; }
    }
}
