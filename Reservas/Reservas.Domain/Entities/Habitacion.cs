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

        public string Estado { get; set; } = string.Empty;

        public bool EstaDisponible { get; set; }

        public string? Descripcion { get; set; }

        public DateTime FechaCreacion { get; set; }

        public DateTime? FechaActualizacion { get; set; }

        // Navigation property
        public Hotel? Hotel { get; set; }
    }
}
