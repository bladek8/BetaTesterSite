using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace BetaTesterSite.Controllers
{
    public class HistoryController : Controller
    {
        public IActionResult Index(int? typeId)
        {
            return View(typeId);
        }
    }
}