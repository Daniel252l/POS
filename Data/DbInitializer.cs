using Microsoft.EntityFrameworkCore;
using Pos.Entities;
using Pos.Helpers;

namespace Pos.Data
{
    public class DbInitializer
    {
        private readonly SeguridadContext _context;
        private readonly ILogger<DbInitializer> _logger;

        public DbInitializer(SeguridadContext context, ILogger<DbInitializer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            try
            {
                _logger.LogInformation("Iniciando verificación de datos iniciales...");

                // Verificar y crear Roles si no existen
                if (!await _context.Roles.AnyAsync())
                {
                    _logger.LogInformation("Creando roles...");

                    // Primero verificar que existan los tipos necesarios
                    var tipoDefault = await _context.Tipos.FirstOrDefaultAsync(t => t.Id == 1);
                    var nivelAdmin = await _context.NivelesDeAccesos.FirstOrDefaultAsync(n => n.Id == 1);

                    if (tipoDefault == null || nivelAdmin == null)
                    {
                        _logger.LogWarning("No se encontraron datos base en SeguridadTipos o SeguridadNivelesDeAccesos");
                        return;
                    }

                    var adminRole = new Rol
                    {
                        Id = "ADMIN",
                        Nombre = "Administrador",
                        NivelDeAccesoId = 1,
                        CambioDeContrasenia = 30,
                        TipoId = 1,
                        AplicarConfiguracionesPorRol = true
                    };

                    await _context.Roles.AddAsync(adminRole);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("✅ Roles creados correctamente");
                }
                else
                {
                    _logger.LogInformation("✅ Los roles ya existen");
                }

                // Verificar y crear Empresa si no existe
                if (!await _context.Empresas.AnyAsync())
                {
                    _logger.LogInformation("Creando empresa por defecto...");

                    var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Id == "ADMIN");
                    if (adminRole == null)
                    {
                        _logger.LogWarning("No se encontró el rol ADMIN para crear la empresa");
                        return;
                    }

                    var empresa = new Empresa
                    {
                        RazonSocial = "POS System S.A.",
                        Eslogan = "Soluciones POS",
                        Direccion = "Ciudad, Guatemala",
                        Nit = "123456789",
                        Telefono = "12345678",
                        Email = "info@pos.com",
                        Imagen = "logo.png",
                        Icono = "icon.ico",
                        Estado = true,
                        ImagenReportes = "report.png",
                        RoleId = "ADMIN",
                        NombreComercial = "POS System"
                    };

                    await _context.Empresas.AddAsync(empresa);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("✅ Empresa creada correctamente");
                }
                else
                {
                    _logger.LogInformation("✅ La empresa ya existe");
                }

                // Verificar y crear Usuario Admin si no existe
                if (!await _context.Usuarios.AnyAsync())
                {
                    _logger.LogInformation("Creando usuario administrador...");

                    var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Id == "ADMIN");
                    if (adminRole == null)
                    {
                        _logger.LogWarning("No se encontró el rol ADMIN para crear el usuario");
                        return;
                    }

                    // Verificar que el TipoInicioSesionId existe
                    var tipoInicio = await _context.Tipos.FirstOrDefaultAsync(t => t.Id == 1);
                    if (tipoInicio == null)
                    {
                        _logger.LogWarning("No se encontró el Tipo con Id=1 para el usuario");
                        return;
                    }

                    var adminUser = new Usuario
                    {
                        Email = "admin@pos.com",
                        UserName = "admin",
                        IdentityId = "admin_identity_" + Guid.NewGuid().ToString().Substring(0, 8),
                        PasswordHash = PasswordHelper.HashPassword("Admin123!"),
                        RoleId = "ADMIN",
                        AccessFailedCount = 0,
                        HorarioInicio = new TimeSpan(8, 0, 0),
                        HorarioFinal = new TimeSpan(18, 0, 0),
                        Imagen = "admin.jpg",
                        Soundex = "A",
                        ContraseniaTemporal = false,
                        Estado = true,
                        TipoInicioSesionId = 1,
                        MfaHabilitado = false
                    };

                    await _context.Usuarios.AddAsync(adminUser);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("✅ Usuario administrador creado correctamente");
                    _logger.LogInformation("   Email: admin@pos.com");
                    _logger.LogInformation("   Contraseña: Admin123!");
                }
                else
                {
                    _logger.LogInformation("✅ El usuario administrador ya existe");
                }

                _logger.LogInformation("🎉 Inicialización de datos completada exitosamente");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"❌ Error de base de datos: {ex.Message}");
                if (ex.InnerException != null)
                {
                    _logger.LogError($"   Detalle: {ex.InnerException.Message}");
                }
                // No lanzamos la excepción para que la aplicación continúe
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error general: {ex.Message}");
                // No lanzamos la excepción para que la aplicación continúe
            }
        }
    }
}