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