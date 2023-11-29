using AllUpHomeWork.Areas.Admin.Models;
using AllUpHomeWork.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PustokHomeWork.Areas.Admin.Service;

namespace AllUpHomeWork.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _dbContext;
        public ProductController(AppDbContext context)
        {
            _dbContext = context;
        }
        public IActionResult Show()
        {

            ViewBag.Sizes = _dbContext.sizes.ToList();
            ViewBag.Colors = _dbContext.colors.ToList();

            return View(_dbContext.products.Include(x=>x.Colors).Include(x=>x.Sizes).ToList());
        }


        public IActionResult Update(int? id)
        {

            

            if(id == null)
            {
                return NotFound();
            }

            ProductModel existPro = _dbContext.products.Include(x=>x.Colors).Include(x => x.Sizes).FirstOrDefault(x => x.Id == id);

            if(existPro == null)
            {
                return BadRequest();
            }

            ViewBag.Colors = _dbContext.colors.ToList();
            ViewBag.Sizes = _dbContext.sizes.ToList();

            return View(existPro);

        }


        public IActionResult Create()
        {
            ViewBag.Colors = _dbContext.colors.ToList();
            ViewBag.Sizes = _dbContext.sizes.ToList();

            return View();
        }

        [HttpPost]
        public IActionResult CreateProduct(ProductModel newProduct)
        {

            bool sizeCheck = true;
            bool colorCheck = true;

            if (newProduct.ColorIds == null || newProduct.ColorIds.Count == 0)
            {
                colorCheck = false;
            }

            if (newProduct.SizeIds == null || newProduct.SizeIds.Count == 0)
            {
                sizeCheck = false;
            }

            if (!sizeCheck && !colorCheck)
            {
                ModelState.AddModelError("ColorIds", "Invalid Colors");
                ModelState.AddModelError("SizeIds", "Invalid Sizes");
            }

            else if (sizeCheck == false)
            {
                ModelState.AddModelError("SizeIds", "Invalid Sizes");
            }

            else
            {
                ModelState.AddModelError("ColorIds", "Invalid Colors");
            }

            if (!(sizeCheck && colorCheck))
            {
                ViewBag.Colors = _dbContext.colors.ToList();
                ViewBag.Sizes = _dbContext.sizes.ToList();

                return View("Create", model: _dbContext.products.Include(x => x.Colors).Include(x => x.Sizes).FirstOrDefault(x => x.Id == newProduct.Id));
            }

            newProduct.ProductImage = new List<ProductImageModel>();

            if (newProduct.UploadedImage != null)
            {
                string name = FileManager.Save(newProduct.UploadedImage);
                newProduct.ProductImage.Add(new ProductImageModel() { ImagePath = name});
            }

            var selectedColors = _dbContext.colors.Where(c => newProduct.ColorIds.Contains(c.Id)).ToList();
            var selectedSizes = _dbContext.sizes.Where(s => newProduct.SizeIds.Contains(s.Id)).ToList();

            newProduct.Colors = selectedColors;
            newProduct.Sizes = selectedSizes;

            _dbContext.products.Add(newProduct);
            _dbContext.SaveChanges();


            return RedirectToAction("Show");            
        }


        [HttpPost]
        public IActionResult UpdateProduct(ProductModel existsPro) {

            if(existsPro.Id == null)
            {
                return BadRequest();
            }

            ProductModel oldPro = _dbContext.products.Include(x=>x.Colors).Include(x=>x.Sizes).Include(x=>x.ProductImage).FirstOrDefault(x=>x.Id == existsPro.Id);

            if(oldPro == null)
            {
                return BadRequest();
            }

        
            if(existsPro?.ColorIds != null && existsPro.ColorIds.Count > 0)
            {
                var selectedColors = _dbContext.colors.Where(c => existsPro.ColorIds.Contains(c.Id)).ToList();
                
                var removedColors = oldPro.Colors.Where(color => !selectedColors.Any(sc => sc.Id == color.Id)).ToList();

                foreach (var removedColor in removedColors)
                {
                    oldPro.Colors.Remove(removedColor);
                }


                var addedColors = selectedColors.Where(sc => !oldPro.Colors.Any(color => color.Id == sc.Id)).ToList();
                
                foreach (var addedColor in addedColors)
                {
                    oldPro.Colors.Add(addedColor);
                }


            }


            if (existsPro?.SizeIds != null && existsPro.SizeIds.Count > 0)
            {

                var selectedSizes = _dbContext.sizes.Where(s => existsPro.SizeIds.Contains(s.Id)).ToList();

                var removedSizes = oldPro.Sizes.Where(size => !selectedSizes.Any(ss => ss.Id == size.Id)).ToList();

                foreach (var removedSize in removedSizes)
                {
                    oldPro.Sizes.Remove(removedSize);
                }

                var addedSizes = selectedSizes.Where(ss => !oldPro.Sizes.Any(size => size.Id == ss.Id)).ToList();

                foreach (var addedSize in addedSizes)
                {
                    oldPro.Sizes.Add(addedSize);

                }
            }

            if(existsPro?.UploadedImage != null && existsPro.ProductImage.Count > 0)
            {
                existsPro.ProductImage.Clear();


                foreach(var image in existsPro.ProductImage)
                {
                    FileManager.Remove(image.ImagePath);
                }

                if(existsPro.UploadedImage != null)
                {
                    string filename = FileManager.Save(existsPro.UploadedImage);
                    oldPro.ProductImage.Add(new ProductImageModel() {ImagePath = filename });
                }
            }



            oldPro.Name = existsPro.Name;
            oldPro.Price = existsPro.Price;
            oldPro.Description = existsPro.Description;
            oldPro.SaleFraction = existsPro.SaleFraction;

            _dbContext.SaveChanges();


            return RedirectToAction("Show");


        
        }

        [HttpPost]
        public IActionResult Delete(int? id)
        {
            if(id == null)
            {
                return BadRequest();
            }

            ProductModel product = _dbContext.products.Include(x=>x.ProductImage).FirstOrDefault(x => x.Id == id);

            if(product == null)
            {
                return BadRequest();
            }

            

            foreach(var image in product.ProductImage)
            {

                FileManager.Remove(image.ImagePath);
            }

            product.ProductImage.RemoveAll(x => x.ProductId == x.Id);


            _dbContext.products.Remove(product);
            _dbContext.SaveChanges();


            return RedirectToAction("Show");
        }
    }
}
