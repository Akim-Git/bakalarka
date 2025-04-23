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
using Humanizer;
using System.Numerics;

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
        [HttpPost("moderator-starts-game/{lobbyId}")]
        public async Task<IActionResult> ModeratorStartsGame(string lobbyId, [FromBody] StartGameDTO gameDTO)
        {

            bool hasTeam = false;

            

            //if (gameDTO.Teams != null && gameDTO.Teams.Any())
            //{
            //    await _quizHub.Clients.Group(lobbyId).SendAsync("ChangeButtons", "");
            //}

            // {teams: {User: "soul", AkimAdmin: "aloloa"}}

            var groupedTeams = gameDTO.Teams
            
                .GroupBy(pair => pair.Value)
                .ToDictionary(g => g.Key, // použij název týmu jako klíč
                g => g.Select(pair => pair.Key)  // z každého pairu (username, teamName) vezme jen username
                .ToList());

            foreach (var team in groupedTeams)
            {

                Console.WriteLine("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAa");
                var newTeam = new Team
                {
                    LobbyId = lobbyId,
                    TeamName = team.Key,
                    TeamScore = 0
                };

                _context.Teams.Add(newTeam);
                await _context.SaveChangesAsync(); 

                foreach (var username in team.Value)
                {
                    var member = new TeamMember
                    {
                        UserName = username,
                        TeamId = newTeam.Id
                    };

                    _context.TeamMembers.Add(member);
                }

                await _context.SaveChangesAsync();
            }




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

                if (gameDTO.Teams != null && gameDTO.Teams.Any())
                {
                    hasTeam = true;

                    var allPlayers = await _context.Players
                        .Where(p => p.LobbyId == lobbyIdInt)
                        .Select(p => p.UserName)
                        .ToListAsync();

                    var assignedPlayers = gameDTO.Teams.Keys;

                    var playersWithoutTeam = allPlayers.Except(assignedPlayers).ToList();

                    if (playersWithoutTeam.Any())
                    {
                        await _quizHub.Clients.Group(lobbyId).SendAsync("ReceiveMessage", $"Moderator vytvořil týmy. Všichni hráči musí být v týmech. Hráči bez týmu: {string.Join(", ", playersWithoutTeam)}");

                        return BadRequest($"Hráči bez týmu: {string.Join(", ", playersWithoutTeam)}");
                    }
                }


                var lobby = await _context.Lobbies.FindAsync(lobbyIdInt);

                lobby.IsActive = true;


                //var Players = await _context.Players.Where(p => p.LobbyId == lobbyIdInt).ToListAsync();

                //foreach (var gamers in Players)
                //{
                //    gamers.Score = 0;
                //}

                await _context.SaveChangesAsync();

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

                        //lobby.AcceptingAnswers = false;
                        //await _context.SaveChangesAsync();


                        //var players = await _context.Players.Where(p => p.LobbyId == lobbyIdInt).ToListAsync();

                        //foreach (var player in players)
                        //{

                        //    Console.WriteLine($"Před: {player.UserName}, DidAnswer: {player.DidAnswer}");
                        //    player.DidAnswer = false;
                        //    Console.WriteLine($"Po: {player.UserName}, DidAnswer: {player.DidAnswer}");
                        //}

                        //await _context.SaveChangesAsync();

                        int QuestionId = 0;

                        await _context.Players
                            .Where(p => p.LobbyId == lobbyIdInt)
                            .ExecuteUpdateAsync(setters => setters
                            .SetProperty(p => p.DidAnswer, false));


                        var currentQuestion = questionMulti[currentQuestionIndex];

                        QuestionId = currentQuestion.Id;

                        string message = $"The moderator has started the game! Quiz ID: {quizId}";

                        await _quizHub.Clients.Group(lobbyId).SendAsync("CheckAnswer", "");

                        await _quizHub.Clients.Group(lobbyId).SendAsync("ReceiveMessage", currentQuestion);

                        //lobby.AcceptingAnswers = true;
                        //await _context.SaveChangesAsync();

                        int timeForAnswerInSeconds = int.Parse(currentQuestion.TimeForAnswer);

                        // Čekání na čas definovaný v `TimeForAnswer`
                        await Task.Delay(timeForAnswerInSeconds * 1000); // Čekání v milisekundách

                        if (hasTeam)
                        {
                            await TeamAnswerPoints(lobbyIdInt, QuestionId);
                            //Console.WriteLine("************************************************************************************");
                        }

                       var usersAnswers = await UsersAnswers(QuestionId, hasTeam, lobbyIdInt);

                        await _quizHub.Clients.Group(lobbyId).SendAsync("ShowUserAnswers", usersAnswers);

                        await Task.Delay(10 * 1000); // Čekání v milisekundách


                        // Přechod na další otázku
                        currentQuestionIndex++;
                    }

                    //await _quizHub.Clients.Group(lobbyId).SendAsync("GameFinished", "All questions have been asked.");

                    var results = await Results(lobbyIdInt, hasTeam);


                    //Console.WriteLine("************************************************************************************");

                    Console.WriteLine("Odesílám výsledky:", results);

                    await _quizHub.Clients.Group(lobbyId).SendAsync("ReceiveResults", results);

                    var teams = await _context.Teams.Where(t => t.LobbyId == lobbyId).ToListAsync();

                    var players = await _context.Players.Where(p => p.LobbyId == lobbyIdInt).ToListAsync();

                    _context.Players.RemoveRange(players);

                    _context.Teams.RemoveRange(teams);

                    lobby.IsActive = false;

                    await _context.SaveChangesAsync();

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

        private async Task TeamAnswerPoints(int lobbyId, int questionId)
        {
            var teamAnswer = await _context.TeamAnswers
                .Where(t => t.QuestionId == questionId && t.Team.LobbyId == lobbyId.ToString())
                .Include(t => t.Team)
                .ToListAsync();

            var correctAnswer = await _context.Answers
                .Where(a => a.QuestionId == questionId && a.IsCorrect)
                .Select(a => a.Text)
                .FirstOrDefaultAsync();

            var groupedByTeam = teamAnswer
                .GroupBy(a => a.TeamId)
                .ToList();

            foreach (var group in groupedByTeam)
            {
                var team = group.First().Team;
                var answers = group;

                var mostCommonAnswer = answers
                    .GroupBy(a => a.Answer)
                    .Select(g => new { Answer = g.Key, Count = g.Count() })
                    .OrderByDescending(g => g.Count)
                    .First();

                Console.WriteLine($"Tým {team.TeamName} zvolil nejčastěji: {mostCommonAnswer.Answer} ({mostCommonAnswer.Count}x)");

                if (mostCommonAnswer.Answer == correctAnswer)
                {
                    team.TeamScore++;
                    //team.TeamAnswers.Answer = mostCommonAnswer.Answer;
                    Console.WriteLine($" Správně! Tým {team.TeamName} získává bod.");
                }
                else
                {
                    Console.WriteLine($" Špatně. Tým {team.TeamName} nezískal bod.");
                }

                var teamCommonAnswer = new TeamCommonAnswer
                {
                    TeamId = team.Id,
                    TeamName = team.TeamName,
                    LobbyId = lobbyId,
                    QuestionId = questionId,
                    Answer = mostCommonAnswer.Answer
                };

                await _context.TeamCommonAnswers.AddAsync(teamCommonAnswer);

                
                _context.TeamAnswers.RemoveRange(answers);

                //var answersToDelete = answers.Where(a => a.Answer != mostCommonAnswer.Answer).ToList();

                //_context.TeamAnswers.RemoveRange(answersToDelete);
            }
                

           
            await _context.SaveChangesAsync();
        }

        private async Task<List<ResultDTO>> Results(int lobbyId , bool hasTeam)
        {
            if (hasTeam)
            {
                var teamResults = await _context.Teams.Where(t => t.LobbyId == lobbyId.ToString()).Select(t => new ResultDTO
                {
                    Name = t.TeamName,
                    Score = t.TeamScore,
                    IsTeam = true
                })
                .OrderByDescending(r => r.Score)
                .ToListAsync();

                return teamResults;
            }
            else
            {
                var playerResults = await _context.Players.Where(p => p.LobbyId == lobbyId).Select(p => new ResultDTO
                {
                    Name = p.UserName,
                    Score = p.Score,
                    IsTeam = false
                })
                .OrderByDescending(r => r.Score)
                .ToListAsync();

                return playerResults;
            }

        }

        private async Task<List<UsersAnswersDTO>> UsersAnswers(int questionId, bool hasTeam, int lobbyId)
        {
            if (hasTeam)
            {
                var teamsAnswers = await _context.TeamCommonAnswers.Where(t => t.LobbyId == lobbyId && t.QuestionId == questionId).Select(t => new UsersAnswersDTO
                {
                    Name = t.TeamName,
                    Answer = t.Answer
                })
                .ToListAsync();

                return teamsAnswers;
            }
            else
            {
                var usersAnswers = await _context.Players.Where(p => p.LobbyId == lobbyId && p.QuestionId == questionId).Select(p => new UsersAnswersDTO
                {
                    Name = p.UserName,
                    Answer = p.Answer
                })
                .ToListAsync();

                return usersAnswers;
            }

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

            var teamMember = await _context.TeamMembers.FirstOrDefaultAsync(p => p.UserName == username);

            if (teamMember != null)
            {
                int teamId = teamMember.TeamId;
                await TeamAnswer(lobbyId, username, request.UserInput, request.QuestionId, teamId);

                Console.WriteLine("ttttttttttttttttttttttttttttttttttttttttttttttttttttttttt");

                return Ok("hráč v týmu odpovědel");
            }

            Console.WriteLine("llllllllllllllllllllllllllllllllllllllllllllllllllll");


            var correctAnswer = await _context.Answers
                .Where(a => a.QuestionId == request.QuestionId && a.IsCorrect == true)
                .FirstOrDefaultAsync();

            //var player = await _context.Players
            //        .Where(p => p.UserName == username && lobbyId == p.LobbyId)
            //        .FirstOrDefaultAsync();

            var player = await _context.Players.Where(p => p.UserName == username && lobbyId == p.LobbyId).FirstOrDefaultAsync();


            //var player = await _context.Players.FirstOrDefaultAsync(p => p.UserName == username);

            //var lobby = await _context.Lobbies.FirstOrDefaultAsync(l => l.Id == player.LobbyId);
            //if (lobby == null || lobby.AcceptingAnswers == false)
            //{
            //    return BadRequest("Ještě nelze odpovídat.");
            //}



            if (player.DidAnswer == true)
            {
                Console.WriteLine("už odpovídal");

                await _quizHub.Clients.Client(player.ConnectionId).SendAsync("CheckAnswer", "Nejde měnit odpověď");

                return Ok("nejde měnit odpověď");
            }

            if (request.UserInput == correctAnswer.Text)
            {
                
                player.Score++;

                player.DidAnswer = true;

                player.QuestionId = request.QuestionId;

                player.Answer = request.UserInput;

                player.IsAnswerCorrect = true;

                await _context.SaveChangesAsync();

                if(teamMember == null)
                {
                    Console.WriteLine("//////////////////////////////////////////////////////////////////////////////////////");

                    //Console.WriteLine(isInTeam);

                    await _quizHub.Clients.Client(player.ConnectionId).SendAsync("CheckAnswer", $"Správné ! Máte score : {player.Score}");


                }                

                return Ok($"Správné ! Máte score : {player.Score}");

            }

            player.DidAnswer = true;

            player.QuestionId = request.QuestionId;

            player.Answer = request.UserInput;

            await _context.SaveChangesAsync();

            await _quizHub.Clients.Client(player.ConnectionId).SendAsync("CheckAnswer", $"Odpověď nebyla správná. Máte score : {player.Score}");

            return Ok($"Odpověď nebyla správná. Máte score : {player.Score}");

        }

        private async Task TeamAnswer(int lobbyId, string username, string useranswer, int questionId, int teamId)
        {
            Console.WriteLine($"Data . jeho loddyId :{lobbyId},jeho username : {username}, jeho odpověď : {useranswer}, jeho questionId : {questionId}, jeho teamID {teamId}");



            var answered = await _context.TeamAnswers.FirstOrDefaultAsync(t => t.QuestionId == questionId && t.Username == username && t.TeamId == teamId);

            // najde všechny hráče z tohoto týmu ve stejné lobby
            var teamPlayersInLobby = await (
                from p in _context.Players
                join tm in _context.TeamMembers on p.UserName equals tm.UserName
                where p.LobbyId == lobbyId && tm.TeamId == teamId
                select p.ConnectionId
            ).ToListAsync();

            if (answered == null)
            {
                Console.WriteLine("11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111");

                var newAnswer = new TeamAnswer
                {
                    TeamId = teamId,
                    QuestionId = questionId,
                    Username = username,
                    Answer = useranswer

                };

                _context.TeamAnswers.Add(newAnswer);

                await _context.SaveChangesAsync();


                await _quizHub.Clients.Clients(teamPlayersInLobby).SendAsync("TeamMemberAnswer", new
                {
                    UserName = username,
                    Answer = useranswer
                });


            }
            else
            {
                answered.Answer = useranswer;
                await _context.SaveChangesAsync();

                


                await _quizHub.Clients.Clients(teamPlayersInLobby).SendAsync("TeamMemberAnswer", new
                {
                    UserName = username,
                    Answer = useranswer
                });
            }


        }

    }
}
