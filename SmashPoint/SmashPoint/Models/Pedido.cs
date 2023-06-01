using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace SmashPoint.Models
{
    public class Pedido
    {
        [Display(Name = "Núm. Pedido")]
        public int Id { get; set; }
                
        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }
        public DateTime Fecha { get; set; }

        public int EstadoActualId { get; set; }
        public Estado? EstadoActual { get; set; }

        public ICollection<Detalle>? Detalles { get; set; }
        public decimal PrecioTotal { get; set; } = 0;

        public int? DescuentoId { get; set; }
        public Descuento? Descuento { get; set; }
    }

}
