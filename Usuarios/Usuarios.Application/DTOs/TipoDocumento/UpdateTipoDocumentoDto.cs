namespace Usuarios.Application.DTOs.TipoDocumento;

public class UpdateTipoDocumentoDto
{
    public int TipoDocumentoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool EstaActivo { get; set; }
}
