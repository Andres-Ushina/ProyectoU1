// COMENTARIO EXPLICATIVO: Datos que la vista de error utiliza para mostrar un identificador de solicitud.
namespace ProyectoU1.Models
{
    public class ErrorViewModel
    {
        // Dato del modelo: Request identificador.
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
