using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraniteHouse.Data;
using GraniteHouse.Extensions;
using GraniteHouse.Models;
using GraniteHouse.Models.VIewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace GraniteHouse.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class ShoppingCartController : Controller
    {
        private readonly ApplicationDbContext _db;

        [BindProperty]
        public ShoppingCartViewModel ShoppingCartVM { get; set; }

        public ShoppingCartController(ApplicationDbContext db)
        {
            _db = db;

            ShoppingCartVM = new ShoppingCartViewModel()
            {
                Products = new List<Models.Products>()
            };
        }


        //Get Index Action For shopping cart
        public async Task<IActionResult> Index()
        {
            List<int> lstShoppingCart = HttpContext.Session.Get<List<int>>("ssShoppingCart");
            if (lstShoppingCart != null)
            {
                foreach (int cartItem in lstShoppingCart)
                {
                    //var prod = await _db.Products.Include(p => p.ProductTypes).Include(o => o.SpecialTags).Where(m => m.Id == cartItem).FirstOrDefaultAsync();   ..... this line works the same as the line below, i just wanna explore what if var and the Product works the same way
                    Products prod = await _db.Products.Include(p => p.ProductTypes).Include(o => o.SpecialTags).Where(m => m.Id == cartItem).FirstOrDefaultAsync();  //When using the include, where and other . stuffs we can use any letter in the format (m => m.(whatever id, name))
                    ShoppingCartVM.Products.Add(prod);
                }
                return View(ShoppingCartVM);
            }
            else
            {
                return View(ShoppingCartVM);
            }
        }

        //Remove post action for items in shopping cart
        public async Task<IActionResult> Remove(int id)
        {
            List<int> lstShoppingCart = HttpContext.Session.Get<List<int>>("ssShoppingCart");
            if (lstShoppingCart != null)
            {
                if (lstShoppingCart.Contains(id))
                {
                    lstShoppingCart.Remove(id);
                }
            }
            HttpContext.Session.Set("ssShoppingCart", lstShoppingCart);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost,ActionName("Index")]
        [ValidateAntiForgeryToken]
        public IActionResult IndexPost()
        {
            List<int> lstShoppingCart = HttpContext.Session.Get<List<int>>("ssShoppingCart");

            ShoppingCartVM.Appointments.AppointmentDate = ShoppingCartVM.Appointments.AppointmentDate
                                                            .AddHours(ShoppingCartVM.Appointments.AppointmentTime.Hour)
                                                            .AddMinutes(ShoppingCartVM.Appointments.AppointmentTime.Minute);

            Appointments appointments = ShoppingCartVM.Appointments;
            _db.Appointments.Add(appointments);
            _db.SaveChanges();

            int appointmentId = appointments.Id;

            foreach (var productId in lstShoppingCart)
            {
                ProductSelectedForAppointment productSelectedForAppointment = new ProductSelectedForAppointment()
                {
                    AppointmentsId = appointmentId,
                    ProductsId = productId
                };
                _db.ProductSelectedForAppointments.Add(productSelectedForAppointment);
            }
            _db.SaveChanges();
            lstShoppingCart = new List<int>();
            HttpContext.Session.Set("ssShoppingCart", lstShoppingCart);

            return RedirectToAction("AppointmentConfirmation", "ShoppingCart", new { Id = appointmentId });

        }

        public async Task<IActionResult> AppointmentConfirmation(int id)
        {

            ShoppingCartVM.Appointments = await _db.Appointments.Where(m => m.Id == id).FirstOrDefaultAsync();
            List<ProductSelectedForAppointment> prodList = await _db.ProductSelectedForAppointments.Where(p => p.AppointmentsId == id).ToListAsync();

            foreach (ProductSelectedForAppointment selectedProd in prodList)
            {
                ShoppingCartVM.Products.Add(await _db.Products.Include(m => m.ProductTypes).Include(m => m.SpecialTags).Where(m => m.Id == selectedProd.ProductsId).FirstOrDefaultAsync());
            }

            return View(ShoppingCartVM);
        }

    }
}