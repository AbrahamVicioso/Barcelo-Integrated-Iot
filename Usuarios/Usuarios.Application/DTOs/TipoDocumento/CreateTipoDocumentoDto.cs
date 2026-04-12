namespace Usuarios.Application.DTOs.TipoDocumento;

public class CreateTipoDocumentoDto
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}
