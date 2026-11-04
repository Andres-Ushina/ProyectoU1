// COMENTARIO EXPLICATIVO: Representa una cuenta autenticable, el hash de contraseña, su rol y su relación con Sakila.
namespace ProyectoU1.Models;

public sealed class UsuarioLogin
{
    // Dato del modelo: Usuario identificador.
    public int UsuarioId { get; set; }
    // Dato del modelo: nombre Usuario.
    public string NombreUsuario { get; set; } = string.Empty;
    // Dato del modelo: contraseña Hash.
    public string PasswordHash { get; set; } = string.Empty;
    // Dato del modelo: Rol.
    public string Rol { get; set; } = string.Empty;
    // Dato del modelo: cliente  identificador.
    public int? CustomerId { get; set; }
    // Dato del modelo: empleado  identificador.
    public int? StaffId { get; set; }
}
