using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebTestPlaywright.Models;

namespace WebTestPlaywright.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Form()
        {
            HomeFormViewModel model = new HomeFormViewModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult Form(HomeFormModel Form)
        {
            if(Form.BirthDate.HasValue && Form.BirthDate.Value.Date >= DateTime.Today)
            {
                ModelState.AddModelError("Form.BirthDate", "The birth date is invalid.");
            }
            if(!ModelState.IsValid)
            {
                HomeFormViewModel model = new HomeFormViewModel();
                model.Form = Form;
                return View(model);
            }

            TempData["FullName"] = string.Format("{0} {1}", Form.FirstName, Form.LastName);
            return RedirectToAction("Success");
        }

        public IActionResult Success()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}