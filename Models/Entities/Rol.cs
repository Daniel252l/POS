using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pos.Entities
{
    [Table("SeguridadRoles")]
    public class Rol
    {
        [Key]
        [MaxLength(20)]
        public string Id { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Nombre { get; set; } = string.Empty;

        public int NivelDeAccesoId { get; set; }

        public int CambioDeContrasenia { get; set; }

        public int TipoId { get; set; }

        public bool AplicarConfiguracionesPorRol { get; set; }

        public virtual ICollection<Usuario>? Usuarios { get; set; }
    }
}