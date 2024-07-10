using LicenseStat24.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LicenseStat24.Data
{
    public class IdentityContext : IdentityDbContext<LicenseStat24User>
    {
        public IdentityContext(DbContextOptions<IdentityContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            string user_admin_security_stamp = "A36WOCEEP5MKS62ZMKBETZJA56SDWFCW";
            string user_admin_concurency_stamp = "f83c81c1-cfc4-47e6-ba8f-d0555bc791af";
            string user_admin_guid = "5e506f3c-f797-4962-a386-736dccf327c0";

            builder.Entity<IdentityRole>()
                   .HasData(new IdentityRole
                   {
                       Id = "1",
                       Name = "ADMIN",
                       NormalizedName = "ADMIN",
                       ConcurrencyStamp = ""
                   });

            builder.Entity<IdentityRole>()
                   .HasData(new IdentityRole
                   {
                       Id = "2",
                       Name = "ANALYSIS",
                       NormalizedName = "ANALYSIS",
                       ConcurrencyStamp = ""
                   });

            LicenseStat24User userAdmin = new LicenseStat24User()
            {
                Id = user_admin_guid,
                UserName = "admin@company.ru",
                NormalizedUserName = "ADMIN@COMPANY.RU",
                Email = "admin@company.ru",
                NormalizedEmail = "ADMIN@COMPANY.RU",
                SecurityStamp = user_admin_security_stamp,
                ConcurrencyStamp = user_admin_concurency_stamp,
                LockoutEnabled = true,
                EmailConfirmed = true
            };
            userAdmin.PasswordHash = new PasswordHasher<LicenseStat24User>().HashPassword(userAdmin, "SuperStrongAdminPassword");

            builder.Entity<LicenseStat24User>()
                   .HasData(userAdmin);

            builder.Entity<IdentityUserRole<string>>()
                   .HasData(new IdentityUserRole<string>
                   {
                       RoleId = "1",
                       UserId = user_admin_guid
                   });
        }
    }
}
