using Reservas.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reservas.Domain.Interfaces
{
    public interface IHotelRepository
    {
        Task<IEnumerable<Hotel>> getAll();
    }
}
