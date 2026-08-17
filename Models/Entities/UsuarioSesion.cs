using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pos.Entities
{
    [Table("SeguridadUsuarioSessions")]
    public class UsuarioSession
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [Required]
        [MaxLength(500)]
        public string Token { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string RefreshToken { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; }

        public DateTime FechaExpiracion { get; set; }

        [MaxLength(50)]
        public string? IPAddress { get; set; }

        [MaxLength(200)]
        public string? UserAgent { get; set; }

        public bool Activo { get; set; } = true;

        // Navigation property
        [ForeignKey("UsuarioId")]
        public virtual Usuario? Usuario { get; set; }
    }
}