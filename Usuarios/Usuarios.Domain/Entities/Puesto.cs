namespace Usuarios.Domain.Entities;

public class Puesto
{
    public int PuestoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool EstaActivo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? EliminadoEn { get; set; }

    public virtual ICollection<Personal> Personals { get; set; } = new List<Personal>();
}
