using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using back_end_2.Models;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System;
using System.Data;
using Microsoft.EntityFrameworkCore;
using back_end_2.Helpers;
using Microsoft.Extensions.Options;


[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly EmailService _emailService;
    private readonly ApplicationDbContext _context;
    public AuthController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IConfiguration configuration,
        ApplicationDbContext context,
        EmailService emailService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _context = context;
        _emailService = emailService;
    }

    

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest("Zadané údaje nejsou platné.");
        }

        var existingUserByEmail = await _userManager.FindByEmailAsync(model.Email);
        var existingUserByUsername = await _userManager.FindByNameAsync(model.Username);

        if (existingUserByEmail != null || existingUserByUsername != null)
        {
            return BadRequest("Tento e-mail nebo uživatelské jméno jsou již používány.");
        }

        var user = new IdentityUser { UserName = model.Username, Email = model.Email };
        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            // Přiřazení role "User" pomocí ApplicationDbContext
            var userRole = new IdentityUserRole<string>
            {
                UserId = user.Id,
                RoleId = (await _context.AspNetRoles.FirstOrDefaultAsync(r => r.Name == "User"))?.Id
            };

            if (userRole.RoleId != null)
            {
                await _context.AspNetUserRoles.AddAsync(userRole);
                await _context.SaveChangesAsync();
            }
            else
            {
                return BadRequest("Role 'User' neexistuje.");
            }

            var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = Url.Action("ConfirmEmail", "Auth", new { userId = user.Id, token = emailConfirmationToken }, Request.Scheme);
            _emailService.SendEmail(user.Email, "Potvrzení e-mailu", $"Prosím, potvrďte svůj e-mail kliknutím na tento odkaz: <a href='{confirmationLink}'>Potvrdit e-mail</a>");

            return Ok(new { message = "Registrace proběhla úspěšně. Zkontrolujte prosím svůj e-mail a potvrďte svůj účet." });
        }

        return BadRequest("Registrace selhala: " + string.Join(", ", result.Errors.Select(e => e.Description)));
    }


    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return BadRequest("Uživatel neexistuje.");
        }

        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (result.Succeeded)
        {
            return Ok("E-mail byl úspěšně potvrzen.");
        }

        return BadRequest("Nepodařilo se potvrdit e-mail.");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return Unauthorized(new { message = "Nesprávný e-mail nebo heslo." });
        }

        // Ověřte heslo
        var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            // Získání rolí uživatele
            var roles = await _context.AspNetUserRoles
                .Where(ur => ur.UserId == user.Id)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            // Získání názvů rolí na základě RoleId
            var roleNames = await _context.AspNetRoles
                .Where(r => roles.Contains(r.Id))
                .Select(r => r.Name)
                .ToListAsync();

            // Kontrola, zda je uživatel admin nebo má potvrzený e-mail
            if (roleNames.Contains("Admin") || await _userManager.IsEmailConfirmedAsync(user))
            {
                var token = Helper.CreateToken(user.UserName, roleNames, _configuration["AppSettings:Token"]); // Předání názvů rolí jako seznam

                // Uložení JWT tokenu do cookie
                HttpContext.Response.Cookies.Append("jwt", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true, // Pouze přes HTTPS
                    SameSite = SameSiteMode.None, // Zamezení CSRF útokům
                    Expires = DateTimeOffset.UtcNow.AddMinutes(30)
                });

                return Ok(new { message = "Přihlášení proběhlo úspěšně." });
            }
            else
            {
                return Unauthorized(new { message = "Uživatelův e-mail nebyl potvrzen." });
            }
        }

        return Unauthorized(new { message = "Nesprávný e-mail nebo heslo." });
    }






    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Response.Cookies.Append("jwt", "", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddMinutes(-1)
        });

        return Ok(new { message = "Odhlášení proběhlo úspěšně." });
    }

    [Authorize]
    [HttpGet("validate")]
    public IActionResult Validate()
    {
        
            return Ok(new { isAuthenticated = true });
        
        
    }


    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
        {
            return BadRequest("Uživatel s tímto e-mailem neexistuje nebo e-mail nebyl potvrzen.");
        }

        // Generování tokenu pro reset hesla
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        // Vytvoření odkazu pro reset hesla přímo
        var resetLink = $"http://localhost:3001/resetpassword?token={Uri.EscapeDataString(token)}&email={model.Email}";

        // Použití EmailService pro odeslání e-mailu
        _emailService.SendEmail(user.Email, "Reset hesla", $"Klikněte na tento odkaz pro obnovení hesla: <a href='{resetLink}'>Obnovit heslo</a>");



        return Ok(new { message = "Odkaz pro resetování hesla byl odeslán." });
    }


    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Najít uživatele podle e-mailu
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return BadRequest("Uživatel s tímto e-mailem neexistuje.");
        }

        var result = await _userManager.ResetPasswordAsync(user, model.ResetCode, model.NewPassword);

        if (result.Succeeded)
        {
            var token = Helper.CreateToken(user.UserName, new List<string>(), _configuration["AppSettings:Token"]);

            HttpContext.Response.Cookies.Append("jwt", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddMinutes(30)
            });

            return Ok(new { message = "Heslo bylo úspěšně resetováno." });
        }

        return BadRequest(result.Errors);
    }

}
