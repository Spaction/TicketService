using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Website.Models;
using Microsoft.Extensions.Configuration;

namespace Website.Controllers
{
    public class MapController : Controller
    {
        private readonly IConfiguration _config;
        public MapController(IConfiguration _config)
        {
            this._config = _config;
        }
        public IActionResult Index(int id =1)
        { 
            ViewBag.ShowId = id;
            ViewBag.dataUrl = _config.GetSection("AppConfiguration")["dataUrl"];
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
