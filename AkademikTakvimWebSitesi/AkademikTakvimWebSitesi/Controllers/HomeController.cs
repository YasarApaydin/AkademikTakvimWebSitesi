using System.Diagnostics;
using AkademikTakvimWebSitesi.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using System.IO.Compression;

namespace AkademikTakvimWebSitesi.Controllers
{
    public class HomeController : Controller
    {

        private readonly AppDbContext context;

        public HomeController(AppDbContext _context)
        {
            context = _context;
            
        }




        [Authorize]
        public IActionResult Edit(int id)
        {
            var eventToEdit = context.Events.FirstOrDefault(e => e.Id == id);
            if (eventToEdit == null)
            {
                return NotFound();
            }

            var model = new EventViewModel
            {
                Id = eventToEdit.Id,
                Title = eventToEdit.Title,
                Description = eventToEdit.Description,
                StartDate = eventToEdit.StartDate,
                EndDate = eventToEdit.EndDate
            };

            return View(model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(EventViewModel model)
        {
            if (ModelState.IsValid)
            {
                var eventToEdit = context.Events.FirstOrDefault(e => e.Id == model.Id);
                if (eventToEdit == null)
                {
                    return NotFound();
                }

                eventToEdit.Title = model.Title;
                eventToEdit.Description = model.Description;
                eventToEdit.StartDate = model.StartDate;
                eventToEdit.EndDate = model.EndDate;

                await context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Etkinlik baþarýyla güncellendi!";


                return View(model);
            }

            return View(model);
        }




        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }
       

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var kullanici = context.Users
                    .FirstOrDefault(u => u.Email == model.Email && u.Password == model.Sifre);

                if (kullanici != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, kullanici.AdSoyad),
                        new Claim(ClaimTypes.Email, kullanici.Email),
                        new Claim("UserId", kullanici.Id.ToString())
                    };

             
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

               
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                    return RedirectToAction("Anasayfa", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "E-posta veya þifre yanlýþ");
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Anasayfa", "Home");
        }

        public IActionResult Anasayfa(DateTime? startDate, DateTime? endDate)
        {
  
            ViewData["StartDate"] = startDate?.ToString("yyyy-MM-dd");
            ViewData["EndDate"] = endDate?.ToString("yyyy-MM-dd");

         
            IQueryable<Event> eventsQuery = context.Events.Include(e => e.Yonetici);

 
            if (startDate.HasValue)
            {
                startDate = startDate.Value.Date;  
                eventsQuery = eventsQuery.Where(e => e.StartDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                endDate = endDate.Value.Date.AddDays(1).AddMilliseconds(-1); 
                eventsQuery = eventsQuery.Where(e => e.EndDate <= endDate.Value);
            }

           
            var events = eventsQuery.ToList();
            return View(events);
        }


        [Authorize]
        public IActionResult Ekle()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ekle(EventViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue("UserId");

                if (userId != null)
                {
                    var newEvent = new Event
                    {
                        Title = model.Title,
                        Description = model.Description,
                        StartDate = model.StartDate,
                        EndDate = model.EndDate,
                        YoneticiId = Convert.ToInt32(userId) 
                    };

               
                    context.Events.Add(newEvent);
                    await context.SaveChangesAsync();

                    return RedirectToAction("Anasayfa");
                }
            }


            return View(model);
        }


        public IActionResult KayitOl()
        {
            return View();
        }




        [Authorize]
        public IActionResult Delete(int id)
        {
            var eventToDelete = context.Events.Find(id);
            if (eventToDelete != null)
            {
                context.Events.Remove(eventToDelete);
                context.SaveChanges();
            }
            return RedirectToAction("Anasayfa");
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult KayitOl(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
         
                var mevcutKullanici = context.Users.FirstOrDefault(u => u.Email == model.Email);
                if (mevcutKullanici != null)
                {

                    ModelState.AddModelError("Email", "Bu e-posta adresi zaten kayýtlý.");
                    return View(model); 
                }

                var yeniKullanici = new User
                {
                    AdSoyad = model.AdSoyad,
                    Email = model.Email,
                    Password = model.Sifre,
                    GsmNo = model.GsmNo
                };
                  context.Users.Add(yeniKullanici);
                context.SaveChanges();
                TempData["SuccessMessage"] = "Baþarýyla Eklendi";
          
                //return RedirectToAction("Login", "Home");
            }

            return View(model);
        }

        public IActionResult DisaAktar(DateTime? startDate, DateTime? endDate)
        {
           
            ViewData["StartDate"] = startDate?.ToString("yyyy-MM-dd");
            ViewData["EndDate"] = endDate?.ToString("yyyy-MM-dd");

         
            IQueryable<Event> eventsQuery = context.Events.Include(e => e.Yonetici);


            if (startDate.HasValue)
            {
                startDate = startDate.Value.Date;
                eventsQuery = eventsQuery.Where(e => e.StartDate >= startDate.Value);
            }

         
            if (endDate.HasValue)
            {
                endDate = endDate.Value.Date.AddDays(1).AddMilliseconds(-1);
                eventsQuery = eventsQuery.Where(e => e.EndDate <= endDate.Value);
            }

            var fileNamePrefix = "event_";
            var files = new List<(string fileName, byte[] fileContent)>();

          
            var eventsToExport = eventsQuery.ToList();

            
            foreach (var eventItem in eventsToExport)
            {
                var icsContent = YazICS(new List<Event> { eventItem });
                var fileName = $"{fileNamePrefix}{eventItem.Id}.ics";
                files.Add((fileName, Encoding.UTF8.GetBytes(icsContent)));
            }

            var zipFile = Ziple(files);
            return File(zipFile, "application/zip", "events.zip");
        }

        private string YazICS(List<Event> events)
        {
            var sb = new StringBuilder();
            sb.AppendLine("BEGIN:VCALENDAR");
            sb.AppendLine("VERSION:2.0");
            sb.AppendLine("CALSCALE:GREGORIAN");

            foreach (var eventItem in events)
            {
                sb.AppendLine("BEGIN:VEVENT");
                sb.AppendLine($"SUMMARY:{eventItem.Title}");
                sb.AppendLine($"DESCRIPTION:{eventItem.Description}");
                sb.AppendLine($"DTSTART:{eventItem.StartDate:yyyyMMddTHHmmss}");
                sb.AppendLine($"DTEND:{eventItem.EndDate:yyyyMMddTHHmmss}");
                sb.AppendLine("END:VEVENT");
            }

            sb.AppendLine("END:VCALENDAR");
            return sb.ToString();
        }

        private byte[] Ziple(List<(string fileName, byte[] fileContent)> files)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (var file in files)
                    {
                        var zipEntry = zipArchive.CreateEntry(file.fileName);
                        using (var entryStream = zipEntry.Open())
                        {
                            entryStream.Write(file.fileContent, 0, file.fileContent.Length);
                        }
                    }
                }
                return memoryStream.ToArray();
            }
        }





    }
}
