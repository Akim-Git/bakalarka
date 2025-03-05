using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using back_end_2.Models;
using System.Linq;
using System.Threading.Tasks;
using back_end_2.Helpers;

namespace back_end_2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet("get-quiz-preview")]
        public IActionResult GetQuizzes([FromQuery] int skip = 0)
        {
            if (!HttpContext.Request.Cookies.TryGetValue("jwt", out var token))
            {
                return Unauthorized(); 
            }

            var roles = Helper.GetRolesFromToken(token);
            bool isAdmin = roles.Contains("Admin");

            var quizzes = _context.Quizzes
                .Skip(skip) 
                .Take(9)    
                .Select(q => new
                {
                    q.Id,
                    q.Title,
                    q.Description,
                    Image = q.ImageData != null ? Convert.ToBase64String(q.ImageData) : null,
                    IsAdmin = isAdmin
                })
                .ToList();

            GC.Collect(); 

            return Ok(new { quizzes, isAdmin });
        }



        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuiz(int id)
        {
            
            if (!HttpContext.Request.Cookies.TryGetValue("jwt", out var token))
            {
                return Unauthorized(); // Pokud token není k dispozici
            }

            
            var roles = Helper.GetRolesFromToken(token);

           
            if (!roles.Contains("Admin")) 
            {
                return Forbid(); 
            }

            
            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz == null)
            {
                return NotFound(); // Kvíz nebyl nalezen
            }

            // Smažeme kvíz a všechny jeho otázky a odpovědi
            _context.Quizzes.Remove(quiz);
            var questions = _context.Questions
                .Where(q => q.QuizId == id).ToList();

            foreach (var question in questions)
            {
                var answers = _context.Answers.Where(a => a.QuestionId == question.Id).ToList();
                _context.Answers.RemoveRange(answers);
                _context.Questions.Remove(question);
            }

            

            var lobbies = _context.Lobbies
                .Where(l => l.QuizId == id)
                .ToList();

            var players = _context.Players
                .Where(p => lobbies.Select(l => l.Id).Contains(p.LobbyId))
                .ToList();

            _context.Players.RemoveRange(players);

            _context.Lobbies.RemoveRange(lobbies);

            await _context.SaveChangesAsync(); // Uložení změn do databáze

            return NoContent(); // Vrátí 204 No Content při úspěšném smazání
        }



    }
}
