using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class DogsController : Controller
    {
        private readonly ShelterContext _context;

        public DogsController(ShelterContext context)
        {
            _context = context;
        }

        // GET: Dogs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dog = await _context.Dogs.FindAsync(id);
            if (dog == null)
            {
                return NotFound();
            }
            return View(dog);
        }



        [HttpPost]
        public async Task<IActionResult> Save(int id, [Bind("Id,Name,Age,Sex,Breed,ImageData")] Dog dog, IFormFile ImageData)
        {
            if (id != dog.Id)
            {
                return NotFound();
            }

            var existingDog = await _context.Dogs.FindAsync(id);

            if (existingDog == null)
            {
                return NotFound();
            }

            // Prověření, zda je poskytnut nový obrázek | && newImageData.Length > 0
            if (ImageData != null )
            {
                // Ověření, že nahrávaný soubor je obrázek JPEG
                if (ImageData.ContentType != "image/jpeg")
                {
                    // Pokud soubor není JPEG, vrátíme chybu
                    ModelState.AddModelError("ImageData", "Soubor musí být ve formátu JPEG (.jpg).");
                    return View(dog);
                }

                using (var memoryStream = new MemoryStream())
                {
                    await ImageData.CopyToAsync(memoryStream);
                    dog.ImageData = memoryStream.ToArray();
                }
            }
            else
            {
                // Použijeme stávající obrázek, pokud žádný nový není nahrán
                dog.ImageData = existingDog.ImageData;
            }

            // Aktualizace zbývajících vlastností psa
            existingDog.Name = dog.Name;
            existingDog.Age = dog.Age;
            existingDog.Sex = dog.Sex;
            existingDog.Breed = dog.Breed;
            existingDog.ImageData = dog.ImageData; // Aktualizace ImageData

            try
            {
                _context.Update(existingDog);
                await _context.SaveChangesAsync();
                //return RedirectToAction("Index", "Home", new { id = existingDog.ShelterId });
                return RedirectToAction("Details", "Shelter", new { id = existingDog.ShelterId });

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DogExists(existingDog.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool DogExists(int id)
        {
            throw new NotImplementedException();
        }

       


    }
}