using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pos.Entities
{
    [Table("SeguridadUsuarios")]
    public class Usuario
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [MaxLength(128)]
        public string IdentityId { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public string? SecurityStamp { get; set; }

        [Required]
        [MaxLength(20)]
        public string RoleId { get; set; } = string.Empty;

        public int AccessFailedCount { get; set; } = 0;

        public TimeSpan HorarioInicio { get; set; }

        public TimeSpan HorarioFinal { get; set; }

        public DateOnly? UltimoCambioDeContrasenia { get; set; }

        [MaxLength(50)]
        public string Imagen { get; set; } = "default.jpg";

        [Required]
        [MaxLength(300)]
        public string Soundex { get; set; } = string.Empty;

        public bool ContraseniaTemporal { get; set; } = false;

        public int? AsociadoId { get; set; }

        public bool Estado { get; set; } = true;

        [MaxLength(50)]
        public string? PasswordTemporal { get; set; }

        public int? GrupoId { get; set; }

        public int TipoInicioSesionId { get; set; }

        public bool? AutorizacionCajero { get; set; }

        public int? AutenticacionCajeroId { get; set; }

        public bool? LimitarBusquedaPorFechas { get; set; }

        public int? RangoPorFechas { get; set; }

        public bool? UtilizarConfiguracionDeRole { get; set; }

        public bool MfaHabilitado { get; set; } = false;

        [MaxLength(500)]
        public string? ClaveHashMfa { get; set; }

        [MaxLength(200)]
        public string? CodigosRecuperacionMfa { get; set; }

        public int? EmpresaId { get; set; }

        [ForeignKey("RoleId")]
        public virtual Rol? Rol { get; set; }

        public virtual ICollection<Bitacora>? Bitacoras { get; set; }

        [MaxLength(500)]
        public string? ResetToken { get; set; }

        public DateTime? ResetTokenExpiration { get; set; }

        public string? PasswordHistory { get; set; } 
    }
}