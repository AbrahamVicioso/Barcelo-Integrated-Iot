using Microsoft.EntityFrameworkCore;
using Reservas.Domain.Entities;
using Reservas.Domain.Interfaces;
using Reservas.Persistence.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reservas.Persistence.Repositories
{
    public class HotelRepository : IHotelRepository
    {
        private readonly BarceloReservasContext _context;

        public HotelRepository(BarceloReservasContext context)
        {
            this._context = context;
        }

        public async Task<IEnumerable<Hotel>> getAll()
        {
            return await _context.Hoteles.ToListAsync();
        }
    }
}
