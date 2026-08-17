using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pos.Helpers;
using Pos.Models.DTOs;
using Pos.Services;
using Pos.Services.Interfaces;
using System.Security.Claims;

namespace Pos.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILoginService _loginService;
        private readonly IEmailService _emailService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            ILoginService loginService,
            IEmailService emailService,
            ILogger<AccountController> logger)
        {
            _loginService = loginService;
            _emailService = emailService;
            _logger = logger;
        }

        // ============================================
        // LOGIN
        // ============================================
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequestDto request, string? returnUrl = null)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(request);
                }

                var response = await _loginService.LoginAsync(request);

                if (response.Empresa == null)
                {
                    _logger.LogError($"Empresa es null para el usuario: {response.Email}");
                    ModelState.AddModelError("", "Error al obtener información de la empresa.");
                    return View(request);
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, response.Id.ToString()),
                    new Claim(ClaimTypes.Email, response.Email),
                    new Claim(ClaimTypes.Name, response.UserName),
                    new Claim(ClaimTypes.Role, response.RoleName),
                    new Claim("RoleId", response.RoleId),
                    new Claim("EmpresaId", response.Empresa.Id.ToString()),
                    new Claim("IsPasswordTemporary", response.IsPasswordTemporary.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = request.Recordar,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                HttpContext.Session.SetString("Token", response.Token);
                HttpContext.Session.SetString("UserEmail", response.Email);
                HttpContext.Session.SetInt32("UserId", response.Id);

                // 🔴 SI LA CONTRASEÑA ES TEMPORAL, REDIRIGIR A CAMBIAR CONTRASEÑA
                if (response.IsPasswordTemporary)
                {
                    _logger.LogInformation($"🔴 Usuario {response.Email} tiene contraseña temporal. Redirigiendo a cambio.");
                    return RedirectToAction("CambiarPassword", new { esTemporal = true });
                }

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }
            catch (UnauthorizedAccessException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(request);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error en login: {ex.Message}");
                ModelState.AddModelError("", "Ocurrió un error al iniciar sesión.");
                return View(request);
            }
        }

        // ============================================
        // CAMBIAR CONTRASEÑA
        // ============================================
        [HttpGet]
        public IActionResult CambiarPassword(bool esTemporal = false)
        {
            var model = new CambiarPasswordViewModel
            {
                EsTemporal = esTemporal
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarPassword(CambiarPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login");
                }

                var usuario = await _loginService.GetUserByEmailAsync(User.FindFirstValue(ClaimTypes.Email)!);
                if (usuario == null)
                {
                    ModelState.AddModelError("", "Usuario no encontrado");
                    return View(model);
                }

                // Verificar contraseña actual
                if (!PasswordHelper.VerifyPassword(model.PasswordActual, usuario.PasswordHash))
                {
                    ModelState.AddModelError("PasswordActual", "La contraseña actual es incorrecta");
                    return View(model);
                }

                // Verificar que la nueva contraseña no sea similar a la anterior (Soundex)
                if (SoundexHelper.IsSimilarPassword(model.NuevaPassword, usuario.PasswordHash))
                {
                    ModelState.AddModelError("NuevaPassword", "La nueva contraseña no puede ser similar a la anterior");
                    return View(model);
                }

                // 🔴 ACTUALIZAR CONTRASEÑA Y DESACTIVAR MODO TEMPORAL
                usuario.PasswordHash = PasswordHelper.HashPassword(model.NuevaPassword);
                usuario.ContraseniaTemporal = false; // ← Esto desactiva la contraseña temporal
                usuario.UltimoCambioDeContrasenia = DateOnly.FromDateTime(DateTime.Now);

                await _loginService.UpdatePasswordAsync(usuario);

                // Enviar notificación por email
                await _emailService.SendPasswordChangedNotificationAsync(usuario.Email, usuario.UserName);

                TempData["SuccessMessage"] = "✅ Contraseña actualizada correctamente. Ahora puedes acceder al sistema.";

                if (model.EsTemporal)
                {
                    return RedirectToAction("Index", "Home");
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al cambiar contraseña: {ex.Message}");
                ModelState.AddModelError("", "Ocurrió un error al cambiar la contraseña");
                return View(model);
            }
        }

        // ============================================
        // RECUPERAR CONTRASEÑA
        // ============================================
        [HttpGet]
        [AllowAnonymous]
        public IActionResult RecuperarPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecuperarPassword(RecuperarPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var usuario = await _loginService.GetUserByEmailAsync(model.Email);
                if (usuario == null)
                {
                    TempData["SuccessMessage"] = "Si el correo existe, recibirás instrucciones para restablecer tu contraseña.";
                    return RedirectToAction("Login");
                }

                var tokenGenerated = await _loginService.GeneratePasswordResetTokenAsync(model.Email);
                if (!tokenGenerated)
                {
                    TempData["ErrorMessage"] = "Error al generar el token de recuperación.";
                    return View(model);
                }

                var emailSent = await _emailService.SendPasswordResetEmailAsync(
                    model.Email,
                    usuario.UserName ?? "Usuario",
                    usuario.ResetToken ?? string.Empty
                );

                if (emailSent)
                {
                    TempData["SuccessMessage"] = "Se han enviado instrucciones a tu correo electrónico.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Error al enviar el correo. Intenta nuevamente.";
                }

                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error en recuperación de contraseña: {ex.Message}");
                ModelState.AddModelError("", "Ocurrió un error. Intenta nuevamente.");
                return View(model);
            }
        }

        // ============================================
        // RESTABLECER CONTRASEÑA (con token)
        // ============================================
        [HttpGet]
        [AllowAnonymous]
        public IActionResult RestablecerPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "Token inválido.";
                return RedirectToAction("Login");
            }

            var model = new RestablecerPasswordViewModel
            {
                Token = token,
                Email = email
            };

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestablecerPassword(RestablecerPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var isValid = await _loginService.ValidateResetTokenAsync(model.Email, model.Token);
                if (!isValid)
                {
                    ModelState.AddModelError("", "El token es inválido o ha expirado.");
                    return View(model);
                }

                var result = await _loginService.ResetPasswordWithTokenAsync(
                    model.Email,
                    model.Token,
                    model.NuevaPassword
                );

                if (result)
                {
                    await _emailService.SendPasswordChangedNotificationAsync(model.Email, model.Email);

                    TempData["SuccessMessage"] = "✅ Contraseña restablecida exitosamente. Inicia sesión con tu nueva contraseña.";
                    return RedirectToAction("Login");
                }
                else
                {
                    ModelState.AddModelError("", "No se pudo restablecer la contraseña. La nueva contraseña no puede ser similar a las anteriores.");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al restablecer contraseña: {ex.Message}");
                ModelState.AddModelError("", "Ocurrió un error. Intenta nuevamente.");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userId))
                {
                    await _loginService.LogoutAsync(int.Parse(userId));
                }

                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                HttpContext.Session.Clear();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error en logout: {ex.Message}");
            }

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}