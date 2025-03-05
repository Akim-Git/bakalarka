using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class ShelterController : Controller
    {
        private readonly ShelterContext _shelterContext;

        public ShelterController(ShelterContext shelterContext)
        {
            _shelterContext = shelterContext;
        }

        public IActionResult Details(int id)
        {
            var shelter = _shelterContext.Shelters
                .Include(s => s.Address)
                .FirstOrDefault(s => s.Id == id);
            if (shelter == null)
            {
                return NotFound();
            }

            var dogsInShelter = _shelterContext.Dogs.Where(d => d.ShelterId == id).ToList();
            ViewData["Dogs"] = dogsInShelter;


            // Uložíme ID aktuálního útulku do ViewBag pro použití ve formuláři
            ViewBag.ShelterId = id;

            // Nastavení ShelterContext do ViewBag
            ViewBag.ShelterContext = _shelterContext;

            return View(shelter);
        }

        [HttpPost]
        public async Task<IActionResult> AddDog(Dog dog, IFormFile imageData)
        {

            // Prověření, zda jsou všechna povinná pole vyplněna
            if (string.IsNullOrEmpty(dog.Name) || string.IsNullOrEmpty(dog.Age) || string.IsNullOrEmpty(dog.Sex) || string.IsNullOrEmpty(dog.Breed))
            {
                
                return RedirectToAction("Details", new { id = dog.ShelterId });
            }

            if (imageData != null && imageData.Length > 0)
            {
                // Ověření, že nahrávaný soubor je obrázek JPEG
                if (imageData.ContentType != "image/jpeg")
                {
                    // Pokud soubor není JPEG, vrátíme chybu
                    ModelState.AddModelError("ImageData", "Soubor musí být ve formátu JPEG (.jpg).");
                    return RedirectToAction("Details", new { id = dog.ShelterId });
                }

                using (var memoryStream = new MemoryStream())
                {
                    await imageData.CopyToAsync(memoryStream);
                    dog.ImageData = memoryStream.ToArray();
                }
            }

            _shelterContext.Dogs.Add(dog);
            await _shelterContext.SaveChangesAsync();

            return RedirectToAction("Details", new { id = dog.ShelterId });
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(int dogId, string commentContent)
        {
            // Vytvoření nového komentáře
            var comment = new Comment
            {
                Content = commentContent,
                DogId = dogId,
                CreatedAt = DateTime.Now
            };

            _shelterContext.Comments.Add(comment);
            await _shelterContext.SaveChangesAsync();

            // Získání útulku, ke kterému patří pes, pro kterého je komentář psán
            var dog = await _shelterContext.Dogs.FindAsync(dogId);
            if (dog == null)
            {
                return NotFound(); // Pes nebyl nalezen
            }

            // Přesměrování zpět na stránku Details útulku
            return RedirectToAction("Details", "Shelter", new { id = dog.ShelterId });
        }

        [HttpPost]
        public async Task<IActionResult> Adopt(int dogId)
        {
            var dog = await _shelterContext.Dogs.FindAsync(dogId);
            if (dog != null)
            {
                _shelterContext.Dogs.Remove(dog);
                await _shelterContext.SaveChangesAsync();
            }
            return RedirectToAction("Details", new { id = dog.ShelterId });
        }


    }
}
