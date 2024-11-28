using DLL.Models;
using Microsoft.AspNetCore.Mvc;

namespace PRL_Web.Controllers
{
    public class ProductsController : Controller
    {
        private readonly BanHangDbContext _context;

        public ProductsController(BanHangDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
