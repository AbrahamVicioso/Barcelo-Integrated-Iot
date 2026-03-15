using System.Collections.Generic;

namespace Dispositivos.Domain.Entities;

public class TipoDispositivo
{
    public int TipoDispositivoId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public virtual ICollection<Dispositivo> Dispositivos { get; set; } = new List<Dispositivo>();
}
