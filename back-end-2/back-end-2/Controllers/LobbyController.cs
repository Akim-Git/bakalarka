using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using back_end_2.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using back_end_2.Helpers;
using System.Configuration;

[Route("api/[controller]")]
[ApiController]
public class LobbyController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public LobbyController(ApplicationDbContext context, IConfiguration configuration)
    {
        
        _context = context; // Initialize _context
        _configuration = configuration;
    }

    [Authorize]
    [HttpPost("create-lobby")]
    public async Task<IActionResult> CreateLobby([FromBody] Lobby lobbyRequest)
    {
        if (!HttpContext.Request.Cookies.TryGetValue("jwt", out var token))
        {
            return Unauthorized(); 
        }

        var username = Helper.GetUsernameFromToken(token);

        if (string.IsNullOrEmpty(username))
        {
            return Unauthorized(new { error = "Uživatel není ověřen." });
        }

        if (lobbyRequest == null)
        {
            return BadRequest("Lobby data is required.");
        }

        var quiz = await _context.Quizzes.FindAsync(lobbyRequest.QuizId);
        if (quiz == null)
        {
            return BadRequest("Musí být lobby data");
        }

        var existingLobby = await _context.Lobbies
                                      .FirstOrDefaultAsync(l => l.Name.ToLower() == lobbyRequest.Name.ToLower());

        if (existingLobby != null)
        {
            return Conflict("Lobby with this name already exists.");
        }

        // hash

        if(lobbyRequest.LobbyPassword != null)
        {
            var passwordHasher = new PasswordHasher<Lobby>();
            string hashedPassword = passwordHasher.HashPassword(null, lobbyRequest.LobbyPassword);

            var newLobbyWithPassword = new Lobby
            {
                Name = lobbyRequest.Name,
                QuizId = lobbyRequest.QuizId,
                LobbyOwner = username,
                IsActive = false,
                LobbyPassword = hashedPassword,
            };

            _context.Lobbies.Add(newLobbyWithPassword);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(CreateLobby), new { id = newLobbyWithPassword.Id }, newLobbyWithPassword);
        }


        var newLobby = new Lobby
        {
            Name = lobbyRequest.Name,
            QuizId = lobbyRequest.QuizId,
            LobbyOwner = username,
            IsActive = false,
        };

        _context.Lobbies.Add(newLobby);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(CreateLobby), new { id = newLobby.Id }, newLobby);
    }

    [Authorize]
    [HttpGet("get-lobbies")]

    public async Task<IActionResult> GetLobbies()
    {
        if (!HttpContext.Request.Cookies.TryGetValue("jwt", out var token))
        {
            return Unauthorized();
        }

        var roles = Helper.GetRolesFromToken(token);
        bool isAdmin = roles.Contains("Admin");

        var lobbies = _context.Lobbies
            .Select(l => new 
            { 
                l.Id, 
                l.Name,
                IsAdmin = isAdmin
            })
            .ToList();

        GC.Collect();

        return Ok(new {lobbies, isAdmin });
    }

    [Authorize]
    [HttpGet("is-moderator/{lobbyId}")]
    public async Task<IActionResult> IsModerator(int lobbyId)
    {
        if (!HttpContext.Request.Cookies.TryGetValue("jwt", out var token))
        {
            return Unauthorized();
        }

        var username = Helper.GetUsernameFromToken(token);

        var userRole = Helper.GetRolesFromToken(token);

        var lobby = _context.Lobbies.FirstOrDefault(l => l.Id == lobbyId);

        bool isModerator = userRole.Contains("Moderator");


        if (lobby == null)
        {
            return NotFound("Lobby not found.");
        }

        if (lobby.LobbyOwner == username && !isModerator)
        {
            var roleName = new List<string> { "Moderator" };            

            var newToken = Helper.CreateToken(username, roleName, _configuration["AppSettings:Token"]);

            HttpContext.Response.Cookies.Append("jwt", newToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Pouze přes HTTPS
                SameSite = SameSiteMode.None, // Zamezení CSRF útokům
                Expires = DateTimeOffset.UtcNow.AddMinutes(30)
            });

            return Ok(new { sendModerator = true});

            
        }

        //aby vratilo isModerator true is když moderator restartoval stránku , ale v tokenu je moderator

        if (isModerator && lobby.LobbyOwner == username)
        {
            return Ok(new { sendModerator = true });
        }

        return Ok(new { sendModerator = false});
    }

    [Authorize]
    [HttpPost("log-in-lobby/{lobbyId}")]
    public async Task<IActionResult> LogInLobby(int lobbyId)
    {


        if (!HttpContext.Request.Cookies.TryGetValue("jwt", out var token))
        {
            return Unauthorized();
        }

        var username = Helper.GetUsernameFromToken(token);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);

        var existing = await _context.Players.FirstOrDefaultAsync(p => p.UserId == user.Id && p.LobbyId != lobbyId);

        var reconnection = await _context.Players.FirstOrDefaultAsync(p => p.UserId == user.Id && p.LobbyId == lobbyId);

        if (existing != null)
        {
            _context.Players.Remove(existing);

            var updatedPlayer = new Player
            {
                UserId = user.Id,
                UserName = username,
                Score = 0,
                LobbyId = lobbyId
            };

            _context.Players.Add(updatedPlayer);


            await _context.SaveChangesAsync();

            return Ok("Úspěšně přihlášen do jiného lobby.");
        }

        if (reconnection != null)
        {
            return Ok("Úspěšné přípojení zpět");
        }

        var newPlayer = new Player
        {
            UserId = user.Id,
            UserName = username, 
            Score = 0, 
            LobbyId = lobbyId 
        };

        _context.Players.Add(newPlayer);


        await _context.SaveChangesAsync();

        return Ok("Úspěšně přihlášen do lobby.");
    }

}