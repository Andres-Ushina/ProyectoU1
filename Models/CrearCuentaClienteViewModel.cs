// COMENTARIO EXPLICATIVO: Datos y reglas de validación para registrar una cuenta vinculada con un cliente Sakila.
using System.ComponentModel.DataAnnotations;

namespace ProyectoU1.Models;

public sealed class CrearCuentaClienteViewModel
{
    [Required, StringLength(50, MinimumLength = 3)]
    [Display(Name = "Nombre de usuario")]
    // Dato del modelo: nombre Usuario.
    public string NombreUsuario { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 10)]
    [DataType(DataType.Password)]
    // Dato del modelo: contraseña.
    public string Password { get; set; } = string.Empty;

    [Required, Compare(nameof(Password))]
    [DataType(DataType.Password)]
    [Display(Name = "Confirmar contraseña")]
    // Dato del modelo: Confirmar contraseña.
    public string ConfirmarPassword { get; set; } = string.Empty;

    [Range(1, 32767)]
    [Display(Name = "ID del cliente Sakila")]
    // Dato del modelo: cliente  identificador.
    public int CustomerId { get; set; }
}
