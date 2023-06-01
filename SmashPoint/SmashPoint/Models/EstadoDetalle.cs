using System.ComponentModel.DataAnnotations;

namespace SmashPoint.Models
{
    public class EstadoDetalle
    {
        public int Id { get; set; }


        [Display(Name = "Fecha")]
        public DateTime Fecha { get; set; }



        [Display(Name = "Id.Detalle")]
        public int DetalleId { get; set; }
        public Detalle? Detalle { get; set; }

        [Display(Name = "Id.Estado")]
        public int EstadoId { get; set; }
        public Estado? Estado { get; set; }

    }
}
