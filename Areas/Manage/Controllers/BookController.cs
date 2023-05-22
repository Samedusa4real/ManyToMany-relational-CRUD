using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PustokTemplate.DAL;
using PustokTemplate.Helpers;
using PustokTemplate.Models;
using PustokTemplate.ViewModels;

namespace PustokTemplate.Areas.Manage.Controllers
{
    [Area("manage")]
    public class BookController : Controller
    {
        private readonly PustokDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public BookController(PustokDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }
        public IActionResult Index(int page = 1, string search = null)
        {
            var query = _context.Books
                .Include(x => x.Author).Include(x => x.Genre).Include(x=>x.Images.Where(bi=>bi.IsMain==true)).AsQueryable();

            if (search != null)
                query = query.Where(x => x.Name.Contains(search));


            ViewBag.SearchValue = search;

            return View(PaginatedList<Book>.Create(query, page, 3));
        }

        public IActionResult Create()
        {
            ViewBag.Authors = _context.Authors.ToList();
            ViewBag.Genres = _context.Genres.ToList();
            ViewBag.Tags = _context.Tags.ToList();

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Book book)
        {
            if (!ModelState.IsValid)
                return View();

            if(!_context.Authors.Any(x=>x.Id == book.AuthorId))
            {
                ModelState.AddModelError("AuthorId", "AuthorId is not correct");
                return View();
            } 

            if (!_context.Genres.Any(x => x.Id == book.GenreId))
            {
                ModelState.AddModelError("GenreId", "GenreId is not correct");
                return View();
            }

            book.GenreId = 3;

            if(book.PosterImage == null)
            {
                ModelState.AddModelError("PosterImage", "PosterImage is required");
                return View();
            }

            if (book.HoverPoster == null)
            {
                ModelState.AddModelError("HoverPoster", "HoverPoster is required");
                return View();
            }

            foreach (var tagId in book.TagIds)
            {
                BookTag bookTag = new BookTag
                {
                    TagId = tagId,
                    Book = book,
                };

                _context.BookTags.Add(bookTag);
            }

            Image poster = new Image
            {
                Url = FileManager.Save(_environment.WebRootPath, "uploads/books", book.PosterImage),
                IsMain = true,
                Book = book
            };

            Image hoverPoster = new Image
            {
                Url = FileManager.Save(_environment.WebRootPath, "uploads/books", book.HoverPoster),
                IsMain = false,
                Book = book
            };

            foreach (var img in book.BookImages)
            {
                Image bookImage = new Image
                {
                    Url = FileManager.Save(_environment.WebRootPath, "uploads/books", img),
                };

                book.Images.Add(bookImage);

            }

            _context.Images.Add(poster);
            _context.Images.Add(hoverPoster);
            _context.Books.Add(book);
            _context.SaveChanges(); 
           

            return RedirectToAction("Index");
        }

        public IActionResult Edit (int id)
        {
            ViewBag.Authors = _context.Authors.ToList();
            ViewBag.Genres = _context.Genres.ToList();
            ViewBag.Tags = _context.Tags.ToList();

            Book book = _context.Books.Include(x=>x.Images).Include(x=>x.BookTags).FirstOrDefault(x=>x.Id == id);

            book.TagIds = book.BookTags.Select(x=>x.TagId).ToList();
            
            return View(book);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit (Book book)
        {
            if (!ModelState.IsValid)
                return View();

            Book existBook = _context.Books.Include(x=>x.BookTags).Include(x=>x.Images).FirstOrDefault(x => x.Id == book.Id);

            if (existBook == null)
                return View("Error");

            if (book.AuthorId!=existBook.AuthorId && !_context.Authors.Any(x => x.Id == book.AuthorId))
            {
                ModelState.AddModelError("AuthorId", "AuthorId is not correct");
                return View();
            }

            if (book.GenreId!=existBook.GenreId && !_context.Genres.Any(x => x.Id == book.GenreId))
            {
                ModelState.AddModelError("GenreId", "GenreId is not correct");
                return View();
            }

            existBook.BookTags.RemoveAll(x => !book.TagIds.Contains(x.TagId));

            var newTagIds = book.TagIds.Where(x => !existBook.BookTags.Any(y => y.TagId == x));

            foreach (var tagId in newTagIds)
            {
                BookTag bookTag = new BookTag { TagId  = tagId };
                existBook.BookTags.Add(bookTag);
            }

            string oldPoster = null;
            if(book.PosterImage!=null)
            {
                Image poster = existBook.Images.FirstOrDefault(x => x.IsMain == true);
                oldPoster = poster.Url;
                poster.Url = FileManager.Save(_environment.WebRootPath, "uploads/books", book.PosterImage);
            }

            string oldHoverPoster = null;
            if(book.HoverPoster!=null)
            {
                Image hoverPoster = existBook.Images.FirstOrDefault(x =>x.IsMain == false);
                oldHoverPoster = hoverPoster.Url;
                hoverPoster.Url = FileManager.Save(_environment.WebRootPath, "uploads/books", book.HoverPoster);
            }

            existBook.Name = book.Name;
            existBook.AuthorId = book.AuthorId;
            existBook.GenreId = book.GenreId;
            existBook.InitialPrice = book.InitialPrice;
            existBook.IsNew = book.IsNew;
            existBook.IsFeatured = book.IsFeatured;
            existBook.IsAviable = book.IsAviable;

            _context.SaveChanges();

            if (oldPoster != null)
                FileManager.Delete(_environment.WebRootPath, "uploads/books", oldPoster);

            if (oldHoverPoster != null)
                FileManager.Delete(_environment.WebRootPath, "uploads/books", oldHoverPoster);
            
            return RedirectToAction("Index");
        }
    }
}
