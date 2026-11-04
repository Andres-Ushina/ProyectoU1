// COMENTARIO EXPLICATIVO: Datos del formulario de acceso y validaciones declarativas antes de comprobar las credenciales.
using System.ComponentModel.DataAnnotations;

namespace ProyectoU1.Models;

public sealed class LoginViewModel
{
    [Required(ErrorMessage = "Ingrese su nombre de usuario.")]
    [StringLength(50)]
    [Display(Name = "Usuario")]
    // Dato del modelo: nombre Usuario.
    public string NombreUsuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingrese su contraseña.")]
    [DataType(DataType.Password)]
    [StringLength(100)]
    [Display(Name = "Contraseña")]
    // Dato del modelo: contraseña.
    public string Password { get; set; } = string.Empty;
}
