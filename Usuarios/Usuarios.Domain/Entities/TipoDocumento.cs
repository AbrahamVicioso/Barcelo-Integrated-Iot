namespace Usuarios.Domain.Entities;

public class TipoDocumento
{
    public int TipoDocumentoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool EstaActivo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? EliminadoEn { get; set; }

    public virtual ICollection<Huespede> Huespedes { get; set; } = new List<Huespede>();
}
