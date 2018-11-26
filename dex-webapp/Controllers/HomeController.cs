using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using dex_webapp.Data;
using Microsoft.AspNetCore.Mvc;
using dex_webapp.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace dex_webapp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string tokenSymbol)
        {
            TokenModel token;
            if (string.IsNullOrWhiteSpace(tokenSymbol))
                token = await _context.Token.FirstOrDefaultAsync();
            else
                token = await _context.Token.SingleOrDefaultAsync(_ => _.Symbol == tokenSymbol);
            //if (token == null) RedirectToAction("Index");
            if (token == null) token = new TokenModel();
            return View(token);
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
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
