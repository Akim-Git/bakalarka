using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using back_end_2.Models;

namespace back_end_2.Classes
{
    public sealed class QuizHub : Hub
    {

        private readonly ApplicationDbContext _context;

        public QuizHub(ApplicationDbContext context)
        {
            _context = context;
        }
        //připojení k lobby
        public async Task JoinLobby(string lobbyId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, lobbyId);
            await Clients.Group(lobbyId).SendAsync("ReceiveMessage", $"{Context.ConnectionId} has joined lobby {lobbyId}");
        }

        // odpojení
        public async Task LeaveLobby(string lobbyId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, lobbyId);
            await Clients.Group(lobbyId).SendAsync("ReceiveMessage", $"{Context.ConnectionId} has left lobby {lobbyId}");
        }

        // zprávy pro lobby přenesený na GameController.cs  do endpointu  ModeratorStartsGame


        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public async Task SendResultsToLobby(string lobbyId, List<PlayersResultDTO> results)
        {
            await Clients.Group(lobbyId).SendAsync("ReceiveResults", results);
        }


        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
