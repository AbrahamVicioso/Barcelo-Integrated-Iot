using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reservas.Application.DTOs
{
    public class HotelesDto
    {
        public int HotelId { get; set; }

        public string Nombre { get; set; }

        public string Direccion { get; set; }

        public string Ciudad { get; set; }

        public string Pais { get; set; }

        public string Telefono { get; set; }

        public string Email { get; set; }

        public bool EstaActivo { get; set; }

        public DateTime FechaCreacion { get; set; }

        public int NumeroHabitaciones { get; set; }

        public int NumeroEstrellas { get; set; }
    }
}
