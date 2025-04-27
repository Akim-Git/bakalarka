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

    [Route("api/Profile")]
    [ApiController]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet("get-profile-data")]

        public IActionResult ProfileData()
        {
            if (!HttpContext.Request.Cookies.TryGetValue("jwt", out var token))
            {
                return Unauthorized();
            }

            var username = Helper.GetUsernameFromToken(token);

            return Ok(new {username});
        }

        [Authorize]
        [HttpGet("get-quizzes")]
        public async Task<IActionResult> GetQuizzes()
        {
            if (!HttpContext.Request.Cookies.TryGetValue("jwt", out var token))
            {
                return Unauthorized();
            }

            var username = Helper.GetUsernameFromToken(token);

            var quizzes = await _context.Quizzes
                .Where(q => q.QuizOwner == username)
                .OrderBy(q => q.Title)
                .Select(q => new QuizOwnerDTO
                {
                    QuizName = q.Title,
                    QuizId = q.Id
                })
                .ToListAsync();

            return Ok(quizzes);
        }

    }

}
