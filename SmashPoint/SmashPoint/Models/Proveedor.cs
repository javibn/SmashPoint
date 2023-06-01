using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace SmashPoint.Models
{
    public class Proveedor
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }
        public string Correo { get; set; }

        public string? Imagen { get; set; }

        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public ICollection<Producto>? Productos { get; set; }
    }
}
