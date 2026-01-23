-- FunciÃ³n: Obtener credencial activa del huÃ©sped
CREATE FUNCTION [dbo].[FN_ObtenerCredencialActivaHuesped]
(
    @HuespedId INT
)
RETURNS NVARCHAR(6)
AS
BEGIN
    DECLARE @CodigoPIN NVARCHAR(6);
    
    SELECT TOP 1 @CodigoPIN = CodigoPIN
    FROM [dbo].[CredencialesAcceso]
    WHERE HuespedId = @HuespedId
      AND EstaActiva = 1
      AND GETUTCDATE() BETWEEN FechaActivacion AND FechaExpiracion
    ORDER BY FechaCreacion DESC;
    
    RETURN @CodigoPIN;
END