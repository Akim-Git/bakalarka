using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using back_end_2.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using NuGet.Common;

namespace back_end_2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuizController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public QuizController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Quiz>> CreateQuiz([FromForm] Quiz quiz)
        {
            if (!HttpContext.Request.Cookies.TryGetValue("jwt", out var token))
            {
                return Unauthorized(); // Pokud token není k dispozici
            }

            var username = Helper.GetUsernameFromToken(token); // Používáme metodu z Helper pro získání jména z tokenu

            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized(new { error = "Uživatel není ověřen." });
            }

            // Nastavení vlastníka kvízu
            quiz.QuizOwner = username;

            // Zkontrolujte, zda jsou kvíz a otázky platné
            if (quiz == null || quiz.Questions == null || !quiz.Questions.Any())
            {
                return BadRequest("Kvíz a otázky jsou povinné.");
            }

            // Zkontrolujte, zda kvíz již neexistuje
            var alreadyExists = await _context.Quizzes.FirstOrDefaultAsync(q => q.Title == quiz.Title);
            if (alreadyExists != null)
            {
                return BadRequest(new { error = "Kvíz s tímto názvem již existuje." });
            }

            // Zpracování obrázku pro kvíz
            var imageData = Request.Form.Files["imageData"];
            if (imageData != null)
            {
                if (!Helper.IsValidImageFormat(imageData))
                {
                    return BadRequest(new { error = "Nepodporovaný formát souboru. Prosím, zvolte obrázek ve formátu JPG, JPEG nebo PNG." });
                }

                quiz.ImageData = await Helper.ConvertToByteArrayAsync(imageData);
            }

            // Zpracování obrázků pro otázky
            for (int i = 0; i < quiz.Questions.Count; i++)
            {
                var question = quiz.Questions.ElementAt(i);
                var imageDataQuestion = Request.Form.Files[$"questions[{i}].imageData"];
                if (imageDataQuestion != null)
                {
                    if (!Helper.IsValidImageFormat(imageDataQuestion))
                    {
                        return BadRequest(new { error = $"Nepodporovaný formát souboru pro otázku {i + 1}. Prosím, zvolte obrázek ve formátu JPG, JPEG nebo PNG." });
                    }

                    question.ImageDataQuestion = await Helper.ConvertToByteArrayAsync(imageDataQuestion);
                }
            }

            // Přidání kvízu do databáze
            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();

            return Ok(quiz);
        }
    }
}
