# API de Usuarios - Barceló IoT System

API REST desarrollada con ASP.NET Core 9.0 siguiendo los principios de Clean Architecture y CQRS con MediatR.

## Estructura del Proyecto

```
Usuarios-Barcelo/
├── Usuarios.Domain/          # Capa de Dominio
│   ├── Entities/            # Entidades del dominio
│   │   ├── Huespede.cs
│   │   ├── Personal.cs
│   │   └── PermisosPersonal.cs
│   └── Interfaces/          # Interfaces de repositorios
│       ├── IGenericRepository.cs
│       ├── IHuespedeRepository.cs
│       ├── IPersonalRepository.cs
│       ├── IPermisosPersonalRepository.cs
│       └── IUnitOfWork.cs
│
├── Usuarios.Application/     # Capa de Aplicación
│   ├── Common/              # Clases comunes
│   │   └── Result.cs
│   ├── DTOs/                # Data Transfer Objects
│   │   ├── Huespedes/
│   │   ├── Personal/
│   │   └── PermisosPersonal/
│   ├── UseCases/            # Casos de uso con MediatR
│   │   ├── Huespedes/
│   │   │   ├── Commands/
│   │   │   │   ├── CreateHuespede/
│   │   │   │   ├── UpdateHuespede/
│   │   │   │   └── DeleteHuespede/
│   │   │   └── Queries/
│   │   │       ├── GetAllHuespedes/
│   │   │       ├── GetHuespedeById/
│   │   │       └── GetHuespedesVip/
│   │   ├── Personal/
│   │   │   ├── Commands/
│   │   │   └── Queries/
│   │   └── PermisosPersonal/
│   │       ├── Commands/
│   │       └── Queries/
│   ├── Validators/          # Validadores FluentValidation
│   │   ├── Huespedes/
│   │   ├── Personal/
│   │   └── PermisosPersonal/
│   └── Mappings/            # Perfiles de AutoMapper
│       └── MappingProfile.cs
│
├── Usuarios.Persistence/    # Capa de Infraestructura - Persistencia
│   ├── Data/
│   │   └── BarceloIoTSystemContext.cs
│   └── Repositories/        # Implementaciones de repositorios
│       ├── GenericRepository.cs
│       ├── HuespedeRepository.cs
│       ├── PersonalRepository.cs
│       ├── PermisosPersonalRepository.cs
│       └── UnitOfWork.cs
│
└── Usuarios.API/            # Capa de Presentación
    ├── Controllers/         # Controladores REST
    │   ├── HuespedesController.cs
    │   ├── PersonalController.cs
    │   └── PermisosPersonalController.cs
    ├── Program.cs           # Configuración de la aplicación
    └── appsettings.json     # Configuración

```

## Tecnologías Utilizadas

- **.NET 9.0**
- **ASP.NET Core Web API**
- **Entity Framework Core 9.0.3**
- **SQL Server**
- **MediatR 12.4.1** - Patrón CQRS
- **AutoMapper 13.0.1** - Mapeo de objetos
- **FluentValidation 11.11.0** - Validación de DTOs

## Patrones Implementados

### Clean Architecture
La solución está organizada en capas con dependencias hacia el centro:
- **Domain**: Núcleo del negocio, sin dependencias externas
- **Application**: Lógica de aplicación, casos de uso
- **Persistence**: Acceso a datos, implementación de repositorios
- **API**: Capa de presentación, controladores

### CQRS (Command Query Responsibility Segregation)
Separación de operaciones de lectura y escritura usando MediatR:
- **Commands**: CreateHuespede, UpdateHuespede, DeleteHuespede, etc.
- **Queries**: GetAllHuespedes, GetHuespedeById, GetHuespedesVip, etc.

### Repository Pattern & Unit of Work
Abstracción del acceso a datos con repositorios genéricos y específicos.

## Endpoints de la API

### Huéspedes
- `GET /api/huespedes` - Obtener todos los huéspedes
- `GET /api/huespedes/{id}` - Obtener huésped por ID
- `GET /api/huespedes/vip` - Obtener huéspedes VIP
- `POST /api/huespedes` - Crear nuevo huésped
- `PUT /api/huespedes/{id}` - Actualizar huésped
- `DELETE /api/huespedes/{id}` - Eliminar huésped

### Personal
- `GET /api/personal` - Obtener todo el personal
- `GET /api/personal/{id}` - Obtener personal por ID
- `GET /api/personal/activo` - Obtener personal activo
- `GET /api/personal/departamento/{departamento}` - Obtener personal por departamento
- `POST /api/personal` - Crear nuevo personal
- `PUT /api/personal/{id}` - Actualizar personal
- `DELETE /api/personal/{id}` - Eliminar personal

### Permisos Personal
- `GET /api/permisospersonal` - Obtener todos los permisos
- `GET /api/permisospersonal/{id}` - Obtener permiso por ID
- `GET /api/permisospersonal/personal/{personalId}` - Obtener permisos de un personal
- `GET /api/permisospersonal/personal/{personalId}/activos` - Obtener permisos activos
- `POST /api/permisospersonal` - Crear nuevo permiso
- `PUT /api/permisospersonal/{id}` - Actualizar permiso
- `DELETE /api/permisospersonal/{id}` - Eliminar permiso

## Configuración

### Cadena de Conexión
Modificar en `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=BarceloIoTSystem;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### Dependencias
Todas las dependencias están configuradas en Program.cs:
- DbContext con SQL Server
- AutoMapper
- MediatR
- FluentValidation
- Repositorios y Unit of Work
- CORS

## Características Implementadas

### Validación
Cada DTO tiene su validador con FluentValidation que valida:
- Campos requeridos
- Longitud máxima de cadenas
- Formatos de datos
- Reglas de negocio

### Manejo de Resultados
Todas las operaciones retornan un `Result<T>` que incluye:
- `IsSuccess`: Indica si la operación fue exitosa
- `Data`: Datos devueltos
- `Message`: Mensaje de éxito
- `Errors`: Lista de errores si los hay

### Mapeo Automático
AutoMapper está configurado para mapear entre:
- Entidades ↔ DTOs
- CreateDTOs → Entidades
- UpdateDTOs → Entidades

### Casos de Uso Completos

#### Huéspedes
- ✅ Crear huésped con validación de duplicados
- ✅ Actualizar información del huésped
- ✅ Eliminar huésped
- ✅ Obtener todos los huéspedes
- ✅ Obtener huésped por ID
- ✅ Obtener huéspedes VIP

#### Personal
- ✅ Crear personal con validación de duplicados
- ✅ Actualizar información del personal
- ✅ Eliminar personal (valida subordinados)
- ✅ Obtener todo el personal
- ✅ Obtener personal por ID
- ✅ Obtener personal activo
- ✅ Obtener personal por departamento
- ✅ Validación de supervisor existente
- ✅ Prevención de auto-asignación como supervisor

#### Permisos Personal
- ✅ Crear permiso con validación de personal activo
- ✅ Actualizar permiso
- ✅ Eliminar permiso
- ✅ Obtener todos los permisos
- ✅ Obtener permiso por ID
- ✅ Obtener permisos por personal
- ✅ Obtener permisos activos
- ✅ Validación de permisos temporales
- ✅ Validación de fechas de expiración

## Cómo Ejecutar

1. Asegurarse de tener .NET 9.0 SDK instalado
2. Configurar la cadena de conexión en `appsettings.json`
3. Restaurar dependencias:
   ```bash
   dotnet restore
   ```
4. Compilar la solución:
   ```bash
   dotnet build
   ```
5. Ejecutar la API:
   ```bash
   cd Usuarios.API
   dotnet run
   ```
6. La API estará disponible en: `https://localhost:5001`
7. Documentación OpenAPI: `https://localhost:5001/openapi/v1.json`

## Estructura de Base de Datos

La base de datos `BarceloIoTSystem` contiene las siguientes tablas:

### Huespedes
- HuespedId (PK)
- UsuarioId (UK)
- NombreCompleto
- TipoDocumento + NumeroDocumento (UK)
- Nacionalidad
- FechaNacimiento
- ContactoEmergencia
- TelefonoEmergencia
- EsVIP
- FechaRegistro
- PreferenciasAlimentarias
- NotasEspeciales

### Personal
- PersonalId (PK)
- UsuarioId (UK)
- HotelId
- NombreCompleto
- Puesto
- Departamento
- NumeroEmpleado (UK)
- FechaContratacion
- EstaActivo
- Turno
- Supervisor (FK → Personal)
- FechaCreacion

### PermisosPersonal
- PermisoId (PK)
- PersonalId (FK → Personal)
- HabitacionId
- TipoPermiso
- FechaOtorgamiento
- FechaExpiracion
- EsTemporal
- OtorgadoPor
- EstaActivo
- Justificacion

## Notas Adicionales

- La API implementa CORS para permitir peticiones desde cualquier origen
- Todas las fechas se manejan en UTC
- Los campos nullable están correctamente manejados
- La API sigue convenciones REST estándar
- El código está organizado siguiendo principios SOLID
- La separación de concerns facilita el testing y mantenimiento
