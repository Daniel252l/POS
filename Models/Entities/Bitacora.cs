using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pos.Entities
{
    [Table("SeguridadBitacoras")]
    public class Bitacora
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public int? EmpresaId { get; set; }

        public long? SucursalId { get; set; }

        [MaxLength(20)]
        public string? ModuloId { get; set; }

        [MaxLength(50)]
        public string? Tabla { get; set; }

        [MaxLength(300)]
        public string? Descripcion { get; set; }

        [MaxLength(100)]
        public string? Evento { get; set; }

        public DateTime Fecha { get; set; }

        public int? UsuarioId { get; set; }

        public string? DatosDeRespaldo { get; set; }

        [MaxLength(20)]
        public string? Identificador { get; set; }

        public bool TipoIdentificador { get; set; }

        [ForeignKey("UsuarioId")]
        public virtual Usuario? Usuario { get; set; }
    }
}