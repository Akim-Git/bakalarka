using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using back_end_2.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using back_end_2.Helpers;
using System.Configuration;
using Newtonsoft.Json;
using NuGet.Common;
using back_end_2.Classes;
using Microsoft.AspNetCore.SignalR;
using static System.Runtime.InteropServices.JavaScript.JSType;

[Route("api/[controller]")]
[ApiController]
public class LobbyController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    
    private readonly IHubContext<QuizHub> _quizHubContext;

    public LobbyController(ApplicationDbContext context, IConfiguration configuration, IHubContext<QuizHub> quizHubContext)
    {
        
        _context = context; // Initialize _context
        _configuration = configuration;
        _quizHubContext = quizHubContext;
    }




    [Authorize]
    [HttpPost("create-lobby")]
    public async Task<IActionResult> CreateLobby([FromBody] LobbyDTO lobbyRequest)
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

        var quiz = await _context.Quizzes.FindAsync(lobbyRequest.QuizId);
        if (quiz == null)
        {

            Console.WriteLine(" Chyba: Neexistující quizId v databázi");
            return BadRequest("Musí být lobby data");
        }

        var existingLobby = await _context.Lobbies.FirstOrDefaultAsync(l => l.Name.ToLower() == lobbyRequest.Name.ToLower());

        if (existingLobby != null)
        {
            return Conflict("Lobby with this name already exists.");
        }


        // Převod objektu na čitelný JSON string a výpis do konzole
        string requestBody = JsonConvert.SerializeObject(lobbyRequest, Formatting.Indented);
        Console.WriteLine(requestBody);

        Console.WriteLine($"username je : {username}");

        //    // hash

        if (!string.IsNullOrEmpty(lobbyRequest.LobbyPassword))
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
                ModerConnectionId = ""
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
            LobbyPassword = string.IsNullOrEmpty(lobbyRequest.LobbyPassword) ? "" : lobbyRequest.LobbyPassword, // Nastavení výchozí hodnoty
            ModerConnectionId = ""
        };

        _context.Lobbies.Add(newLobby);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Data úspěšně obdržena", receivedData = lobbyRequest });
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

        var lobbies = await _context.Lobbies
            .Select(l => new
            {
                l.Id,
                l.Name,
                HasPassword = !string.IsNullOrEmpty(l.LobbyPassword), 
                IsAdmin = isAdmin
            })
            .ToListAsync();

        return Ok(new { lobbies, isAdmin });
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

        //var players = _context.Players
        //    .Where(p => p.Id == lobbyId)
        //    .Select(p => new PlayersDTO)


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

            lobby.IsActive = true;

            await _context.SaveChangesAsync(); 

            return Ok(new { sendModerator = true});

            
        }

        //aby vratilo isModerator true i když moderator restartoval stránku , ale v tokenu je moderator

        if (isModerator && lobby.LobbyOwner == username)
        {
            if (lobby.IsActive == false)
            {
                lobby.IsActive = true;

                await _context.SaveChangesAsync();
            }

            return Ok(new { sendModerator = true });
        }

        return Ok(new { sendModerator = false});
    }

    //[Authorize]
    //[HttpPost("log-in-lobby/{lobbyId}")]
    //public async Task<IActionResult> LogInLobby(int lobbyId)
    //{


    //    if (!HttpContext.Request.Cookies.TryGetValue("jwt", out var token))
    //    {
    //        return Unauthorized();
    //    }

    //    var username = Helper.GetUsernameFromToken(token);

    //    var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);

    //    var existing = await _context.Players.FirstOrDefaultAsync(p => p.UserId == user.Id && p.LobbyId != lobbyId);

    //    var reconnection = await _context.Players.FirstOrDefaultAsync(p => p.UserId == user.Id && p.LobbyId == lobbyId);

    //    if (existing != null)
    //    {
    //        _context.Players.Remove(existing);

    //        var updatedPlayer = new Player
    //        {
    //            UserId = user.Id,
    //            UserName = username,
    //            Score = 0,
    //            LobbyId = lobbyId
    //        };

    //        _context.Players.Add(updatedPlayer);


    //        await _context.SaveChangesAsync();

    //        return Ok("Úspěšně přihlášen do jiného lobby.");
    //    }

    //    if (reconnection != null)
    //    {
    //        return Ok("Úspěšné přípojení zpět");
    //    }

    //    var newPlayer = new Player
    //    {
    //        UserId = user.Id,
    //        UserName = username, 
    //        Score = 0, 
    //        LobbyId = lobbyId 
    //    };

    //    _context.Players.Add(newPlayer);


    //    await _context.SaveChangesAsync();

    //    return Ok("Úspěšně přihlášen do lobby.");
    //}

    [Authorize]
    [HttpGet("has-password/{lobbyId}")]
    public async Task<IActionResult> HasPassword(int lobbyId)
    {
        var lobby = await _context.Lobbies.FirstOrDefaultAsync(l => l.Id == lobbyId);

        if (lobby == null)
        {
            return NotFound(new { error = "Lobby nenalezena." });
        }

        return Ok(new { hasPassword = !string.IsNullOrEmpty(lobby.LobbyPassword) });
    }


    [HttpDelete("delete")]
    public async Task<IActionResult> Delete()
    {

        


        //var allPlayers = await _context.Players.ToListAsync();

        //if (!allPlayers.Any())
        //{
        //    return NotFound(" Žádní hráči k odstranění.");
        //}

        //_context.Players.RemoveRange(allPlayers);
        //await _context.SaveChangesAsync();

        

        var allBlock = await _context.BlockList.ToListAsync();
        Console.WriteLine($"Počet blokovaných hráčů: {allBlock.Count}");

        if (!allBlock.Any())
        {
            return NotFound(" Žádní hráči k odstranění.");
        }

        Console.WriteLine("Odstraňuji všechny záznamy z BlockList...");
        _context.BlockList.RemoveRange(allBlock);
        await _context.SaveChangesAsync();
        Console.WriteLine("Všechny záznamy byly úspěšně odstraněny.");

        return Ok("hura");


    }

    [Authorize]
    [HttpPost("block-player/{lobbyId}")]
    public async Task<IActionResult> Block(int lobbyId, [FromBody] BlockDTO request)
    {
        if (!HttpContext.Request.Cookies.TryGetValue("jwt", out var token))
        {
            return Unauthorized();
        }

      
        var username = Helper.GetUsernameFromToken(token);

        if (!await IsUserModeratorAsync(username, lobbyId))
        {
            return BadRequest("Nejste moderátor nebo nemáte oprávnění.");
        }



        var lobby = await _context.Lobbies.FirstOrDefaultAsync(l => l.Id == lobbyId);

        

        if (string.IsNullOrEmpty(request.Username))
        {
            return BadRequest(new { message = "Neplatné uživatelské jméno." });
        }

        var player = await _context.Players.FirstOrDefaultAsync(p => p.UserName == request.Username);
        if (player == null)
        {
            return NotFound(new { message = "Uživatel nebyl nalezen." });
        }

        
        var blockEntry = new BlockList
        {
            UserId = player.UserId,  
            UserName = request.Username,
            LobbyId = lobbyId,
            ConnectionId = player.ConnectionId
        };

        _context.BlockList.Add(blockEntry);
        await _context.SaveChangesAsync();

        await _quizHubContext.Clients.Client(player.ConnectionId)
    .SendAsync("TriggerPageReload", new { message = $"Hráč {request.Username} byl úspěšně zablokován." });

        _context.Players.RemoveRange(player);
        await _context.SaveChangesAsync();

        string moderConnection = lobby.ModerConnectionId;

        var updatedPlayers = await _context.Players
                       .Where(p => p.LobbyId == lobbyId)
                       .Select(p => new PlayersDTO
                       {
                           Username = p.UserName,
                           Score = p.Score
                       })
                       .ToListAsync();

        await _quizHubContext.Clients.Client(moderConnection).SendAsync("ReceivePlayersList", updatedPlayers);

       
        

        var block = await _context.BlockList.Where(b => b.LobbyId == lobbyId).Select(b => new { b.UserName }).ToListAsync();

        if (block != null)
        {
            await _quizHubContext.Clients.Client(moderConnection).SendAsync("ReceiveBlockedPlayersList", block);
        }


        return Ok(new { message = $"Hráč {request.Username} byl úspěšně zablokován v lobby {lobbyId}." });
    }

    [Authorize]
    [HttpPost("unblock/{lobbyId}")]
    public async Task<IActionResult> Unblock(int lobbyId, [FromBody] BlockDTO request)
    {
        if (!HttpContext.Request.Cookies.TryGetValue("jwt", out var token))
        {
            return Unauthorized();
        }
        
        var username = Helper.GetUsernameFromToken(token);

        if (!await IsUserModeratorAsync(username, lobbyId))
        {
            return BadRequest("Nejste moderátor nebo nemáte oprávnění.");
        }

        var lobby = await _context.Lobbies.FirstOrDefaultAsync(l => l.Id == lobbyId);     


        if (string.IsNullOrEmpty(request.Username))
        {
            return BadRequest(new { message = "Neplatné uživatelské jméno." });
        }        

        var blocked = await _context.BlockList.FirstOrDefaultAsync( b => b.LobbyId == lobbyId && b.UserName == request.Username);


        string moderConnection = lobby.ModerConnectionId;        

        _context.BlockList.RemoveRange(blocked);
        await _context.SaveChangesAsync();

        var block = await _context.BlockList.Where(b => b.LobbyId == lobbyId).Select(b => new { b.UserName }).ToListAsync();
        
        await _quizHubContext.Clients.Client(moderConnection).SendAsync("ReceiveBlockedPlayersList", block);      


        return Ok(new { message = $"Hráč {request.Username} byl úspěšně zablokován v lobby {lobbyId}." });
    }

    [Authorize]
    [HttpDelete("get-out/{lobbyId}")]
    public async Task<IActionResult> GetOut(int lobbyId, [FromBody] BlockDTO request)
    {
        

        if (!HttpContext.Request.Cookies.TryGetValue("jwt", out var token))
        {
            return Unauthorized();
        }

        var username = Helper.GetUsernameFromToken(token);

        if (!await IsUserModeratorAsync(username, lobbyId))
        {
            return BadRequest("Nejste moderátor nebo nemáte oprávnění.");
        }

        // Váš logický kód
        var lobby = await _context.Lobbies.FirstOrDefaultAsync(l => l.Id == lobbyId);

        if (lobby == null)
        {
            return NotFound(new { message = "Lobby nebyla nalezena." });
        }

        var player = await _context.Players.FirstOrDefaultAsync(p => p.UserName == request.Username);
        if (player == null)
        {
            return NotFound(new { message = "Hráč nebyl nalezen." });
        }

        string moderConnection = lobby.ModerConnectionId;

        string lobbyIdStr = lobby.ToString();

        await _quizHubContext.Groups.RemoveFromGroupAsync(player.ConnectionId, lobbyIdStr);
        await _quizHubContext.Clients.Client(player.ConnectionId).SendAsync("GetOutMessage", $" has left lobby {lobbyId}");

        _context.Players.RemoveRange(player);
        await _context.SaveChangesAsync();

        var updatedPlayers = await _context.Players
                       .Where(p => p.LobbyId == lobbyId)
                       .Select(p => new PlayersDTO
                       {
                           Username = p.UserName,
                           Score = p.Score
                       })
                       .ToListAsync();

        await _quizHubContext.Clients.Client(moderConnection).SendAsync("ReceivePlayersList", updatedPlayers);

        Console.WriteLine("hello world");


        return Ok(new { message = $"Hráč {request.Username} byl úspěšně vychozen z lobby {lobbyId}." });

        
    }

    [HttpGet("does-lobby-exists")]
    public async Task<IActionResult> DoesLobbyExist([FromQuery] int lobbyId)
    {
        var exists = await _context.Lobbies.AnyAsync(l => l.Id == lobbyId);
        if (!exists)
            return NotFound(); 

        return Ok(); 
    }


    private async Task<bool> IsUserModeratorAsync(string username, int lobbyId)
    {
        var lobby = await _context.Lobbies.FirstOrDefaultAsync(l => l.Id == lobbyId);
        if (lobby == null)
        {
            return false;  
        }

        
        var userRole = Helper.GetRolesFromToken(HttpContext.Request.Cookies["jwt"]);
        bool isModerator = userRole.Contains("Moderator");

        if (!isModerator || lobby.LobbyOwner != username)
        {
            return false;  
        }

        return true;
    }


}