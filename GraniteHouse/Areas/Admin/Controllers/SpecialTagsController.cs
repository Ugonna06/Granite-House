﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraniteHouse.Data;
using GraniteHouse.Models;
using GraniteHouse.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GraniteHouse.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.SuperAdminEndUser)]
    [Area("Admin")]
    public class SpecialTagsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public SpecialTagsController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return View(_db.SpecialTags.ToList());
        }

        //Get Create method
        public IActionResult Create()
        {
            return View();
        }


        //Post Create method
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create (SpecialTags specialTags)
        {
            if (ModelState.IsValid)
            {
                _db.Add(specialTags);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        //Get Edit method
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var specialTags = await _db.SpecialTags.FindAsync(id);
            if (specialTags == null)
            {
                return NotFound();
            }
            return View(specialTags);
        }

        //Post Edit method
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SpecialTags specialTags)
        {
            if (id != specialTags.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                _db.Update(specialTags);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(specialTags);
        }


        //Get Details method
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var specialTags = await _db.SpecialTags.FindAsync(id);
            if (specialTags == null)
            {
                return NotFound();
            }
            return View(specialTags);
        }

        //Get Delete method
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var specialTags = await _db.SpecialTags.FindAsync(id);
            if (specialTags == null)
            {
                return NotFound();
            }
            return View(specialTags);
        }

        //Post Delete method
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, SpecialTags specialTags)
        {
            if (id != specialTags.Id)
            {
                return NotFound();
            }
                _db.Remove(specialTags);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
        }


    }
}