using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentCar.DAL;
using RentCar.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RentCar.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _dbContext;

        public HomeController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index()
        {
            return View(new HomeIndexViewModel { 
                CarModels = await _dbContext.CarModels.Where(c => !c.IsDeleted)
                .Include(c => c.Car)
                .Include(c => c.District).ThenInclude(d => d.City)
                .Include(c => c.CarImages.Where(i => i.IsMain))
                .Take(4).Where(c => c.Rating > 2).ToListAsync()
            });
        }

        public async Task<IActionResult> LoadMore(int skipCount)
        {
            var carModels = await _dbContext.CarModels.Where(c => !c.IsDeleted)
                .Skip(skipCount).Take(4).Include(c => c.Car)
                .Include(c => c.District).ThenInclude(d => d.City)
                .Include(c => c.CarImages.Where(i => i.IsMain))
                .ToListAsync();

            return PartialView("_CarModelPartial", new HomeIndexViewModel { CarModels = carModels});
        }

    }
}
