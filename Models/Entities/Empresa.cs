using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pos.Entities
{
    [Table("SeguridadEmpresas")]
    public class Empresa
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string RazonSocial { get; set; } = string.Empty;

        [MaxLength(150)]
        public string Eslogan { get; set; } = string.Empty;

        [MaxLength(250)]
        public string Direccion { get; set; } = string.Empty;

        [Required]
        [MaxLength(25)]
        public string Nit { get; set; } = string.Empty;

        [MaxLength(25)]
        public string? Dpi { get; set; }

        [MaxLength(100)]
        public string Telefono { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Imagen { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Icono { get; set; } = string.Empty;

        public bool Estado { get; set; } = true;

        [MaxLength(100)]
        public string ImagenReportes { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string RoleId { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? CodigoIntegriax { get; set; }

        public int? ClienteId { get; set; }

        [MaxLength(150)]
        public string? NombreComercial { get; set; }

        [MaxLength(150)]
        public string? Url { get; set; }
    }
}