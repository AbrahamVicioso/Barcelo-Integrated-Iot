-- =============================================
-- STORED PROCEDURES PARA OPERACIONES COMUNES
-- =============================================

-- SP: Generar PIN Ãºnico de 6 dÃ­gitos
CREATE PROCEDURE [dbo].[SP_GenerarPINUnico]
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @PIN NVARCHAR(6);
    DECLARE @Existe BIT = 1;
    
    WHILE @Existe = 1
    BEGIN
        SET @PIN = RIGHT('000000' + CAST(ABS(CHECKSUM(NEWID())) % 1000000 AS VARCHAR(6)), 6);
        
        IF NOT EXISTS (SELECT 1 FROM [dbo].[CredencialesAcceso] WHERE CodigoPIN = @PIN AND EstaActiva = 1)
            SET @Existe = 0;
    END
    
    SELECT @PIN AS PIN;
END