using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pos.Helpers;
using Pos.Models.DTOs;
using System.Diagnostics;

namespace Pos.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Pos.Data.SeguridadContext _context;

        public HomeController(ILogger<HomeController> logger, Pos.Data.SeguridadContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> TestHash()
        {
            var results = new List<string>();
            results.Add("=== PRUEBA DE HASH ===\n");

            var passwords = new[] { "Admin123!", "123456", "password", "admin" };

            foreach (var pwd in passwords)
            {
                var hash = PasswordHelper.HashPassword(pwd);
                results.Add($"Contraseña: '{pwd}'");
                results.Add($"Hash: {hash}");
                results.Add($"Longitud: {hash.Length}\n");
            }

            var user = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == "soporte@pos.com");

            if (user != null)
            {
                results.Add($"\n=== USUARIO ENCONTRADO ===");
                results.Add($"Email: {user.Email}");
                results.Add($"Hash en BD: {user.PasswordHash}");
                results.Add($"Longitud hash BD: {user.PasswordHash?.Length ?? 0}\n");

                results.Add("=== VERIFICANDO CONTRASEÑAS ===");
                foreach (var pwd in passwords)
                {
                    var hash = PasswordHelper.HashPassword(pwd);
                    var coincide = hash == user.PasswordHash;
                    results.Add($"'{pwd}' coincide? {(coincide ? "✅ SI" : "❌ NO")}");
                    if (!coincide)
                    {
                        results.Add($"  Hash calculado: {hash}");
                        results.Add($"  Hash en BD:     {user.PasswordHash}");
                        results.Add($"  Diferencia:     {hash != user.PasswordHash}");
                    }
                }
            }
            else
            {
                results.Add("\n❌ Usuario soporte@pos.com NO encontrado en la BD");
            }

            results.Add("\n=== USUARIOS EN BD ===");
            var users = await _context.Usuarios.ToListAsync();
            foreach (var u in users)
            {
                results.Add($"Email: {u.Email}, RoleId: {u.RoleId}, Estado: {u.Estado}");
            }

            return Ok(string.Join("\n", results));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}