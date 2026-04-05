using Audit.Worker.Models;
using Microsoft.Data.SqlClient;

namespace Audit.Worker;

public interface IAuditQueryRepository
{
    Task<PagedResult<AuditRegistroDto>> GetPagedAsync(AuditQueryParams query, CancellationToken ct = default);
    Task<AuditFiltersDto> GetFiltersAsync(CancellationToken ct = default);
}

public class AuditQueryRepository : IAuditQueryRepository
{
    private readonly string _connectionString;

    public AuditQueryRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<PagedResult<AuditRegistroDto>> GetPagedAsync(AuditQueryParams query, CancellationToken ct = default)
    {
        var (whereSql, parameters) = BuildWhere(query);
        int offset = (query.Page - 1) * query.PageSize;

        const string dataTemplate = """
            SELECT
                r.AuditoriaId,
                r.UsuarioId,
                u.UserName   AS NombreUsuario,
                u.Email      AS EmailUsuario,
                r.Accion,
                r.TipoEntidad,
                r.EntidadId,
                r.ValorAnterior,
                r.ValorNuevo,
                r.FechaHora,
                r.DireccionIP,
                r.AgenteUsuario,
                r.Resultado,
                r.MensajeError,
                r.HotelId
            FROM RegistrosAuditoria r
            LEFT JOIN Users u ON r.UsuarioId = u.Id
            {0}
            ORDER BY r.FechaHora DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        const string countTemplate = """
            SELECT COUNT(*)
            FROM RegistrosAuditoria r
            LEFT JOIN Users u ON r.UsuarioId = u.Id
            {0}
            """;

        var dataSql = string.Format(dataTemplate, whereSql);
        var countSql = string.Format(countTemplate, whereSql);

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(ct);

        // Total count
        await using var countCmd = new SqlCommand(countSql, connection);
        AddParameters(countCmd, parameters);
        var totalCount = (int)(await countCmd.ExecuteScalarAsync(ct) ?? 0);

        // Paged data
        var items = new List<AuditRegistroDto>(query.PageSize);
        await using var dataCmd = new SqlCommand(dataSql, connection);
        AddParameters(dataCmd, parameters);
        dataCmd.Parameters.AddWithValue("@Offset", offset);
        dataCmd.Parameters.AddWithValue("@PageSize", query.PageSize);

        await using var reader = await dataCmd.ExecuteReaderAsync(ct);

        // Cache ordinals once
        int ordAuditoriaId   = reader.GetOrdinal("AuditoriaId");
        int ordUsuarioId     = reader.GetOrdinal("UsuarioId");
        int ordNombreUsuario = reader.GetOrdinal("NombreUsuario");
        int ordEmailUsuario  = reader.GetOrdinal("EmailUsuario");
        int ordAccion        = reader.GetOrdinal("Accion");
        int ordTipoEntidad   = reader.GetOrdinal("TipoEntidad");
        int ordEntidadId     = reader.GetOrdinal("EntidadId");
        int ordValorAnterior = reader.GetOrdinal("ValorAnterior");
        int ordValorNuevo    = reader.GetOrdinal("ValorNuevo");
        int ordFechaHora     = reader.GetOrdinal("FechaHora");
        int ordDireccionIP   = reader.GetOrdinal("DireccionIP");
        int ordAgenteUsuario = reader.GetOrdinal("AgenteUsuario");
        int ordResultado     = reader.GetOrdinal("Resultado");
        int ordMensajeError  = reader.GetOrdinal("MensajeError");
        int ordHotelId       = reader.GetOrdinal("HotelId");

        while (await reader.ReadAsync(ct))
        {
            items.Add(new AuditRegistroDto(
                AuditoriaId:   reader.GetInt64(ordAuditoriaId),
                UsuarioId:     reader.IsDBNull(ordUsuarioId)     ? null : reader.GetString(ordUsuarioId),
                NombreUsuario: reader.IsDBNull(ordNombreUsuario) ? null : reader.GetString(ordNombreUsuario),
                EmailUsuario:  reader.IsDBNull(ordEmailUsuario)  ? null : reader.GetString(ordEmailUsuario),
                Accion:        reader.GetString(ordAccion),
                TipoEntidad:   reader.GetString(ordTipoEntidad),
                EntidadId:     reader.IsDBNull(ordEntidadId)     ? null : reader.GetInt32(ordEntidadId),
                ValorAnterior: reader.IsDBNull(ordValorAnterior) ? null : reader.GetString(ordValorAnterior),
                ValorNuevo:    reader.IsDBNull(ordValorNuevo)    ? null : reader.GetString(ordValorNuevo),
                FechaHora:     reader.GetDateTime(ordFechaHora),
                DireccionIp:   reader.IsDBNull(ordDireccionIP)   ? null : reader.GetString(ordDireccionIP),
                AgenteUsuario: reader.IsDBNull(ordAgenteUsuario) ? null : reader.GetString(ordAgenteUsuario),
                Resultado:     reader.GetString(ordResultado),
                MensajeError:  reader.IsDBNull(ordMensajeError)  ? null : reader.GetString(ordMensajeError),
                HotelId:       reader.IsDBNull(ordHotelId)       ? null : reader.GetInt32(ordHotelId)
            ));
        }

        return new PagedResult<AuditRegistroDto>(items, query.Page, query.PageSize, totalCount);
    }

    private static (string Sql, List<(string Name, object? Value)> Params) BuildWhere(AuditQueryParams query)
    {
        var conditions = new List<string>();
        var parameters = new List<(string, object?)>();

        if (!string.IsNullOrWhiteSpace(query.UsuarioId))
        {
            conditions.Add("r.UsuarioId = @UsuarioId");
            parameters.Add(("@UsuarioId", query.UsuarioId));
        }
        if (!string.IsNullOrWhiteSpace(query.TipoEntidad))
        {
            conditions.Add("r.TipoEntidad = @TipoEntidad");
            parameters.Add(("@TipoEntidad", query.TipoEntidad));
        }
        if (!string.IsNullOrWhiteSpace(query.Accion))
        {
            conditions.Add("r.Accion LIKE @Accion");
            parameters.Add(("@Accion", $"%{query.Accion}%"));
        }
        if (!string.IsNullOrWhiteSpace(query.Resultado))
        {
            conditions.Add("r.Resultado = @Resultado");
            parameters.Add(("@Resultado", query.Resultado));
        }
        if (query.Desde.HasValue)
        {
            conditions.Add("r.FechaHora >= @Desde");
            parameters.Add(("@Desde", query.Desde.Value.ToUniversalTime()));
        }
        if (query.Hasta.HasValue)
        {
            conditions.Add("r.FechaHora <= @Hasta");
            parameters.Add(("@Hasta", query.Hasta.Value.ToUniversalTime()));
        }
        if (query.HotelId.HasValue)
        {
            conditions.Add("r.HotelId = @HotelId");
            parameters.Add(("@HotelId", query.HotelId.Value));
        }

        var whereSql = conditions.Count > 0
            ? "WHERE " + string.Join(" AND ", conditions)
            : string.Empty;

        return (whereSql, parameters);
    }

    private static void AddParameters(SqlCommand cmd, List<(string Name, object? Value)> parameters)
    {
        foreach (var (name, value) in parameters)
            cmd.Parameters.AddWithValue(name, value ?? DBNull.Value);
    }

    public async Task<AuditFiltersDto> GetFiltersAsync(CancellationToken ct = default)
    {
        const string sql = """
            SELECT DISTINCT TipoEntidad FROM RegistrosAuditoria
            WHERE TipoEntidad IS NOT NULL AND TipoEntidad <> ''
            ORDER BY TipoEntidad;

            SELECT DISTINCT Accion FROM RegistrosAuditoria
            WHERE Accion IS NOT NULL AND Accion <> ''
            ORDER BY Accion;

            SELECT DISTINCT Resultado FROM RegistrosAuditoria
            WHERE Resultado IS NOT NULL AND Resultado <> ''
            ORDER BY Resultado;

            SELECT DISTINCT HotelId FROM RegistrosAuditoria
            WHERE HotelId IS NOT NULL
            ORDER BY HotelId;

            SELECT DISTINCT r.UsuarioId, u.UserName, u.Email
            FROM RegistrosAuditoria r
            LEFT JOIN Users u ON r.UsuarioId = u.Id
            WHERE r.UsuarioId IS NOT NULL
            ORDER BY u.UserName;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, connection);
        await using var reader = await cmd.ExecuteReaderAsync(ct);

        var tiposEntidad = new List<string>();
        while (await reader.ReadAsync(ct))
            tiposEntidad.Add(reader.GetString(0));

        await reader.NextResultAsync(ct);
        var acciones = new List<string>();
        while (await reader.ReadAsync(ct))
            acciones.Add(reader.GetString(0));

        await reader.NextResultAsync(ct);
        var resultados = new List<string>();
        while (await reader.ReadAsync(ct))
            resultados.Add(reader.GetString(0));

        await reader.NextResultAsync(ct);
        var hotelIds = new List<int>();
        while (await reader.ReadAsync(ct))
            hotelIds.Add(reader.GetInt32(0));

        await reader.NextResultAsync(ct);
        var usuarios = new List<UsuarioFiltroDto>();
        while (await reader.ReadAsync(ct))
            usuarios.Add(new UsuarioFiltroDto(
                reader.GetString(0),
                reader.IsDBNull(1) ? null : reader.GetString(1),
                reader.IsDBNull(2) ? null : reader.GetString(2)
            ));

        return new AuditFiltersDto(tiposEntidad, acciones, resultados, hotelIds, usuarios);
    }
}
