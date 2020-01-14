using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GraniteHouse.Data;
using GraniteHouse.Models.VIewModel;
using GraniteHouse.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GraniteHouse.Controllers
{
    [Authorize(Roles = SD.SuperAdminEndUser)]
    [Area("Admin")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private HostingEnvironment _hostingEnvironment;
        [BindProperty]
        public ProductsViewModel ProductsVM { get; set; }
        public ProductsController(ApplicationDbContext db, HostingEnvironment hostingEnvironment)
        {
            _db = db;
            _hostingEnvironment = hostingEnvironment;
            ProductsVM = new ProductsViewModel()
            {
                ProductTypes = _db.ProductTypes.ToList(),
                SpecialTags = _db.SpecialTags.ToList(),
                Products = new Models.Products()
            };

        }
        public async Task<IActionResult> Index()
        {
            var products = _db.Products.Include(m => m.ProductTypes).Include(m => m.SpecialTags);
            return View(await products.ToListAsync());
        }

        //Get Create Method
        public IActionResult Create()
        {
            return View(ProductsVM);
        }

        //Post Create Method
        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePOST()
        {
            if (!ModelState.IsValid)
            {
                return View(ProductsVM);
            }

            _db.Products.Add(ProductsVM.Products);
            await _db.SaveChangesAsync();

            //To add image
            string webRootPath = _hostingEnvironment.WebRootPath; //This will retrieve the path of the wwwroot folder and save it in the webRootPath
            var files = HttpContext.Request.Form.Files; //This saves whatever file was uploaded to the variable "files"
            var productsFromDb = _db.Products.Find(ProductsVM.Products.Id);


            if (files.Count != 0)
            {
                //It means that a file was uploaded in the form
                var uploads = Path.Combine(webRootPath, SD.ImageFolder); //This states where the file will be uploaded to. webRootPath signifies the wwwroot folder, while the SD.ImageFolder gives the path to where the images are stored(check in the Utility folder to view the path)
                var extension = Path.GetExtension(files[0].FileName); //This saves the extension of the file uploaded

                using (var filestream = new FileStream(Path.Combine(uploads, ProductsVM.Products.Id + extension), FileMode.Create)) //this creates the path and the new file name for the image by using FileMode.Create
                {
                    files[0].CopyTo(filestream); //This then copies the image uploaded onto the server and renames it to the name specified in the filestream.
                }

                productsFromDb.Image = $@"\{SD.ImageFolder}\{ProductsVM.Products.Id + extension}"; //This updates the products in the database with the new path for the image

            }
            else
            {
                //If no image was added
                var uploads = Path.Combine(webRootPath, SD.ImageFolder + @"\" + SD.DefaultProductImage);//This specifies where the default image of the application is.
                System.IO.File.Copy(uploads, webRootPath + @"\" + SD.ImageFolder + @"\" + ProductsVM.Products.Id + ".png");//This is done to copy the image from its original location to the location we want on the server. The first parameter in the "file.copy" method is the current location of the file while the 2nd parameter is the location where the file is to be copies to
                productsFromDb.Image = $@"\{SD.ImageFolder}\{ProductsVM.Products.Id}.png"; // Here we update the products image with the path

            }
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }


        //Get Edit method

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            ProductsVM.Products = await _db.Products.Include(m => m.ProductTypes).Include(m => m.SpecialTags).SingleOrDefaultAsync(m => m.Id == id);
            if (ProductsVM.Products == null)
            {
                return NotFound();
            }

            return View(ProductsVM);

        }

        //Post Edit Method
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            if (ModelState.IsValid)
            {
                string webRootPath = _hostingEnvironment.WebRootPath;
                var files = HttpContext.Request.Form.Files;
                var productFromDB = _db.Products.Where(m => m.Id == ProductsVM.Products.Id).FirstOrDefault();


                if (files.Count != 0 )
                {
                    var uploads = Path.Combine(webRootPath, SD.ImageFolder);
                    var extension_new = Path.GetExtension(files[0].FileName);
                    var extension_old = Path.GetExtension(productFromDB.Image);

                    if (System.IO.File.Exists(Path.Combine(webRootPath, ProductsVM.Products.Id + extension_old)))//Before editing an image in the database, you have to first delete the image in the db first and then upload the new image, this if statement checks if the file exists.
                    {
                        System.IO.File.Delete(Path.Combine(webRootPath, ProductsVM.Products.Id + extension_old));
                    }
                    using (var filestream = new FileStream(Path.Combine(uploads, ProductsVM.Products.Id + extension_new), FileMode.Create)) //this creates the path and the new file name for the image by using FileMode.Create
                    {
                        files[0].CopyTo(filestream); //This then copies the image uploaded onto the server and renames it to the name specified in the filestream.
                    }

                    ProductsVM.Products.Image = $@"\{SD.ImageFolder}\{ProductsVM.Products.Id + extension_new}"; //This updates the products in the database with the new path for the image

                }

                if (ProductsVM.Products.Image != null)
                {
                    productFromDB.Image = ProductsVM.Products.Image;
                }

                productFromDB.Name = ProductsVM.Products.Name;
                productFromDB.Price = ProductsVM.Products.Price;
                productFromDB.Available = ProductsVM.Products.Available;
                productFromDB.SpecialTagsId = ProductsVM.Products.SpecialTagsId;
                productFromDB.ProductTypesId = ProductsVM.Products.ProductTypesId;
                productFromDB.ShadeColor = ProductsVM.Products.ShadeColor;

                await _db.SaveChangesAsync();

                return RedirectToAction(nameof(Index));

            }

            return View(ProductsVM);
        }


        //Get Details method
        public async Task<IActionResult> Details(int? id)
        {
           if (id == null)
            {
                return NotFound();
            }
            ProductsVM.Products = await _db.Products.Include(m => m.ProductTypes).Include(m => m.SpecialTags).SingleOrDefaultAsync(m => m.Id == id);
            if (ProductsVM.Products == null)
            {
                return NotFound();
            }
            return View(ProductsVM);
        }


        //Get Delete Method
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            ProductsVM.Products = await _db.Products.Include(m => m.ProductTypes).Include(m => m.SpecialTags).SingleOrDefaultAsync(m => m.Id == id);
            if (ProductsVM.Products == null)
            {
                return NotFound();
            }
            return View(ProductsVM);
        }

        //Post Delete Method
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            string webRootPath = _hostingEnvironment.WebRootPath;
            ProductsVM.Products = _db.Products.Include(m => m.ProductTypes).Include(m => m.SpecialTags).SingleOrDefault(m => m.Id == id);

            if (ProductsVM.Products == null)
            {
                return NotFound();
            }
            else
            {
                var uploads = Path.Combine(webRootPath, SD.ImageFolder);
                var extension = Path.GetExtension(ProductsVM.Products.Image);

                if (System.IO.File.Exists(Path.Combine(uploads, ProductsVM.Products.Id + extension)))
                {
                    System.IO.File.Delete(Path.Combine(uploads, ProductsVM.Products.Id + extension));
                }

                _db.Products.Remove(ProductsVM.Products);
                await _db.SaveChangesAsync();

                return RedirectToAction(nameof(Index));


            }


        }







    }
}