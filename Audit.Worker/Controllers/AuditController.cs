using Audit.Worker.Models;
using Microsoft.AspNetCore.Mvc;

namespace Audit.Worker.Controllers;

[ApiController]
[Route("audit")]
[Produces("application/json")]
public class AuditController : ControllerBase
{
    private readonly IAuditQueryRepository _queryRepository;

    public AuditController(IAuditQueryRepository queryRepository)
    {
        _queryRepository = queryRepository;
    }

    /// <summary>
    /// Obtiene el log de auditoría del sistema con paginación y filtros opcionales.
    /// Incluye el nombre y email del usuario relacionado.
    /// </summary>
    /// <param name="queryParams">Parámetros de paginación y filtro</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Lista paginada de registros de auditoría</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AuditRegistroDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAudit([FromQuery] AuditQueryParams queryParams, CancellationToken ct)
    {
        var result = await _queryRepository.GetPagedAsync(queryParams, ct);
        return Ok(result);
    }

    /// <summary>
    /// Devuelve los valores distintos disponibles para cada filtro de auditoría.
    /// Útil para construir dropdowns o autocompletes en el cliente.
    /// </summary>
    [HttpGet("filters")]
    [ProducesResponseType(typeof(AuditFiltersDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFilters(CancellationToken ct)
    {
        var result = await _queryRepository.GetFiltersAsync(ct);
        return Ok(result);
    }
}
