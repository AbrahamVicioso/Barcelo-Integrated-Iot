-- SP: Desactivar credenciales expiradas
CREATE PROCEDURE [dbo].[SP_DesactivarCredencialesExpiradas]
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE [dbo].[CredencialesAcceso]
    SET EstaActiva = 0
    WHERE EstaActiva = 1
      AND FechaExpiracion < GETUTCDATE();
    
    SELECT @@ROWCOUNT AS CredencialesDesactivadas;
END