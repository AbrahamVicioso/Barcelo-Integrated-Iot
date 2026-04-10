namespace Usuarios.Application.DTOs.Dashboard;

public class HuespedesStatsDto
{
    public int Total { get; set; }
    public int Vip { get; set; }
    public List<HuespedesPorTipoDocumentoDto> PorTipoDocumento { get; set; } = new();
}

public class HuespedesPorTipoDocumentoDto
{
    public string Tipo { get; set; } = string.Empty;
    public int Cantidad { get; set; }
}

public class PersonalStatsDto
{
    public int Total { get; set; }
    public int Activos { get; set; }
    public int Inactivos { get; set; }
    public List<PersonalPorDepartamentoDto> PorDepartamento { get; set; } = new();
}

public class PersonalPorDepartamentoDto
{
    public string Departamento { get; set; } = string.Empty;
    public int Cantidad { get; set; }
}
