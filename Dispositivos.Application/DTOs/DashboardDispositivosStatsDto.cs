namespace Dispositivos.Application.DTOs;

public class DashboardDispositivosStatsDto
{
    public CredencialesStatsDto Credenciales { get; set; } = new();
    public DispositivosStatsDto Dispositivos { get; set; } = new();
    public CerradurasStatsDto Cerraduras { get; set; } = new();
    public AuditoriaStatsDto Auditoria { get; set; } = new();
}

public class CredencialesStatsDto
{
    public int Total { get; set; }
    public int Activas { get; set; }
    public int PorVencer { get; set; }
    public int Expiradas { get; set; }
    public List<CredencialesPorTipoDto> PorTipo { get; set; } = new();
    public List<CredencialProximaVencerDto> ProximasVencer { get; set; } = new();
}

public class CredencialesPorTipoDto
{
    public string Tipo { get; set; } = string.Empty;
    public int Activas { get; set; }
    public int Total { get; set; }
}

public class CredencialProximaVencerDto
{
    public int CredencialId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string CodigoPin { get; set; } = string.Empty;
    public int DiasRestantes { get; set; }
    public DateTime FechaExpiracion { get; set; }
}

public class DispositivosStatsDto
{
    public int Total { get; set; }
    public int Operativos { get; set; }
    public int BateriaBaja { get; set; }
    public int Offline { get; set; }
    public decimal SaludPorcentaje { get; set; }
    public List<DispositivosPorTipoDto> PorTipo { get; set; } = new();
    public List<DispositivoAlertaBateriaDto> AlertasBateria { get; set; } = new();
}

public class DispositivosPorTipoDto
{
    public string Tipo { get; set; } = string.Empty;
    public int Total { get; set; }
    public int Online { get; set; }
}

public class DispositivoAlertaBateriaDto
{
    public Guid DispositivoId { get; set; }
    public string NumeroSerie { get; set; } = string.Empty;
    public int NivelBateria { get; set; }
    public int HotelId { get; set; }
    public int? HabitacionId { get; set; }
}

public class CerradurasStatsDto
{
    public int Total { get; set; }
    public int Online { get; set; }
    public int Offline { get; set; }
    public int Abiertas { get; set; }
    public int Cerradas { get; set; }
    public int SoportanOffline { get; set; }
    public decimal PromedioAperturasHoy { get; set; }
}

public class AuditoriaStatsDto
{
    public int Ultimas24h { get; set; }
    public int UltimaSemana { get; set; }
    public List<AuditoriaRecienteDto> RegistrosRecientes { get; set; } = new();
}

public class AuditoriaRecienteDto
{
    public long AuditoriaId { get; set; }
    public string Accion { get; set; } = string.Empty;
    public string TipoEntidad { get; set; } = string.Empty;
    public string Resultado { get; set; } = string.Empty;
    public string UsuarioId { get; set; } = string.Empty;
    public DateTime FechaHora { get; set; }
}
