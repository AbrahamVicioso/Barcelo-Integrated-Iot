using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Reservas.Application.Interfaces;
using Reservas.Persistence.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reservas.Persistence.Repositories
{
    public class HuespedRepository : IHuespedRepository
    {
        private readonly BarceloReservasContext _context;

        public HuespedRepository(BarceloReservasContext barceloReservasContext)
        {
            this._context = barceloReservasContext;
        }

        public async Task<int?> GetHuespedIdByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            var conn = _context.Database.GetDbConnection();
            await conn.OpenAsync(cancellationToken);

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT H.HuespedId
                FROM Huespedes H
                WHERE H.UsuarioId = @userId
            ";
            cmd.Parameters.Add(new SqlParameter("@userId", userId));

            var result = await cmd.ExecuteScalarAsync(cancellationToken);
            return result != null ? Convert.ToInt32(result) : null;
        }

        public async Task<string> GetHuespedIdByEmail(int idHuesped)
        {
            var conn = _context.Database.GetDbConnection();
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT u.Email
                FROM Huespedes H
                INNER JOIN Users u ON H.UsuarioId = u.Id
                WHERE H.HuespedId = @huespedId
            ";
            cmd.Parameters.Add(new SqlParameter("@huespedId", idHuesped));

            var result = await cmd.ExecuteScalarAsync();
            return result?.ToString() ?? string.Empty;
        }
    }
}
