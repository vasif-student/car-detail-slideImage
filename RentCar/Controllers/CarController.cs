using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentCar.DAL;
using RentCar.Models.Entities;
using RentCar.Models.ViewModels;
using RentCar.Models.ViewModels.Car;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RentCar.Controllers
{
    public class CarController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly UserManager<IdentityUser> _userManager;

        public CarController(AppDbContext dbContext, UserManager<IdentityUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        //***** Detail *****//
        public async Task<IActionResult> Detail(int id)
        {
            var car = await _dbContext.CarModels
                .Include(c => c.Car)
                .Include(c => c.District).ThenInclude(d => d.City)
                .Include(c => c.CarImages)
                .FirstOrDefaultAsync(c => c.Id == id);
            var comments = await _dbContext.Comments.Where(c => c.CarModelId == id).ToListAsync();

            var imageNames = await _dbContext.CarImages.Where(i => i.CarModelId == id).Select(i => i.ImageName).ToListAsync();

            string[] imageNamesArr = imageNames.ToArray();

            return View(new CarDetailViewModel { 
                CarModel = car,
                Comments = comments,
                ImageNames = imageNamesArr
            });
        }

        //***** Comment *****//
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Comment(CreateCommentViewModel model)
        {
            IdentityUser user = await _userManager.GetUserAsync(User);

            Comment comment = new Comment
            {
                UserName = user.UserName,
                CarModelId = model.CarModelId,
                Text = model.Text
            };

            await _dbContext.Comments.AddAsync(comment);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Detail), "Car", new { id = model.CarModelId });
        }

        //***** Renting *****//
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Renting(RentingViewModel model)
        {
            IdentityUser user = await _userManager.GetUserAsync(User);

            
            RentedCar rentedCar = new RentedCar
            {
                CarModelId = model.CarModelId,
                UserId = user.Id,
                StartDate = model.StartDate,
                EndDate = model.EndDate
            };

            await _dbContext.RentedCars.AddAsync(rentedCar);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Detail), nameof(Car), new { id = model.CarModelId });
        }

        //***** Search *****//
        public async Task<IActionResult> Search(CarSearchViewModel model)
        {
            var rentedCars = await _dbContext.RentedCars.ToListAsync();

            //var cars = await _dbContext.Cars.Where(c => !c.IsDeleted && c.BrandName == model.CarBrandName)
            //    .Include(c => c.CarModels)
            //    .ThenInclude(c => c.CarImages)
            //    .ToListAsync();

            var carModels = await _dbContext.CarModels.Where(c => !c.IsDeleted && c.ModelName == model.CarModelName 
            && c.District.City.Name == model.CityName && (c.CurrentPrice > model.MinPrice && c.CurrentPrice < model.MaxPrice))
                .Include(c => c.Car)
                .Include(c => c.District).ThenInclude(d => d.City)
                .Include(c => c.CarImages.Where(i => i.IsMain))
                .ToListAsync();

            //var city = await _dbContext.Cities.Include(c => c.Districts)
            //    .Where(c => c.Name == model.CityName).ToListAsync();

            List<CarModel> searchedCarList = new List<CarModel>();


            //foreach (var carModel in carModels)
            //{
            //    foreach (var rentedCar in rentedCars)
            //    {
            //        if (carModel.Id == rentedCar.CarModelId)
            //        {
            //            if ((model.StartDate < rentedCar.StartDate || model.StartDate > rentedCar.StartDate) &&
            //                (model.EndDate < rentedCar.EndDate || model.EndDate > rentedCar.EndDate))
            //            {
            //                searchedCarList.Add(carModel);
            //            }
            //        }
            //        searchedCarList.Add(carModel);
            //    }
                
            //}



            return View(carModels);

        }

    }
}
