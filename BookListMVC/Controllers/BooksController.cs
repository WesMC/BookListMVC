using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookListMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookListMVC.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDBContext _db;
        [BindProperty]
        public Book Book { get; set; }

        public BooksController(ApplicationDBContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Upsert(int? id)
        {
            Book = new Book();

            if (id != null)
            {
                // update
                Book = await _db.Books.FirstOrDefaultAsync(u=>u.Id == id);

                if (Book == null)
                {
                    return NotFound();
                }
                return View(Book);
            }
            //create

            return View(Book);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert()
        {
            if (ModelState.IsValid)
            {
                if (Book.Id == 0)
                {
                    //create
                    await _db.Books.AddAsync(Book);
                }
                else
                {
                    //Update
                    var BookFromDb = await _db.Books.FindAsync(Book.Id);
                    BookFromDb.Name = Book.Name;
                    BookFromDb.ISBN = Book.ISBN;
                    BookFromDb.Author = Book.Author;
                }

                await _db.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            return View();
        }

        #region API Calls
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Json(new { data = await _db.Books.ToListAsync() });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var bookFromDb = await _db.Books.FirstOrDefaultAsync(u => u.Id == id);
            if (bookFromDb == null)
            {
                return Json(new { success = false, message = "Error while Deleting" });
            }
            _db.Books.Remove(bookFromDb);
            await _db.SaveChangesAsync();
            return Json(new { success = true, message = "Delete successful" });
        }
        #endregion
    }
}