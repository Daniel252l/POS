using System.ComponentModel.DataAnnotations;

namespace Pos.Models.DTOs
{
    public class RecuperarPasswordViewModel
    {
        [Required(ErrorMessage = "El correo electrónico es requerido")]
        [EmailAddress(ErrorMessage = "Formato de correo electrónico inválido")]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; } = string.Empty;
    }
}