using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace back_end_2.Classes
{
    public class RoleSeeder
    {
        private readonly ApplicationDbContext _context;

        public RoleSeeder(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SeedRolesAsync()
        {
            string[] roleNames = { "Admin", "User", "Moderator" };

            var existingRoles = await _context.AspNetRoles
            .Where(r => roleNames.Contains(r.Name))
            .ToListAsync();

            var rolesToAdd = roleNames
            .Where(roleName => !existingRoles.Any(r => r.Name == roleName))
            .Select(roleName => new IdentityRole
            {
                Name = roleName,
                NormalizedName = roleName.ToUpper()
            })
            .ToList();

            // Přidáme nové role do databáze
            if (rolesToAdd.Any())
            {
                await _context.AspNetRoles.AddRangeAsync(rolesToAdd);
                await _context.SaveChangesAsync();
            }


            var admin = await _context.Users.FirstOrDefaultAsync(u => u.UserName == "AkimAdmin");

            if (admin != null)
            {
                // 1. Najdeme nebo vytvoříme roli Admin
                var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
                if (adminRole == null)
                {
                    adminRole = new IdentityRole
                    {
                        Name = "Admin",
                        NormalizedName = "ADMIN"
                    };
                    _context.Roles.Add(adminRole);
                    await _context.SaveChangesAsync();
                }

                // 2. Najdeme roli User (pro smazání)
                var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
                if (userRole != null)
                {
                    var oldUserRole = await _context.UserRoles
                        .FirstOrDefaultAsync(ur => ur.UserId == admin.Id && ur.RoleId == userRole.Id);

                    if (oldUserRole != null)
                    {
                        _context.UserRoles.Remove(oldUserRole);
                        Console.WriteLine("🗑️ Odebrána role 'User' uživateli 'AkimAdmin'.");
                    }
                }

                // 3. Zkontrolujeme, zda už má roli Admin
                var hasAdminRole = await _context.UserRoles
                    .AnyAsync(ur => ur.UserId == admin.Id && ur.RoleId == adminRole.Id);

                if (!hasAdminRole)
                {
                    _context.UserRoles.Add(new IdentityUserRole<string>
                    {
                        UserId = admin.Id,
                        RoleId = adminRole.Id
                    });
                    Console.WriteLine(" Rola 'Admin' byla přiřazena uživateli 'AkimAdmin'.");
                }
                else
                {
                    Console.WriteLine(" Uživatel 'AkimAdmin' už má roli 'Admin'.");
                }

                await _context.SaveChangesAsync();
            }
            else
            {
                Console.WriteLine("Uživatel 'AkimAdmin' nebyl nalezen.");
            }

        }
    }
}
