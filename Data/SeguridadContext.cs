using Microsoft.EntityFrameworkCore;
using Pos.Entities;

namespace Pos.Data
{
    public class SeguridadContext : DbContext
    {
        public SeguridadContext(DbContextOptions<SeguridadContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Empresa> Empresas { get; set; }
        public DbSet<Bitacora> Bitacoras { get; set; }
        public DbSet<Tipo> Tipos { get; set; }
        public DbSet<NivelDeAcceso> NivelesDeAccesos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar Usuario
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.UserName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.RoleId).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Soundex).IsRequired().HasMaxLength(300);

                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.UserName).IsUnique();
                entity.HasIndex(e => e.IdentityId).IsUnique();

                entity.HasOne(e => e.Rol)
                    .WithMany(r => r.Usuarios)
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configurar Rol
            modelBuilder.Entity<Rol>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(50);

                entity.HasIndex(e => e.Nombre).IsUnique();
            });

            // Configurar Empresa
            modelBuilder.Entity<Empresa>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RazonSocial).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Nit).IsRequired().HasMaxLength(25);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.RoleId).IsRequired().HasMaxLength(20);

                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.Nit).IsUnique();
            });

            // Configurar Bitacora
            modelBuilder.Entity<Bitacora>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Descripcion).HasMaxLength(300);
                entity.Property(e => e.Evento).HasMaxLength(100);
                entity.Property(e => e.Tabla).HasMaxLength(50);
                entity.Property(e => e.Identificador).HasMaxLength(20);
            });

            // Configurar Tipo
            modelBuilder.Entity<Tipo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
            });

            // Configurar NivelDeAcceso
            modelBuilder.Entity<NivelDeAcceso>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
            });
        }
    }
}