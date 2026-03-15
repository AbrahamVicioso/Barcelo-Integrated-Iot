using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reservas.Domain.Entities
{
    public class Habitacion
    {
        public int HabitacionId { get; set; }

        public int HotelId { get; set; }

        public string NumeroHabitacion { get; set; } = string.Empty;

        public string TipoHabitacion { get; set; } = string.Empty;

        public int Piso { get; set; }

        public int CapacidadMaxima { get; set; }

        public decimal PrecioPorNoche { get; set; }

        public int EstadoHabitacionId { get; set; }

        public string? Descripcion { get; set; }

        public DateTime FechaCreacion { get; set; }

        public DateTime? FechaActualizacion { get; set; }

        // Navigation properties
        public Hotel? Hotel { get; set; }

        public EstadoHabitacion? EstadoHabitacion { get; set; }
    }
}
