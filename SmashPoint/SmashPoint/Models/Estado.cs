using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace SmashPoint.Models
{
    public class Estado
    {
        public int Id { get; set; }
        [Display(Name = "Descripción")]
        [Required(ErrorMessage = "La descripción es un campo requerido.")]
        public string? Descripcion { get; set; }
        public ICollection<EstadoDetalle>? EstadoDetalles { get; set; }
    }
}
