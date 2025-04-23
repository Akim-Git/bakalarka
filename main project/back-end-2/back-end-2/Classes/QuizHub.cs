using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using back_end_2.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using back_end_2.Helpers;
using System.Globalization;
using System.Linq;

namespace back_end_2.Classes
{
    public sealed class QuizHub : Hub
    {

        private readonly ApplicationDbContext _context;
        private readonly ILogger<QuizHub> _logger;

        public QuizHub(ApplicationDbContext context, ILogger<QuizHub> logger)
        {
            _context = context;
            _logger = logger;
        }
        //připojení k lobby
        //public async Task JoinLobby(string lobbyId)
        //{
        //    Console.WriteLine("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

        //    await Groups.AddToGroupAsync(Context.ConnectionId, lobbyId);
        //    await Clients.Group(lobbyId).SendAsync("ReceiveMessage", $"{Context.ConnectionId} has joined lobby {lobbyId}");
        //}

        public async Task<bool> JoinLobby(string lobbyId, string? enteredPassword, bool isModerator)
        {

            Console.WriteLine("Spůstil se JoinLobby----------------------------------------------");

            var httpContext = Context.GetHttpContext(); // Získání HTTP kontextu
            if (httpContext == null)
            {
                Console.WriteLine("--------------------------------------- Nelze získat HTTP kontext.");
                return false;
            }

            string token = httpContext.Request.Cookies["jwt"]; // Název cookie
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("--------------------------------------------- Cookie `jwt` nebylo nalezeno.");
                return false;
            }

            Console.WriteLine($" JWT Cookie: {token}");

            Console.WriteLine($"User with ConnectionId {Context.ConnectionId} joined lobby {lobbyId} as Moderator: {isModerator}");
            // Výpis lobbyId a hesla přijatého z frontendu
            Console.WriteLine($"Received lobbyId: {lobbyId}");
            if (!string.IsNullOrEmpty(enteredPassword))
            {
                Console.WriteLine($"Received enteredPassword: {enteredPassword}");
            }

            int lobbyIdInt = int.Parse( lobbyId );

            var lobby = await _context.Lobbies.FirstOrDefaultAsync(l => l.Id == lobbyIdInt);

            if (lobby == null)
            {
                // lobby nebyla nalezena
                await Clients.Caller.SendAsync("ReceiveMessage", "Lobby nebyla nalezena.");
                return false;

            }
            // string moderConnection = lobby.ModerConnectionId;

            var username = Helper.GetUsernameFromToken(token);

            bool exist = await IsPlayerInLobby(username);

            var block = await _context.BlockList.FirstOrDefaultAsync(b => b.LobbyId == lobbyIdInt && b.UserName == username);

            

            if(block != null)
            {
                int count1 = await _context.BlockList.CountAsync(b => b.LobbyId == lobbyIdInt);

                Console.WriteLine($"Počet blokovaných hráčů v lobby: {count1}");


                var withoutBlockedPlater = await _context.Players.FirstOrDefaultAsync(p => p.UserName == block.UserName);


                _context.Players.RemoveRange(withoutBlockedPlater);
                await _context.SaveChangesAsync();

                var updatedPlayers = await _context.Players
                       .Where(p => p.LobbyId == lobbyIdInt)
                       .Select(p => new PlayersDTO
                       {
                           Username = p.UserName,
                           Score = p.Score
                       })
                       .ToListAsync();




                await Clients.Client(lobby.ModerConnectionId).SendAsync("ReceivePlayersList", updatedPlayers);

                await Clients.Caller.SendAsync("ReceiveMessage", "Moderator lobby Vás zablokoval");
                return false;
            }

            if (!string.IsNullOrEmpty(lobby.LobbyPassword))
            {

               
                
                if (enteredPassword == null)
                {

                    await Clients.Caller.SendAsync("ReceiveMessage", "Tato lobby vyžaduje heslo.");
                    return false;
                }

                var passwordHasher = new PasswordHasher<Lobby>();
                var verificationResult = passwordHasher.VerifyHashedPassword(null, lobby.LobbyPassword, enteredPassword);

                if (verificationResult == PasswordVerificationResult.Success)
                {
                    Console.WriteLine("Heslo je správné.");

                   // var usernamePass = Helper.GetUsernameFromToken(token);



                    var userPass = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);
                


                    if (!exist)
                    {

                        

                            var updatedPlayerPass = new Player
                            {
                                UserId = userPass.Id,
                                UserName = username,
                                Score = 0,
                                LobbyId = lobbyIdInt,
                                ConnectionId = Context.ConnectionId,
                                Answer = " "
                            };

                            _context.Players.Add(updatedPlayerPass);

                            await _context.SaveChangesAsync();
                        
                        
                        
                    }

                    if (exist)
                    {
                        await HandlePlayerLobbyChange(username, lobbyIdInt);
                    }


                    var playersPasswordRequired = await _context.Players
                        .Where(p => p.LobbyId == lobbyIdInt)
                        .Select(p => new PlayersDTO
                        {
                            Username = p.UserName,
                            Score = p.Score
                        })
                        .ToListAsync();

                    //int count1 = playersPasswordRequired.Count;

                    //Console.WriteLine($"počet hráču : {count1}-----------------------------------------------------------------------------");

                    await Groups.AddToGroupAsync(Context.ConnectionId, lobbyId);
                    await Clients.Group(lobbyId).SendAsync("ReceiveMessage", $"{Context.ConnectionId} has joined lobby {lobbyId}");

                    if (lobby.IsActive == true && isModerator == true)
                    {
                        lobby.ModerConnectionId = Context.ConnectionId;


                        await _context.SaveChangesAsync();

                        await SendBlockedPlayersList(lobbyIdInt, lobby.ModerConnectionId);

                        await Clients.Client(lobby.ModerConnectionId).SendAsync("ReceivePlayersList", playersPasswordRequired);
                        Console.WriteLine("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");


                    }

                    if (!string.IsNullOrEmpty(lobby.ModerConnectionId))
                    {
                        await Clients.Client(lobby.ModerConnectionId).SendAsync("ReceivePlayersList", playersPasswordRequired);
                    }

                    return true;
                }
                else
                {
                    Console.WriteLine("Password incorrect.");
                    await Clients.Caller.SendAsync("ReceiveMessage", "Heslo nebylo správné.");
                    return false;
                }

            }

            Console.WriteLine("bez hesla------------------------------------------------------------------------------------");

            //----------bez hesla--------------------

            //var username = Helper.GetUsernameFromToken(token);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);

            

            if (!exist)
            {

                var newPlayer = new Player
                {
                    UserId = user.Id,
                    UserName = username,
                    Score = 0,
                    LobbyId = lobbyIdInt,
                    ConnectionId = Context.ConnectionId,
                    Answer = " "
                };

                _context.Players.Add(newPlayer);

                await _context.SaveChangesAsync();
            }


            if (exist)
            {
                await HandlePlayerLobbyChange(username, lobbyIdInt);
            }


            

            
            // lobby bez hesla
            await Groups.AddToGroupAsync(Context.ConnectionId, lobbyId);
            await Clients.Group(lobbyId).SendAsync("ReceiveMessage", $"{Context.ConnectionId} has joined lobby {lobbyId}");

            if (lobby.IsActive == true && isModerator == true)
            {

                

                lobby.ModerConnectionId = Context.ConnectionId;


                await _context.SaveChangesAsync();

                // oprava chyby, kdy moderatora zapisovalo dvákrat
                var fakeModer = await _context.Players.Where(p => p.UserName == username && p.ConnectionId != lobby.ModerConnectionId).ToListAsync();

                if (fakeModer.Any()) // Ověříme, zda existují hráči k odstranění
                {
                    _context.Players.RemoveRange(fakeModer);
                    await _context.SaveChangesAsync();

                    Console.WriteLine($"Smazáno {fakeModer.Count} hráčů s uživatelským jménem {username}, kteří neměli odpovídající ConnectionId.");
                }
                else
                {
                    Console.WriteLine("Nebyli nalezeni žádní hráči k odstranění.");
                }

                var playersWithoutPasswordClean = await _context.Players
                        .Where(p => p.LobbyId == lobbyIdInt)
                        .Select(p => new PlayersDTO
                        {
                            Username = p.UserName,
                            Score = p.Score
                        })
                        .ToListAsync();

                await Clients.Client(lobby.ModerConnectionId).SendAsync("ReceivePlayersList", playersWithoutPasswordClean);

                await SendBlockedPlayersList(lobbyIdInt, lobby.ModerConnectionId);

            }

            var playersWithoutPassword = await _context.Players
                        .Where(p => p.LobbyId == lobbyIdInt)
                        .Select(p => new PlayersDTO
                        {
                            Username = p.UserName,
                            Score = p.Score
                        })
                        .ToListAsync();

            int count = playersWithoutPassword.Count;

            Console.WriteLine($"počet hráču bez hesla : {count}-----------------------------------------------------------------------------");


            await Clients.Client(lobby.ModerConnectionId).SendAsync("ReceivePlayersList", playersWithoutPassword);


            // Potvrzení úspěšného připojení
            return true;
        }



        // odpojení
        public async Task LeaveLobby(string lobbyId)
        {

            int lobbyIdInt = int.Parse(lobbyId);

            var lobby = await _context.Lobbies.FirstOrDefaultAsync(l => l.Id == lobbyIdInt);

            

            

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, lobbyId);
            await Clients.Group(lobbyId).SendAsync("ReceiveMessage", $"{Context.ConnectionId} has left lobby {lobbyId}");

            if (lobby.ModerConnectionId == Context.ConnectionId)
            {
                
                lobby.IsActive = false;
                lobby.ModerConnectionId = "";

                await _context.SaveChangesAsync();
            }

            


            var delete = await _context.Players.FirstOrDefaultAsync(p => p.ConnectionId == Context.ConnectionId);

            _context.Players.Remove(delete);

            await _context.SaveChangesAsync();

            var players = await _context.Players
                        .Where(p => p.LobbyId == lobbyIdInt)
                        .Select(p => new PlayersDTO
                        {
                            Username = p.UserName,
                            Score = p.Score
                        })
                        .ToListAsync();

            await Clients.Client(lobby.ModerConnectionId).SendAsync("ReceivePlayersList", players);


        }

        // zprávy pro lobby přenesený na GameController.cs  do endpointu  ModeratorStartsGame

        


        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        //public async Task SendResultsToLobby(string lobbyId, List<PlayersResultDTO> results)
        //{
        //    await Clients.Group(lobbyId).SendAsync("ReceiveResults", results);
        //}


        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }

        public async Task<bool> IsPlayerInLobby(string username)
        {
            return await _context.Players
                .AnyAsync(p => p.UserName == username);
        }

        private async Task SendBlockedPlayersList(int lobbyIdInt, string moderConnectionId)
        {
            var blockList = await _context.BlockList
                .Where(b => b.LobbyId == lobbyIdInt)
                .Select(b => new { b.UserName })
                .ToListAsync();

            if (blockList.Any())
            {
                await Clients.Client(moderConnectionId).SendAsync("ReceiveBlockedPlayersList", blockList);
            }
        }




        private async Task HandlePlayerLobbyChange(string username, int newLobbyId)
        {
            var player = await _context.Players.FirstOrDefaultAsync(p => p.UserName == username);
            if (player == null)
            {
                Console.WriteLine("Hráč nebyl nalezen.");
                return;
            }

            

            // Načtení předchozího lobby
            var previousLobby = await _context.Lobbies.FirstOrDefaultAsync(l => l.Id == player.LobbyId);
            if (previousLobby == null)
            {
                Console.WriteLine("Předchozí lobby neexistuje.");
                return;
            }

            // Aktualizace hráčových údajů
            player.ConnectionId = Context.ConnectionId;
            player.LobbyId = newLobbyId;

            await _context.SaveChangesAsync();

            // Načtení seznamu hráčů v předchozím lobby
            var playersInPreviousLobby = await _context.Players
                .Where(p => p.LobbyId == previousLobby.Id)
                .Select(p => new PlayersDTO
                {
                    Username = p.UserName,
                    Score = p.Score
                })
                .ToListAsync();

            // Odeslání aktualizovaného seznamu hráčů moderátorovi předchozího lobby
            if (!string.IsNullOrEmpty(previousLobby.ModerConnectionId))
            {
                await Clients.Client(previousLobby.ModerConnectionId)
                    .SendAsync("ReceivePlayersList", playersInPreviousLobby);
            }

            Console.WriteLine($"Změna: {playersInPreviousLobby.Count} hráčů v lobby.");
            Console.WriteLine("Exist --------------------------------------------------------------");
        }


    }
}
