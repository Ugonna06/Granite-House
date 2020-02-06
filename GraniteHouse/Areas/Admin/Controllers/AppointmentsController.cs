using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using GraniteHouse.Data;
using GraniteHouse.Models;
using GraniteHouse.Models.VIewModel;
using GraniteHouse.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GraniteHouse.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.SuperAdminEndUser + "," + SD.AdminEndUser)]
    [Area("Admin")]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private int PageSize = 3;


        [BindProperty]
        public AppointmentViewModel AppointmentVM { get; set; }

        public AppointmentsController(ApplicationDbContext db)
        {
            _db = db;
            AppointmentVM = new AppointmentViewModel()
            {
                Appointments = new List<Models.Appointments>()
            };
        }
        public async Task<IActionResult> Index(int productPage = 1, string searchName = null, string searchEmail = null, string searchPhone = null, string searchDate = null)
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);


            StringBuilder param = new StringBuilder(); ///this is an attempt to build the url that the tag helpers would consume. it would be built based on the parameters we recieve from the index argument. we also include the search criteria should the search item be more than the page size as such pagination would have to occur so if you go to the next page, the search criteria would still remain the same with what was searched in the first page.

            param.Append("/Admin/Appointments?productPage=:");
            param.Append("&searchName");
            if (searchName != null)
            {
                param.Append(searchName);
            }
            param.Append("&searchEmail");
            if (searchEmail != null)
            {
                param.Append(searchEmail);
            }
            param.Append("&searchPhone");
            if (searchPhone != null)
            {
                param.Append(searchPhone);
            }
            param.Append("&searchDate");
            if (searchDate != null)
            {
                param.Append(searchDate);
            }



            AppointmentVM.Appointments = _db.Appointments.Include(m => m.SalesPerson).ToList();
            if (User.IsInRole(SD.AdminEndUser))
            {
                AppointmentVM.Appointments = AppointmentVM.Appointments.Where(m => m.SalesPersonId == claim.Value).ToList();
            }

            if (searchName != null)
            {
                AppointmentVM.Appointments =  AppointmentVM.Appointments.Where(m => m.CustomerName.ToLower().Contains(searchName.ToLower())).ToList();
            }
            if (searchEmail != null)
            {
                AppointmentVM.Appointments = AppointmentVM.Appointments.Where(m => m.CustomerEmail.ToLower().Contains(searchEmail.ToLower())).ToList();
            }
            if (searchPhone != null)
            {
                AppointmentVM.Appointments = AppointmentVM.Appointments.Where(m => m.CustomerPhoneNumber.ToLower().Contains(searchPhone.ToLower())).ToList();
            }
            if (searchDate != null)
            {
                try
                {
                    DateTime appDate = Convert.ToDateTime(searchDate);
                    AppointmentVM.Appointments = AppointmentVM.Appointments.Where(m => m.AppointmentDate.ToShortDateString().Equals(appDate.ToShortDateString())).ToList();
                }
                catch (Exception ex)
                {
                }
            }

            var count = AppointmentVM.Appointments.Count;

            AppointmentVM.Appointments = AppointmentVM.Appointments.OrderBy(p => p.AppointmentDate)
                .Skip((productPage - 1) * PageSize)
                .Take(PageSize).ToList();

            AppointmentVM.PagingInfo = new PagingInfo()
            {
                CurrentPage = productPage,
                ItemsPerPage = PageSize,
                TotalItems = count,
                urlParams = param.ToString()
            };


            return View(AppointmentVM);
        }


        //Get Edit action
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productList = (IEnumerable<Products>)(from p in _db.Products
                                                      join a in _db.ProductSelectedForAppointments
                                                      on p.Id equals a.ProductsId
                                                      where a.AppointmentsId == id
                                                      select p).Include("ProductTypes");//this is a link method that brings the products selected by a particular appointment
            AppointmentDetailsViewModel objAppointmentVM = new AppointmentDetailsViewModel()
            {
                Appointments = _db.Appointments.Include(m => m.SalesPerson).Where(a => a.Id == id).FirstOrDefault(),
                SalesPerson = _db.ApplicationUser.ToList(),
                Products = productList.ToList()
            };

            return View(objAppointmentVM);
        }

        //Post edit action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit (int id, AppointmentDetailsViewModel objAppointmentVM)
        {
            if (ModelState.IsValid)
            {
                objAppointmentVM.Appointments.AppointmentDate = objAppointmentVM.Appointments.AppointmentDate
                                                                .AddHours(objAppointmentVM.Appointments.AppointmentTime.Hour)
                                                                .AddMinutes(objAppointmentVM.Appointments.AppointmentTime.Minute); // this adds the time to the date to save it in the db

                var appointmentFromDb = await _db.Appointments.Where(a => a.Id == objAppointmentVM.Appointments.Id).FirstOrDefaultAsync();

                appointmentFromDb.CustomerName = objAppointmentVM.Appointments.CustomerName;
                appointmentFromDb.CustomerPhoneNumber = objAppointmentVM.Appointments.CustomerPhoneNumber;
                appointmentFromDb.CustomerEmail = objAppointmentVM.Appointments.CustomerEmail;
                appointmentFromDb.AppointmentDate = objAppointmentVM.Appointments.AppointmentDate;
                appointmentFromDb.IsConfirmed = objAppointmentVM.Appointments.IsConfirmed;
                if (User.IsInRole(SD.SuperAdminEndUser))
                {
                    appointmentFromDb.SalesPersonId = objAppointmentVM.Appointments.SalesPersonId;
                }

                await _db.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(objAppointmentVM);
        }


        //Get Details action
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productList = (IEnumerable<Products>)(from p in _db.Products
                                                      join a in _db.ProductSelectedForAppointments
                                                      on p.Id equals a.ProductsId
                                                      where a.AppointmentsId == id
                                                      select p).Include("ProductTypes");
            AppointmentDetailsViewModel objAppointmentVM = new AppointmentDetailsViewModel()
            {
                Appointments = _db.Appointments.Include(m => m.SalesPerson).Where(a => a.Id == id).FirstOrDefault(),
                SalesPerson = _db.ApplicationUser.ToList(),
                Products = productList.ToList()
            };

            return View(objAppointmentVM);
        }

        //Get Delete Action
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();

            }

            var productList = (IEnumerable<Products>)(from p in _db.Products
                                                      join a in _db.ProductSelectedForAppointments
                                                      on p.Id equals a.ProductsId
                                                      where a.AppointmentsId == id
                                                      select p).Include("ProductTypes");
            AppointmentDetailsViewModel objAppointmentVM = new AppointmentDetailsViewModel()
            {
                Appointments = _db.Appointments.Include(m => m.SalesPerson).Where(a => a.Id == id).FirstOrDefault(),
                SalesPerson = _db.ApplicationUser.ToList(),
                Products = productList.ToList()
            };

            return View(objAppointmentVM);
        }

        //Post Delete action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var appointment = await _db.Appointments.FindAsync(id);
            _db.Appointments.Remove(appointment);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}