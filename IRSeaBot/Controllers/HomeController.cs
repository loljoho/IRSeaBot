using IRSeaBot.Models;
using IRSeaBot.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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

        [HttpPost]
        public IActionResult StartBot(string config)
        {
            BotConfiguration configuration = JsonConvert.DeserializeObject<BotConfiguration>(config);
            Guid botID = configuration.Id;
            _botContainer.StartBot(configuration);
            string rtn = JsonConvert.SerializeObject(configuration);
            return Ok(rtn);
        }

        [HttpGet]
        public IActionResult GetBotList()
        {
            List<IRCBot> bots = _botContainer.GetBotList();
            List<BotConfiguration> configurations = bots.Select(x => x.getConfig()).ToList();
            string rtn = JsonConvert.SerializeObject(configurations);
            return Ok(rtn);

        }

        [HttpGet]
        public IActionResult Kill([FromQuery] string id)
        {
            Guid guid = Guid.Parse(id);
            _botContainer.StopBot(guid);
            return Ok(guid);
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
