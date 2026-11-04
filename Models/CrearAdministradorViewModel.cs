// COMENTARIO EXPLICATIVO: Datos del formulario de configuracion inicial del administrador con validaciones de usuario y contraseña.
using System.ComponentModel.DataAnnotations;

namespace ProyectoU1.Models;

public sealed class CrearAdministradorViewModel
{
    [Required, StringLength(50, MinimumLength = 3)]
    [Display(Name = "Usuario administrador")]
    // Dato del modelo: nombre Usuario.
    public string NombreUsuario { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 10)]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña (mínimo 10 caracteres)")]
    // Dato del modelo: contraseña.
    public string Password { get; set; } = string.Empty;

    [Required, Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirmar contraseña")]
    // Dato del modelo: Confirmar contraseña.
    public string ConfirmarPassword { get; set; } = string.Empty;

    [Range(1, 255)]
    [Display(Name = "ID de empleado Sakila")]
    // Dato del modelo: empleado  identificador.
    public int StaffId { get; set; } = 1;
}
