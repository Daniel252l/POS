using System.ComponentModel.DataAnnotations;

namespace Pos.Models.DTOs
{
    public class CambiarPasswordViewModel
    {
        [Required(ErrorMessage = "La contraseña actual es requerida")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña Actual")]
        public string PasswordActual { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nueva contraseña es requerida")]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        [Display(Name = "Nueva Contraseña")]
        public string NuevaPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "La confirmación de contraseña es requerida")]
        [DataType(DataType.Password)]
        [Compare("NuevaPassword", ErrorMessage = "Las contraseñas no coinciden")]
        [Display(Name = "Confirmar Contraseña")]
        public string ConfirmarPassword { get; set; } = string.Empty;

        public bool EsTemporal { get; set; } = false;
    }
}