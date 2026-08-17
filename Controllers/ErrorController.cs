using Microsoft.AspNetCore.Mvc;

namespace Pos.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}