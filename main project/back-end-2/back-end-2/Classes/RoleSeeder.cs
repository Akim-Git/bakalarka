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
        }
    }
}
