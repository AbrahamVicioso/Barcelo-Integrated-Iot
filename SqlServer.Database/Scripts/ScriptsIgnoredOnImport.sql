
USE [master]
GO

/****** Object:  Database [BarceloIoTSystem]    Script Date: 12/9/2025 8:27:24 AM ******/
CREATE DATABASE [BarceloIoTSystem]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'BarceloIoTSystem', FILENAME = N'/var/opt/mssql/data/BarceloIoTSystem.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'BarceloIoTSystem_log', FILENAME = N'/var/opt/mssql/data/BarceloIoTSystem_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO

ALTER DATABASE [BarceloIoTSystem] SET COMPATIBILITY_LEVEL = 160
GO

IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [BarceloIoTSystem].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO

ALTER DATABASE [BarceloIoTSystem] SET ANSI_NULL_DEFAULT OFF
GO

ALTER DATABASE [BarceloIoTSystem] SET ANSI_NULLS OFF
GO

ALTER DATABASE [BarceloIoTSystem] SET ANSI_PADDING OFF
GO

ALTER DATABASE [BarceloIoTSystem] SET ANSI_WARNINGS OFF
GO

ALTER DATABASE [BarceloIoTSystem] SET ARITHABORT OFF
GO

ALTER DATABASE [BarceloIoTSystem] SET AUTO_CLOSE OFF
GO

ALTER DATABASE [BarceloIoTSystem] SET AUTO_SHRINK OFF
GO

ALTER DATABASE [BarceloIoTSystem] SET AUTO_UPDATE_STATISTICS ON
GO

ALTER DATABASE [BarceloIoTSystem] SET CURSOR_CLOSE_ON_COMMIT OFF
GO

ALTER DATABASE [BarceloIoTSystem] SET CURSOR_DEFAULT  GLOBAL
GO

ALTER DATABASE [BarceloIoTSystem] SET CONCAT_NULL_YIELDS_NULL OFF
GO

ALTER DATABASE [BarceloIoTSystem] SET NUMERIC_ROUNDABORT OFF
GO

ALTER DATABASE [BarceloIoTSystem] SET QUOTED_IDENTIFIER OFF
GO

ALTER DATABASE [BarceloIoTSystem] SET RECURSIVE_TRIGGERS OFF
GO

ALTER DATABASE [BarceloIoTSystem] SET  ENABLE_BROKER
GO

ALTER DATABASE [BarceloIoTSystem] SET AUTO_UPDATE_STATISTICS_ASYNC OFF
GO

ALTER DATABASE [BarceloIoTSystem] SET DATE_CORRELATION_OPTIMIZATION OFF
GO

ALTER DATABASE [BarceloIoTSystem] SET TRUSTWORTHY OFF
GO

ALTER DATABASE [BarceloIoTSystem] SET ALLOW_SNAPSHOT_ISOLATION ON
GO

ALTER DATABASE [BarceloIoTSystem] SET PARAMETERIZATION SIMPLE
GO

ALTER DATABASE [BarceloIoTSystem] SET READ_COMMITTED_SNAPSHOT ON
GO

ALTER DATABASE [BarceloIoTSystem] SET HONOR_BROKER_PRIORITY OFF
GO

ALTER DATABASE [BarceloIoTSystem] SET RECOVERY FULL
GO

ALTER DATABASE [BarceloIoTSystem] SET  MULTI_USER
GO

ALTER DATABASE [BarceloIoTSystem] SET PAGE_VERIFY CHECKSUM
GO

ALTER DATABASE [BarceloIoTSystem] SET DB_CHAINING OFF
GO

ALTER DATABASE [BarceloIoTSystem] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF )
GO

ALTER DATABASE [BarceloIoTSystem] SET TARGET_RECOVERY_TIME = 60 SECONDS
GO

ALTER DATABASE [BarceloIoTSystem] SET DELAYED_DURABILITY = DISABLED
GO

ALTER DATABASE [BarceloIoTSystem] SET ACCELERATED_DATABASE_RECOVERY = OFF
GO

EXEC sys.sp_db_vardecimal_storage_format N'BarceloIoTSystem', N'ON'
GO

ALTER DATABASE [BarceloIoTSystem] SET QUERY_STORE = ON
GO

USE [BarceloIoTSystem]
GO

/****** Object:  UserDefinedFunction [dbo].[FN_CalcularTasaOcupacion]    Script Date: 12/9/2025 8:27:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****** Object:  UserDefinedFunction [dbo].[FN_ObtenerCredencialActivaHuesped]    Script Date: 12/9/2025 8:27:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****** Object:  UserDefinedFunction [dbo].[FN_VerificarDisponibilidadHabitacion]    Script Date: 12/9/2025 8:27:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****** Object:  Table [dbo].[CerradurasInteligentes]    Script Date: 12/9/2025 8:27:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****** Object:  Table [dbo].[RegistrosAcceso]    Script Date: 12/9/2025 8:27:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****** Object:  Table [dbo].[Hoteles]    Script Date: 12/9/2025 8:27:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****** Object:  Table [dbo].[Habitaciones]    Script Date: 12/9/2025 8:27:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****** Object:  Table [dbo].[Dispositivos]    Script Date: 12/9/2025 8:27:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****** Object:  View [dbo].[VW_RendimientoCerraduras]    Script Date: 12/9/2025 8:27:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****** Object:  Table [dbo].[ActividadesRecreativas]    Script Date: 12/9/2025 8:27:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****** Object:  Table [dbo].[ReservasActividades]    Script Date: 12/9/2025 8:27:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****** Object:  View [dbo].[VW_ActividadesPopulares]    Script Date: 12/9/2025 8:27:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****** Object:  View [dbo].[VW_SaludDispositivosPorHotel]    Script Date: 12/9/2025 8:27:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****** Object:  Table [dbo].[Huespedes]    Script Date: 12/9/2025 8:27:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****** Object:  Table [dbo].[Reservas]    Script Date: 12/9/2025 8:27:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****** Object:  View [dbo].[VW_OcupacionHabitaciones]    Script Date: 12/9/2025 8:27:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****** Object:  Table [dbo].[CredencialesAcceso]    Script Date: 12/9/2025 8:27:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****** Object:  Table [dbo].[Users]    Script Date: 12/9/2025 8:27:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****** Object:  View [dbo].[VW_AccesosRecientes]    Script Date: 12/9/2025 8:27:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****** Object:  View [dbo].[VW_EstadoCerraduras]    Script Date: 12/9/2025 8:27:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****** Object:  View [dbo].[VW_InventarioDispositivos]    Script Date: 12/9/2025 8:27:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****** Object:  View [dbo].[VW_ActividadesHoy]    Script Date: 12/9/2025 8:27:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****** Object:  Table [dbo].[ConfiguracionSistema]    Script Date: 12/9/2025 8:27:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****** Object:  Table [dbo].[EventosSistema]    Script Date: 12/9/2025 8:27:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****** Object:  Table [dbo].[IndicadoresKPI]    Script Date: 12/9/2025 8:27:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****** Object:  Table [dbo].[LogsSistema]    Script Date: 12/9/2025 8:27:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****** Object:  Table [dbo].[MantenimientoCerraduras]    Script Date: 12/9/2025 8:27:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****** Object:  Table [dbo].[Notificaciones]    Script Date: 12/9/2025 8:27:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****** Object:  Table [dbo].[PermisosPersonal]    Script Date: 12/9/2025 8:27:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****** Object:  Table [dbo].[Personal]    Script Date: 12/9/2025 8:27:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****** Object:  Table [dbo].[PreferenciasNotificacion]    Script Date: 12/9/2025 8:27:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****** Object:  Table [dbo].[RegistrosAuditoria]    Script Date: 12/9/2025 8:27:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****** Object:  Table [dbo].[Reportes]    Script Date: 12/9/2025 8:27:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****** Object:  Table [dbo].[RoleClaims]    Script Date: 12/9/2025 8:27:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****** Object:  Table [dbo].[Roles]    Script Date: 12/9/2025 8:27:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****** Object:  Table [dbo].[UserClaims]    Script Date: 12/9/2025 8:27:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****** Object:  Table [dbo].[UserLogins]    Script Date: 12/9/2025 8:27:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****** Object:  Table [dbo].[UserRoles]    Script Date: 12/9/2025 8:27:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****** Object:  Table [dbo].[UserTokens]    Script Date: 12/9/2025 8:27:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****** Object:  Table [Seguridad].[ClavesEncriptacion]    Script Date: 12/9/2025 8:27:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

SET ANSI_PADDING ON
GO

SET ANSI_PADDING ON
GO

SET ANSI_PADDING ON
GO

SET ANSI_PADDING ON
GO

SET ANSI_PADDING ON
GO

SET ANSI_PADDING ON
GO

SET ANSI_PADDING ON
GO

SET ANSI_PADDING ON
GO

SET ANSI_PADDING ON
GO

SET ANSI_PADDING ON
GO

SET ANSI_PADDING ON
GO

SET ANSI_PADDING ON
GO

SET ANSI_PADDING ON
GO

SET ANSI_PADDING ON
GO

SET ANSI_PADDING ON
GO

/****** Object:  StoredProcedure [dbo].[SP_ActualizarKPIsDiarios]    Script Date: 12/9/2025 8:27:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****** Object:  StoredProcedure [dbo].[SP_AlertasBateriaBaja]    Script Date: 12/9/2025 8:27:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****** Object:  StoredProcedure [dbo].[SP_DesactivarCredencialesExpiradas]    Script Date: 12/9/2025 8:27:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****** Object:  StoredProcedure [dbo].[SP_EnviarRecordatoriosActividades]    Script Date: 12/9/2025 8:27:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****** Object:  StoredProcedure [dbo].[SP_GenerarPINUnico]    Script Date: 12/9/2025 8:27:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****** Object:  StoredProcedure [dbo].[SP_LimpiarLogsAntiguos]    Script Date: 12/9/2025 8:27:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****** Object:  StoredProcedure [dbo].[SP_ObtenerKPIsHotel]    Script Date: 12/9/2025 8:27:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****** Object:  StoredProcedure [dbo].[SP_RegistrarAcceso]    Script Date: 12/9/2025 8:27:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****** Object:  StoredProcedure [dbo].[SP_SincronizarEstadoDispositivo]    Script Date: 12/9/2025 8:27:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

USE [master]
GO

ALTER DATABASE [BarceloIoTSystem] SET  READ_WRITE
GO

ALTER DATABASE [BarceloIoTSystem] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)

GO

--Syntax Error: Incorrect syntax near 'WAIT_STATS_CAPTURE_MODE'.
--ALTER DATABASE [BarceloIoTSystem] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)



GO
