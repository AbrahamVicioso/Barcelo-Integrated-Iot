using System.Collections.Generic;

namespace Dispositivos.Domain.Entities;

public class EstadoDispositivo
{
    public int EstadoDispositivoId { get; set; }

    public string Descripcion { get; set; } = string.Empty;

    public virtual ICollection<Dispositivo> Dispositivos { get; set; } = new List<Dispositivo>();
}
