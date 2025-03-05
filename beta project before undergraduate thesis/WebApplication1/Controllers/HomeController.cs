using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Threading.Tasks;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        //_shelterContext obsahuje instanci tøídy ShelterContext
        private readonly ILogger<HomeController> _logger;
        private readonly ShelterContext _shelterContext;

        public HomeController(ILogger<HomeController> logger, ShelterContext shelterContext)
        {
            _logger = logger;
            _shelterContext = shelterContext;
        }

        public async Task<IActionResult> Index()
        {
            var shelters = await _shelterContext.Shelters.Include(s => s.Address).ToListAsync();
            return View(shelters);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        public IActionResult New()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Shelter shelter)
        {
            if (ModelState.IsValid)
            {
                _shelterContext.Shelters.Add(shelter);
                await _shelterContext.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(shelter);
        }

        [HttpGet]
        public async Task<IActionResult> Search(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                // Pokud jméno není zadáno, zobrazíme všechny psy
                var allDogs = await _shelterContext.Dogs.ToListAsync();
                return PartialView("_DogsPartial", allDogs); // Použijeme èásteèný pohled pro zobrazení všech psù
            }

            // Vyhledáme psy podle zadaného jména
            var dogs = await _shelterContext.Dogs.Where(d => d.Name.Contains(name)).ToListAsync();
            return PartialView("_DogsPartial", dogs); // Použijeme èásteèný pohled pro zobrazení výsledkù vyhledávání
        }





    }
}
;