using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmpresaProyecto.Core.Entities
{
    public class Suscripcion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long IdSuscripcion {  get; set; }
        [Required]
        public string IdCliente { get; set; }

        [Required]
        public DateTime FechaCreacion { get; set; }
        
        public DateTime? FechaPago { get; set; }

        [Required]
        public DateTime UltimaFechaModificacion { get; set; }

        [Required]
        public string Estado { get; set; }
        [Required]
        public string Plan { get; set; }
    }

}
