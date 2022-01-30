using IRSeaBot.Models;
using IRSeaBot.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace IRSeaBot.Controllers
{
    public class HomeController : Controller
    {
        private BotContainer _botContainer;

        public HomeController(BotContainer botContainer)
        {
            _botContainer = botContainer;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Restart([FromQuery] string nick)
        {
            Settings.nick = nick;
            await _botContainer.Restart();
            return Index();
        }

        public IActionResult Privacy()
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
