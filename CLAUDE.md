# CLAUDE.md — Barcelo Integrated IoT

## Instrucciones
- Leer este archivo al inicio de cada sesión
- **Antes de leer código**, consultar aquí. Solo leer archivos si el patrón no está documentado
- Si se descubre algo no documentado, agregarlo aquí inmediatamente

### Cuándo NO leer archivos
- Nuevo endpoint → "Estándar de paginación"
- Nuevo command handler → "Manejo de errores en handlers"
- Métodos de repositorio → sección repositorios
- Constraints DB → "DbUpdateException"
- Llamadas gRPC → "Comunicación gRPC"
- Flujo credenciales → "Flujo creación credenciales"

---

## 🚫 NUNCA hacer
1. Configurar Kestrel en Program.cs si existe appsettings.Docker.json
2. Lanzar excepciones en handlers de Dispositivos/Reservas → usar `Result<T>`
3. Usar HTTP para conectar a gRPC si el servidor usa HTTPS (5285, 5118, 7288)
4. Olvidar HttpClientHandler con bypass de certificado en llamadas gRPC

## ✅ SIEMPRE hacer
1. Retry pattern (3 intentos, delay 500ms * attempt) en llamadas gRPC
2. Validar FKs antes de insert/update
3. Capturar DbUpdateException por nombre de string
4. Usar `Result<T>.NotFound()` cuando entidad no existe
5. Limpiar navigation properties antes de UpdateAsync si FK cambió
6. Usar appsettings.Docker.json como única fuente de verdad para Kestrel
7. Separar puertos y protocolos: HTTP/1.1 ≠ HTTP/2

---

## Infraestructura
- **.NET 9** · SQL Server (sqlserver:1433) · Kafka (kafka:29092) · ThingsBoard CE (thingsboard-ce:8080)
- **JWT:** Issuer=`barcelo`, Audience=`BarceloIoT`, Key=`u9Z3fBq7M!8@R2L#A4xCkWmP0EJvH5Ys`

## Servicios y puertos
| Servicio | REST | gRPC |
|---|---|---|
| Dispositivos.API | 5185 | 7288 (HTTPS) |
| Usuarios.API | 5284 | 5285 (HTTPS) |
| Authenticate.API | 5117 | 5118 (HTTPS) |
| Reservas.API | 5141 | — |

---

## gRPC clientes
```csharp
var httpHandler = new HttpClientHandler();
if (skipCertValidation)
    httpHandler.ServerCertificateCustomValidationCallback = 
        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

using var channel = GrpcChannel.ForAddress(grpcUrl, new GrpcChannelOptions {
    HttpHandler = httpHandler, DisposeHttpClient = true
});
// 3 retries con delay 500ms * attempt
```

---

## Patrones clave
- **Result<T>** en Dispositivos/Reservas: Success(data) → 200, NotFound(msg) → 404, Failure(msg) → 400
- **Excepciones** en Usuarios: NotFoundException → 404, ConflictException → 409, BusinessException → 400
- **Controller response:** `!IsSuccess ? (IsNotFound ? NotFound : BadRequest) : Ok/CreatedAtAction`
- **ThingsBoard:** no-bloqueante, siempre continúa con RegistrosAcceso

---

## Errores en handlers
```csharp
catch (Exception ex) when (ex.GetType().Name == "DbUpdateException") {
    var inner = ex.InnerException?.Message ?? ex.Message;
    if (inner.Contains("UQ_Cerraduras_Habitacion")) return Result<T>.Failure("...");
    // ... otros constraints
}
```

---

## Kafka topics
- `credenciales.creada` (Dispositivos → Notification)
- `dispositivos.unlock-door` (Reservas → Dispositivos)
- `reservas.checkin-realizado` (Reservas → Dispositivos)
- `habitacion.personal-unlock` (Reservas → Dispositivos)
- `habitacion.permiso-personal` (Usuarios → Dispositivos)
- `cerradura.acceso` (ThingsBoard → Dispositivos)

---

## ThingsBoard
- Device name = `CerradurasInteligente.DispositivoId.ToString()`
- Sync credenciales: `FechaActivacion <= NOW+7d AND FechaExpiracion >= NOW`
- API: `/api/tenant/devices?deviceName={name}`, `/api/plugins/telemetry/DEVICE/{id}/SHARED_SCOPE`

---

## Permisos (JWT claims)
- `Permissions.Usuarios.View/Create/Edit/Delete`
- `Permissions.Dispositivos.View/Create/Edit/Delete`
- `Permissions.Reservas.View/Create/Edit/Delete`
- `Permissions.Cerraduras.View/Create/Edit/Delete`
- `Permissions.Credenciales.View/Create/Edit/Delete`
- `Permissions.Roles.View/Create/Edit/Delete/ManagePermissions`
- Claim type: `Permission`

---

## Finicio rápido
```powershell
.\scripts\bootstrap-local.ps1           # Certificados
docker compose up -d sqlserver kafka ntfy  # Infra
docker compose -f docker-compose.yml -f docker-compose.dev.yml up  # APIs
```

---

## Endpoints importantes
- `/reservas/{id}/unlock-door?pin=` — Unlock huésped
- `/reservas/{id}/checkin` — Check-in
- `/habitacion/{id}/unlock` — Unlock personal
- `/credencialesacceso` — CRUD credenciales
- `/personal` — CRUD personal
- `/huesped/me` — Perfil propio

---

## Troubleshooting
- gRPC error: verificar protocolo (HTTPS vs HTTP), certificado, puertos
- Email null: verificar Huesped.UsuarioId existe y usuario en Identity
- Kestrel: solo usar appsettings.Docker.json, nunca Program.cs