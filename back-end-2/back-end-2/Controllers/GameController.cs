using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using back_end_2.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using back_end_2.Classes;
using Microsoft.AspNetCore.SignalR;
using back_end_2.Helpers;
using NuGet.Common;

namespace back_end_2.Controllers
{
    [Route("api/Game")]
    [ApiController]
    public class QuestionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<QuizHub> _quizHub;


        public QuestionController(ApplicationDbContext context, IHubContext<QuizHub> quizHub)
        {
            _context = context;
            _quizHub = quizHub;
        }

        // solo
        [Authorize]
        [HttpGet("{quizId}/questions-solo")]
        public async Task<ActionResult<IEnumerable<Question>>> GetQuestionsByQuizId(int quizId)
        {

            var questionCount = await _context.Questions
                .CountAsync(q => q.QuizId == quizId);

            if (questionCount > 10 || questionCount > 14)
            {
                return BadRequest("Kvíz má více než 10 otázek.");
            }


            var questions = await _context.Questions
                .Include(q => q.Answers)
                .Where(q => q.QuizId == quizId)
                .ToListAsync();

            if (questions == null || !questions.Any())
            {
                return NotFound("Žádné otázky nebyly nalezeny pro tento kvíz.");
            }

            return Ok(questions);
        }





        // multiplayer
        [Authorize]
        [HttpGet("moderator-starts-game/{lobbyId}")]
        public async Task<IActionResult> ModeratorStartsGame(string lobbyId)
        {

            if (!HttpContext.Request.Cookies.TryGetValue("jwt", out var token))
            {
                return Unauthorized();
            }

            var userRole = Helper.GetRolesFromToken(token);

            bool isModerator = userRole.Contains("Moderator");

            if (!isModerator) {
                return BadRequest("Nejste moderátor");
            }

            if (int.TryParse(lobbyId, out int lobbyIdInt))
            {

                var lobby = await _context.Lobbies.FindAsync(lobbyIdInt);

                if (lobby != null)
                {
                    int quizId = lobby.QuizId;

                    var questionMulti = await _context.Questions
                        .Where(q => q.QuizId.Equals(quizId))
                        .Include(q => q.Answers)
                        .Select(q => new QuestionDTO
                        {
                            Id = q.Id,
                            Text = q.Text,
                            QuestionType = q.QuestionType,
                            ImageDataQuestion = q.ImageDataQuestion,
                            TimeForAnswer = q.TimeForAnswer,
                            // anonymní objekt je pouze read-only, potřebujeme DTO
                            Answers = new List<string>()
                        })
                        .ToListAsync();

                    foreach (var question in questionMulti)
                    {
                        if (question.QuestionType != "contains")
                        {
                            question.Answers = await _context.Answers
                                .Where(a => a.QuestionId == question.Id)
                                .Select(a => a.Text)
                                .ToListAsync();
                        }
                    }

                    var currentQuestionIndex = 0;

                    int amountOfQuestions = questionMulti.Count;

                    while (currentQuestionIndex < amountOfQuestions)
                    {
                        var currentQuestion = questionMulti[currentQuestionIndex];

                        string message = $"The moderator has started the game! Quiz ID: {quizId}";

                        await _quizHub.Clients.Group(lobbyId).SendAsync("ReceiveMessage", currentQuestion);

                        int timeForAnswerInSeconds = int.Parse(currentQuestion.TimeForAnswer);

                        // Čekání na čas definovaný v `TimeForAnswer`
                        await Task.Delay(timeForAnswerInSeconds * 1000); // Čekání v milisekundách

                        // Přechod na další otázku
                        currentQuestionIndex++;
                    }



                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //var playerstodelete = await _context.players
                    //    .where(p => p.lobbyid == lobbyidint)
                    //    .tolistasync();

                    //_context.players.removerange(playerstodelete); 
                    //await _context.savechangesasync();





                    //await _quizHub.Clients.Group(lobbyId).SendAsync("GameFinished", "All questions have been asked.");

                    var results = await Results(lobbyIdInt);

                    Console.WriteLine("Odesílám výsledky:", results);

                    await _quizHub.Clients.Group(lobbyId).SendAsync("ReceiveResults", results);


                    return Ok("Game finished");
                }
                else
                {
                    return NotFound($"Lobby with Id {lobbyId} not found.");
                }
            }
            else
            {
                return BadRequest($"Invalid lobbyId format: {lobbyId}");
            }
        }


        private async Task<List<PlayersResultDTO>> Results(int lobbyid)
        {
            var players = await _context.Players
                .Where(p => p.LobbyId == lobbyid)
                .OrderByDescending(p => p.Score)
                .Select(p => new PlayersResultDTO
                {
                    Username = p.UserName,
                    Score = p.Score
                })
                .ToListAsync();



            return players;

        }



        [Authorize]
        [HttpPost("check-answer/{lobbyId}")]
        public async Task<IActionResult> CheckAnswer(int lobbyId, [FromBody] AnswerRequestDTO request)
        {
            //await _context.Players.ExecuteDeleteAsync();
            

            if (!HttpContext.Request.Cookies.TryGetValue("jwt", out var token))
            {
                return Unauthorized();
            }

            var username = Helper.GetUsernameFromToken(token); 
            // zatím nevím , jestli musím kontrolovat , jestli player je v lobby 

            //var isPlayerInLobby = await _context

            var correctAnswer = await _context.Answers
                .Where(a => a.QuestionId == request.QuestionId && a.IsCorrect == true)
                .FirstOrDefaultAsync();

            var playerScore = await _context.Players
                    .Where(p => p.UserName == username && lobbyId == p.LobbyId)
                    .FirstOrDefaultAsync();

            if (request.UserInput == correctAnswer.Text)
            {
                
                playerScore.Score++;

                await _context.SaveChangesAsync();

                return Ok($"Správné ! Máte score : {playerScore.Score}");

            }
            

            return Ok($"Odpověď nebyla správná. Máte score : {playerScore.Score}");

        }

    }
}
