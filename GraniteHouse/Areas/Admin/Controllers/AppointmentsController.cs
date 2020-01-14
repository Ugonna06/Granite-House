using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GraniteHouse.Data;
using GraniteHouse.Models.VIewModel;
using GraniteHouse.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GraniteHouse.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.SuperAdminEndUser + "," + SD.SuperAdminEndUser)]
    [Area("Admin")]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _db;

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
        public async Task<IActionResult> Index()
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            AppointmentVM.Appointments = _db.Appointments.Include(m => m.SalesPerson).ToList();
            if (User.IsInRole(SD.AdminEndUser))
            {
                AppointmentVM.Appointments = _db.Appointments.Where(m => m.SalesPersonId == claim.Value).ToList();
            }

            return View(AppointmentVM);
        }
    }
}