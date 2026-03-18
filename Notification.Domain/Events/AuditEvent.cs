namespace Notification.Domain.Events
{
    public class AuditEvent
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Which API published this event (e.g. "Reservas.API")</summary>
        public string Servicio { get; set; } = string.Empty;

        public string UsuarioId { get; set; } = string.Empty;

        /// <summary>CREATE | UPDATE | DELETE | LOGIN | REGISTER | UNLOCK_DOOR | ACCESS | MAINTENANCE</summary>
        public string Accion { get; set; } = string.Empty;

        /// <summary>Domain entity name (e.g. "Reserva", "Huesped", "Dispositivo")</summary>
        public string TipoEntidad { get; set; } = string.Empty;

        public string? EntidadId { get; set; }

        public string? ValorAnterior { get; set; }

        public string? ValorNuevo { get; set; }

        public string DireccionIp { get; set; } = string.Empty;

        public string? AgenteUsuario { get; set; }

        /// <summary>Exitoso | Fallido</summary>
        public string Resultado { get; set; } = "Exitoso";

        public string? MensajeError { get; set; }

        public int? HotelId { get; set; }

        public DateTime FechaHora { get; set; } = DateTime.UtcNow;
    }
}
