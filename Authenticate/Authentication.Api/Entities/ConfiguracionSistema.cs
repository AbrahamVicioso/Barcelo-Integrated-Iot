using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Authentication.Api.Entities;

[Table("ConfiguracionSistema")]
public class ConfiguracionSistema
{
    [Key]
    public int ConfiguracionId { get; set; }
    public int? HotelId { get; set; }

    [MaxLength(100)]
    public string Clave { get; set; } = string.Empty;

    public string Valor { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Descripcion { get; set; }

    [MaxLength(50)]
    public string TipoDato { get; set; } = string.Empty;

    public bool EsGlobal { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime? FechaActualizacion { get; set; }

    [MaxLength(450)]
    public string? ModificadoPor { get; set; }
}
