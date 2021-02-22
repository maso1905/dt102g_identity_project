using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AuthSystem.Data;
using AuthSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Identity;
using AuthSystem.Areas.Identity.Data;

namespace AuthSystem.Controllers
{
    // Show index page only if user is logged in (authenticated).
    [Authorize]
    public class GameController : Controller
    {
        private readonly AuthDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostEnvironment;

        public GameController(AuthDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _hostEnvironment = hostEnvironment;
        }

        // GET: Game
        public async Task<IActionResult> MyGames()
        {
            var authDbContext = _context.Games.Include(g => g.ApplicationUser);
            return View(await authDbContext.ToListAsync());
        }

        // GET: Game
        public async Task<IActionResult> Index()
        {
            var authDbContext = _context.Games.Include(g => g.ApplicationUser);
            return View(await authDbContext.ToListAsync());
        }

        // GET: Game/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var game = await _context.Games
                .Include(g => g.ApplicationUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (game == null)
            {
                return NotFound();
            }

            return View(game);
        }

        // GET: Game/Create
        public IActionResult Create()
        {
            ViewData["UserFK"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Game/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Genre,ImageFile")] Game game)
        {
            if (ModelState.IsValid)
            {
                // Saves images to wwroot/img folder.
                string wwRootPath = _hostEnvironment.WebRootPath;
                string fileName = Path.GetFileNameWithoutExtension(game.ImageFile.FileName);
                string extension = Path.GetExtension(game.ImageFile.FileName);
                game.ImageName=fileName = fileName + extension;
                string path = Path.Combine(wwRootPath + "/img/", fileName);
                using (var filestream = new FileStream(path, FileMode.Create))
                {
                    await game.ImageFile.CopyToAsync(filestream);
                }

                _context.Add(game);

                var currentUser = await _userManager.GetUserAsync(User);
                game.UserFK = currentUser.Id;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(game);
        }

        // GET: Game/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var game = await _context.Games.FindAsync(id);
            if (game == null)
            {
                return NotFound();
            }
            ViewData["UserFK"] = new SelectList(_context.Users, "Id", "Id", game.UserFK);
            return View(game);
        }

        // POST: Game/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Genre,ImageFile")] Game game)
        {
            if (id != game.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Saves images to wwroot/img folder.
                    string wwRootPath = _hostEnvironment.WebRootPath;
                    string fileName = Path.GetFileNameWithoutExtension(game.ImageFile.FileName);
                    string extension = Path.GetExtension(game.ImageFile.FileName);
                    game.ImageName = fileName = fileName + extension;
                    string path = Path.Combine(wwRootPath + "/img/", fileName);
                    using (var filestream = new FileStream(path, FileMode.Create))
                    {
                        await game.ImageFile.CopyToAsync(filestream);
                    }

                    var currentUser = await _userManager.GetUserAsync(User);
                    game.UserFK = currentUser.Id;

                    _context.Update(game);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GameExists(game.Id))
                    {
                        return NotFound();
                    }
                    else
                    {                
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(game);
        }

        // GET: Game/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var game = await _context.Games
                .Include(g => g.ApplicationUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (game == null)
            {
                return NotFound();
            }

            return View(game);
        }

        // POST: Game/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var game = await _context.Games.FindAsync(id);

            // Delete image from wwroot/img folder.
            var imagePath = Path.Combine(_hostEnvironment.WebRootPath, "img", game.ImageName);
            if (System.IO.File.Exists(imagePath))         
                System.IO.File.Delete(imagePath);

            _context.Games.Remove(game);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GameExists(int id)
        {
            return _context.Games.Any(e => e.Id == id);
        }
    }
}
