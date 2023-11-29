using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PustokHomeWork.Areas.Admin.Service;
using PustokHomeWork.Data;
using PustokHomeWork.Models;
using static System.Net.Mime.MediaTypeNames;

namespace PustokHomeWork.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BookController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly IWebHostEnvironment _env;
        public BookController(AppDbContext dbContext, IWebHostEnvironment env)
        {
            _dbContext = dbContext;
            _env = env;

        }


        public IActionResult Index()
        {
            return View(_dbContext.Books.Include(book => book.Tags).ToList());
        }

        [HttpPost]
        public IActionResult UpdateBook(BookModel updatedBook)
        {
            var authors = _dbContext.Authors.ToList();
            var tags = _dbContext.Tags.ToList();
            ViewBag.Authors = authors;
            ViewBag.Tags = tags;



            BookModel oldBook = _dbContext.Books.Include(x=>x.Tags).Include(x=>x.BookImages).FirstOrDefault(x=>x.Id == updatedBook.Id);

            if(oldBook == null)
            {
                return BadRequest();
            }

            if (updatedBook.TagIds == null || updatedBook.TagIds.Count == 0 || !updatedBook.TagIds.All(x => tags.Any(tag => tag.Id == x)))
            {
                ModelState.AddModelError("TagIds", "Tag error");
                return View("Update", model: oldBook);
            }

            if (updatedBook?.AuthorId == null || !authors.Any(x => x.Id == updatedBook?.AuthorId))
            {
                ModelState.AddModelError("AuthorId", "Invalid Author");
                return View("Update", model: oldBook);
            }


            if (!ModelState.IsValid)
            {

                return View("Update", model: oldBook);
            }

            if (updatedBook.TagIds != null)
            {
                var selectedTags = _dbContext.Tags.Where(t => updatedBook.TagIds.Contains(t.Id)).ToList();

                var removedTags = oldBook.Tags.Where(tag => !selectedTags.Any(st => st.Id == tag.Id)).ToList();
                foreach (var removedTag in removedTags)
                {
                    oldBook.Tags.Remove(removedTag);
                }

                var addedTags = selectedTags.Where(st => !oldBook.Tags.Any(tag => tag.Id == st.Id)).ToList();
                foreach (var addedTag in addedTags)
                {
                    oldBook.Tags.Add(addedTag);
                }
            }


            if(updatedBook.ImageFiles != null)
            {
                oldBook.BookImages.RemoveAll(x=>x.isPoster == null);

                foreach(var file in updatedBook.ImageFiles)
                {
                    if (!((new string[] { "image/jpeg", "image/png" }).Contains(file.ContentType) && (file.Length < 3 * 1024 * 1024)))
                    {
                        return BadRequest();
                    }
                    string filename = FileManager.Save(file);
                    oldBook.BookImages.Add(new BookImageModel() { ImageURI =filename, isPoster=null });
                  
                }
            }

            if (updatedBook.BookPosterImage !=null) {

                oldBook.BookImages.RemoveAll(x => x.isPoster == true);
                if (!((new string[] { "image/jpeg", "image/png" }).Contains(updatedBook.BookPosterImage.ContentType) && (updatedBook.BookPosterImage.Length < 3 * 1024 * 1024)))
                {
                    return BadRequest();
                }

                string filename = FileManager.Save(updatedBook.BookPosterImage);
                oldBook.BookImages.Add(new BookImageModel() { ImageURI = filename, isPoster = true });

            }



            if (updatedBook.BookHoverImage != null)
            {

                oldBook.BookImages.RemoveAll(x => x.isPoster == false);
                if (!((new string[] { "image/jpeg", "image/png" }).Contains(updatedBook.BookHoverImage.ContentType) && (updatedBook.BookHoverImage.Length < 3 * 1024 * 1024)))
                {
                    return BadRequest();
                }

                string filename = FileManager.Save(updatedBook.BookHoverImage);
                updatedBook.BookImages.Add(new BookImageModel() { ImageURI = filename, isPoster = false });

            }

            oldBook.Name = updatedBook.Name;
            oldBook.Price = updatedBook.Price;
            oldBook.AuthorId = updatedBook.AuthorId;
          
            _dbContext.SaveChanges();


            return RedirectToAction("Index");

        }

        public IActionResult Update(int id)
        {
            var authors = _dbContext.Authors.ToList();
            var tags = _dbContext.Tags.ToList();

            ViewBag.Authors = authors;
            ViewBag.Tags = tags;


            BookModel existBook = _dbContext.Books.FirstOrDefault(x => x.Id == id);

            if(existBook == null)
            {
                return BadRequest();
            }

            return View(existBook);

        }

        public IActionResult Create()
        {
            ViewBag.Authors = _dbContext.Authors.ToList();
            ViewBag.Tags = _dbContext.Tags.ToList();
            return View();
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            BookModel? model = _dbContext.Books.FirstOrDefault(x => x.Id == id);

            if(model != null)
            {




                _dbContext.Books.Remove(model);
                foreach(var file in model.ImageFiles){
                    FileManager.Remove(file.FileName);
                }
                _dbContext.SaveChanges();

                return RedirectToAction("Index");
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPost]
        public IActionResult CreateBook(BookModel newBook)
        {
            var authors = _dbContext.Authors.ToList();
            var tags = _dbContext.Tags.ToList();
            ViewBag.Authors = authors;
            ViewBag.Tags = tags;

            if(newBook.TagIds == null || newBook.TagIds.Count == 0 || !newBook.TagIds.All(x=>tags.Any(tag=>tag.Id == x)))
            {
                ModelState.AddModelError("TagIds", "Tag error");
            }

            if(newBook?.AuthorId == null || !authors.Any(x=>x.Id == newBook?.AuthorId))
            {
                ModelState.AddModelError("AuthorId", "Invalid Author");
            }


            if (!ModelState.IsValid)
            {
             
                return View("Create");
            }

            newBook.BookImages = new List<BookImageModel>();

            if(newBook.ImageFiles != null)
            {
                foreach(var image in newBook.ImageFiles)
                {
                    if(!new string[] {"image/jpeg", "image/png" }.Contains(image.ContentType) || ! (image.Length < 3 * 1024 * 1024))
                    {
                        return BadRequest();
                    }
                    string filename = FileManager.Save(image);

                    newBook.BookImages.Add(new BookImageModel() { ImageURI=filename, isPoster=null});
                }
            }
            if (newBook.BookPosterImage != null) {

                if (!((new string[] { "image/jpeg", "image/png" }).Contains(newBook.BookPosterImage.ContentType) && (newBook.BookPosterImage.Length < 3 * 1024 * 1024)))
                {
                    return BadRequest();
                }
                string filename = FileManager.Save(newBook.BookPosterImage);

                newBook.BookImages.Add(new BookImageModel() { ImageURI = filename, isPoster = true });

            }



            if (newBook.BookHoverImage != null)
            {

                if (!((new string[] { "image/jpeg", "image/png" }).Contains(newBook.BookHoverImage.ContentType) && (newBook.BookHoverImage.Length < 3 * 1024 * 1024)))
                    {
                    return BadRequest();
                }
                string filename = FileManager.Save(newBook.BookHoverImage);

                newBook.BookImages.Add(new BookImageModel() { ImageURI = filename, isPoster = false });

            }




            var selectedTags = _dbContext.Tags.Where(t => newBook.TagIds.Contains(t.Id)).ToList();

            newBook.Tags = new List<TagModel>();
            foreach (var tag in selectedTags)
            {
                newBook.Tags.Add(tag);
            }

                _dbContext.Books.Add(newBook);
                _dbContext.SaveChanges();

            return RedirectToAction("Index");
            
        }


       

        




    }
}
