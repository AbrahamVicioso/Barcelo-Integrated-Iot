namespace Barcelo.Authorization.Shared;

public static class Permissions
{
    public static class Usuarios
    {
        public const string View = "Permissions.Usuarios.View";
        public const string Create = "Permissions.Usuarios.Create";
        public const string Edit = "Permissions.Usuarios.Edit";
        public const string Delete = "Permissions.Usuarios.Delete";
    }

    public static class Huespedes
    {
        public const string View = "Permissions.Huespedes.View";
        public const string Create = "Permissions.Huespedes.Create";
        public const string Edit = "Permissions.Huespedes.Edit";
        public const string Delete = "Permissions.Huespedes.Delete";
    }

    public static class Personal
    {
        public const string View = "Permissions.Personal.View";
        public const string Create = "Permissions.Personal.Create";
        public const string Edit = "Permissions.Personal.Edit";
        public const string Delete = "Permissions.Personal.Delete";
    }

    public static class Auth
    {
        public const string View = "Permissions.Auth.View";
        public const string Create = "Permissions.Auth.Create";
    }

    public static class Dispositivos
    {
        public const string View = "Permissions.Dispositivos.View";
        public const string Create = "Permissions.Dispositivos.Create";
        public const string Edit = "Permissions.Dispositivos.Edit";
        public const string Delete = "Permissions.Dispositivos.Delete";
    }

    public static class Reservas
    {
        public const string View = "Permissions.Reservas.View";
        public const string Create = "Permissions.Reservas.Create";
        public const string Edit = "Permissions.Reservas.Edit";
        public const string Delete = "Permissions.Reservas.Delete";
    }

    public static class Hoteles
    {
        public const string View = "Permissions.Hoteles.View";
        public const string Create = "Permissions.Hoteles.Create";
        public const string Edit = "Permissions.Hoteles.Edit";
        public const string Delete = "Permissions.Hoteles.Delete";
    }

    public static class Habitaciones
    {
        public const string View = "Permissions.Habitaciones.View";
        public const string Create = "Permissions.Habitaciones.Create";
        public const string Edit = "Permissions.Habitaciones.Edit";
        public const string Delete = "Permissions.Habitaciones.Delete";
    }

    public static class Cerraduras
    {
        public const string View = "Permissions.Cerraduras.View";
        public const string Create = "Permissions.Cerraduras.Create";
        public const string Edit = "Permissions.Cerraduras.Edit";
        public const string Delete = "Permissions.Cerraduras.Delete";
    }

    public static class Credenciales
    {
        public const string View = "Permissions.Credenciales.View";
        public const string Create = "Permissions.Credenciales.Create";
        public const string Edit = "Permissions.Credenciales.Edit";
        public const string Delete = "Permissions.Credenciales.Delete";
    }

    public static class Mantenimientos
    {
        public const string View = "Permissions.Mantenimientos.View";
        public const string Create = "Permissions.Mantenimientos.Create";
        public const string Edit = "Permissions.Mantenimientos.Edit";
        public const string Delete = "Permissions.Mantenimientos.Delete";
    }

    public static class Roles
    {
        public const string View = "Permissions.Roles.View";
        public const string Create = "Permissions.Roles.Create";
        public const string Edit = "Permissions.Roles.Edit";
        public const string Delete = "Permissions.Roles.Delete";
        public const string ManagePermissions = "Permissions.Roles.ManagePermissions";
    }

    public static class Reports
    {
        public const string View = "Permissions.Reports.View";
    }

    public static class Audit
    {
        public const string View = "Permissions.Audit.View";
    }

    public static class Admin
    {
        public const string All = "Permissions.Admin.All";
    }

    public static IReadOnlyList<string> GetAllPermissions()
    {
        return
        [
            Admin.All,
            Usuarios.View, Usuarios.Create, Usuarios.Edit, Usuarios.Delete,
            Huespedes.View, Huespedes.Create, Huespedes.Edit, Huespedes.Delete,
            Personal.View, Personal.Create, Personal.Edit, Personal.Delete,
            Auth.View, Auth.Create,
            Dispositivos.View, Dispositivos.Create, Dispositivos.Edit, Dispositivos.Delete,
            Reservas.View, Reservas.Create, Reservas.Edit, Reservas.Delete,
            Hoteles.View, Hoteles.Create, Hoteles.Edit, Hoteles.Delete,
            Habitaciones.View, Habitaciones.Create, Habitaciones.Edit, Habitaciones.Delete,
            Cerraduras.View, Cerraduras.Create, Cerraduras.Edit, Cerraduras.Delete,
            Credenciales.View, Credenciales.Create, Credenciales.Edit, Credenciales.Delete,
            Mantenimientos.View, Mantenimientos.Create, Mantenimientos.Edit, Mantenimientos.Delete,
            Roles.View, Roles.Create, Roles.Edit, Roles.Delete, Roles.ManagePermissions,
            Reports.View,
            Audit.View,
        ];
    }
}

public class PermissionDescription
{
    public required string Permission { get; init; }
    public required string Description { get; init; }
    public required string Category { get; init; }
    public required string Action { get; init; }
}

public static class PermissionDescriptions
{
    public static IReadOnlyList<PermissionDescription> GetAll()
    {
        return
        [
            new() { Permission = Permissions.Admin.All, Description = "Acceso completo a todas las funciones del sistema", Category = "Administración", Action = "Admin" },
            new() { Permission = Permissions.Usuarios.View, Description = "Ver lista de usuarios y perfiles", Category = "Usuarios", Action = "Ver" },
            new() { Permission = Permissions.Usuarios.Create, Description = "Crear nuevos usuarios y perfiles de	huésped/personal", Category = "Usuarios", Action = "Crear" },
            new() { Permission = Permissions.Usuarios.Edit, Description = "Editar información de usuarios y perfiles", Category = "Usuarios", Action = "Editar" },
            new() { Permission = Permissions.Usuarios.Delete, Description = "Eliminar usuarios y perfiles", Category = "Usuarios", Action = "Eliminar" },
            new() { Permission = Permissions.Huespedes.View, Description = "Ver lista de huéspedes", Category = "Huéspedes", Action = "Ver" },
            new() { Permission = Permissions.Huespedes.Create, Description = "Crear nuevos perfiles de huésped", Category = "Huéspedes", Action = "Crear" },
            new() { Permission = Permissions.Huespedes.Edit, Description = "Editar información de huéspedes", Category = "Huéspedes", Action = "Editar" },
            new() { Permission = Permissions.Huespedes.Delete, Description = "Eliminar perfiles de huésped", Category = "Huéspedes", Action = "Eliminar" },
            new() { Permission = Permissions.Personal.View, Description = "Ver lista de personal", Category = "Personal", Action = "Ver" },
            new() { Permission = Permissions.Personal.Create, Description = "Crear nuevos perfiles de personal", Category = "Personal", Action = "Crear" },
            new() { Permission = Permissions.Personal.Edit, Description = "Editar información de personal", Category = "Personal", Action = "Editar" },
            new() { Permission = Permissions.Personal.Delete, Description = "Eliminar perfiles de personal", Category = "Personal", Action = "Eliminar" },
            new() { Permission = Permissions.Auth.View, Description = "Buscar usuarios del sistema por email", Category = "Autenticación", Action = "Ver" },
            new() { Permission = Permissions.Auth.Create, Description = "Crear usuarios del sistema", Category = "Autenticación", Action = "Crear" },
            new() { Permission = Permissions.Dispositivos.View, Description = "Ver lista de dispositivos", Category = "Dispositivos", Action = "Ver" },
            new() { Permission = Permissions.Dispositivos.Create, Description = "Registrar nuevos dispositivos", Category = "Dispositivos", Action = "Crear" },
            new() { Permission = Permissions.Dispositivos.Edit, Description = "Editar información de dispositivos", Category = "Dispositivos", Action = "Editar" },
            new() { Permission = Permissions.Dispositivos.Delete, Description = "Eliminar dispositivos", Category = "Dispositivos", Action = "Eliminar" },
            new() { Permission = Permissions.Reservas.View, Description = "Ver lista de reservas", Category = "Reservas", Action = "Ver" },
            new() { Permission = Permissions.Reservas.Create, Description = "Crear nuevas reservas", Category = "Reservas", Action = "Crear" },
            new() { Permission = Permissions.Reservas.Edit, Description = "Editar reservas existentes", Category = "Reservas", Action = "Editar" },
            new() { Permission = Permissions.Reservas.Delete, Description = "Cancelar reservas", Category = "Reservas", Action = "Eliminar" },
            new() { Permission = Permissions.Hoteles.View, Description = "Ver lista de hoteles", Category = "Hoteles", Action = "Ver" },
            new() { Permission = Permissions.Hoteles.Create, Description = "Crear nuevos hoteles", Category = "Hoteles", Action = "Crear" },
            new() { Permission = Permissions.Hoteles.Edit, Description = "Editar información de hoteles", Category = "Hoteles", Action = "Editar" },
            new() { Permission = Permissions.Hoteles.Delete, Description = "Eliminar hoteles", Category = "Hoteles", Action = "Eliminar" },
            new() { Permission = Permissions.Habitaciones.View, Description = "Ver lista de habitaciones", Category = "Habitaciones", Action = "Ver" },
            new() { Permission = Permissions.Habitaciones.Create, Description = "Crear nuevas habitaciones", Category = "Habitaciones", Action = "Crear" },
            new() { Permission = Permissions.Habitaciones.Edit, Description = "Editar información de habitaciones", Category = "Habitaciones", Action = "Editar" },
            new() { Permission = Permissions.Habitaciones.Delete, Description = "Eliminar habitaciones", Category = "Habitaciones", Action = "Eliminar" },
            new() { Permission = Permissions.Cerraduras.View, Description = "Ver lista de cerraduras inteligentes", Category = "Cerraduras", Action = "Ver" },
            new() { Permission = Permissions.Cerraduras.Create, Description = "Registrar nuevas cerraduras", Category = "Cerraduras", Action = "Crear" },
            new() { Permission = Permissions.Cerraduras.Edit, Description = "Editar configuración de cerraduras", Category = "Cerraduras", Action = "Editar" },
            new() { Permission = Permissions.Cerraduras.Delete, Description = "Eliminar cerraduras", Category = "Cerraduras", Action = "Eliminar" },
            new() { Permission = Permissions.Credenciales.View, Description = "Ver lista de credenciales de acceso", Category = "Credenciales", Action = "Ver" },
            new() { Permission = Permissions.Credenciales.Create, Description = "Crear credenciales de acceso", Category = "Credenciales", Action = "Crear" },
            new() { Permission = Permissions.Credenciales.Edit, Description = "Editar credenciales de acceso", Category = "Credenciales", Action = "Editar" },
            new() { Permission = Permissions.Credenciales.Delete, Description = "Eliminar credenciales de acceso", Category = "Credenciales", Action = "Eliminar" },
            new() { Permission = Permissions.Mantenimientos.View, Description = "Ver lista de mantenimientos", Category = "Mantenimientos", Action = "Ver" },
            new() { Permission = Permissions.Mantenimientos.Create, Description = "Programar mantenimientos", Category = "Mantenimientos", Action = "Crear" },
            new() { Permission = Permissions.Mantenimientos.Edit, Description = "Editar mantenimientos", Category = "Mantenimientos", Action = "Editar" },
            new() { Permission = Permissions.Mantenimientos.Delete, Description = "Eliminar mantenimientos", Category = "Mantenimientos", Action = "Eliminar" },
            new() { Permission = Permissions.Roles.View, Description = "Ver lista de roles", Category = "Roles", Action = "Ver" },
            new() { Permission = Permissions.Roles.Create, Description = "Crear nuevos roles", Category = "Roles", Action = "Crear" },
            new() { Permission = Permissions.Roles.Edit, Description = "Editar roles existentes", Category = "Roles", Action = "Editar" },
            new() { Permission = Permissions.Roles.Delete, Description = "Eliminar roles", Category = "Roles", Action = "Eliminar" },
            new() { Permission = Permissions.Roles.ManagePermissions, Description = "Asignar permisos a roles", Category = "Roles", Action = "Administrar" },
            new() { Permission = Permissions.Reports.View, Description = "Ver reportes y estadísticas", Category = "Reportes", Action = "Ver" },
            new() { Permission = Permissions.Audit.View, Description = "Ver registros de auditoría", Category = "Auditoría", Action = "Ver" },
        ];
    }
}