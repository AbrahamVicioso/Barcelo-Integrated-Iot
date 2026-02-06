using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reservas.Application.Interfaces
{
    public interface IHuespedRepository
    {
        Task<int?> GetHuespedIdByUserIdAsync(string userId, CancellationToken cancellationToken = default);
        Task<string> GetHuespedIdByEmail(int idHuesped);
    }
}
