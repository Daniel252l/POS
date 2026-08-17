using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pos.Entities
{
    [Table("SeguridadTipos")]
    public class Tipo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        public bool Estado { get; set; }

        [MaxLength(20)]
        public string? ModuloId { get; set; }

        public int ClaseTipoId { get; set; }
    }
}