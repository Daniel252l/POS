using System.ComponentModel.DataAnnotations;

namespace Pos.Models.DTOs
{
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "El correo electrónico es requerido")]
        [EmailAddress(ErrorMessage = "Formato de correo electrónico inválido")]
        [MaxLength(100, ErrorMessage = "El correo no puede tener más de 100 caracteres")]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Recordarme")]
        public bool Recordar { get; set; } = false;
    }
}