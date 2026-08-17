namespace Pos.Models.DTOs
{
    public class LoginResponseDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string RoleId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime TokenExpiration { get; set; }
        public bool IsPasswordTemporary { get; set; } // ← Esta propiedad es la clave
        public List<MenuDto> Menus { get; set; } = new();
        public EmpresaDto Empresa { get; set; } = null!;
        public SucursalDto Sucursal { get; set; } = null!;
    }

    public class MenuDto
    {
        public string Id { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string LayoutItems { get; set; } = string.Empty;
    }

    public class EmpresaDto
    {
        public int Id { get; set; }
        public string RazonSocial { get; set; } = string.Empty;
        public string NombreComercial { get; set; } = string.Empty;
        public string Nit { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Imagen { get; set; } = string.Empty;
        public string Icono { get; set; } = string.Empty;
    }

    public class SucursalDto
    {
        public long Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool EsCentral { get; set; }
        public string Codigo { get; set; } = string.Empty;
    }
}