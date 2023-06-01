using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace SmashPoint.Models
{
    public class Descuento
    {
        public int Id { get; set; }


        [DisplayFormat(DataFormatString = "{0:n2}")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Valor { get; set; }

        
        private string _valorCadena;

        [Display(Name = "ValorCadena")]
        [Required(ErrorMessage = "El precio es un campo requerido")]
        public string ValorCadena
        {
            get
            {
                return _valorCadena;
            }
            set
            {
                if (Valor==0)
                {
                    _valorCadena= value;
                    Valor = SacarDecimal(value);
                }
            }
        }

        public string Codigo { get; set; }

        public ICollection<Pedido>? Pedidos { get; set; }

        public decimal SacarDecimal(string value)
        {
            return Convert.ToDecimal((100-int.Parse(value))*0.01);
        }
    }

    
}
