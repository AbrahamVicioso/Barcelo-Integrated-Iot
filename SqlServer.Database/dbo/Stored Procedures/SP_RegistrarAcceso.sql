-- SP: Registrar acceso a habitaciÃ³n
CREATE PROCEDURE [dbo].[SP_RegistrarAcceso]
    @CerraduraId INT,
    @CredencialId INT = NULL,
    @UsuarioId NVARCHAR(450) = NULL,
    @TipoAcceso NVARCHAR(50),
    @MotivoAcceso NVARCHAR(200) = NULL,
    @DireccionIP NVARCHAR(50) = NULL,
    @InfoDispositivo NVARCHAR(500) = NULL,
    @FueExitoso BIT,
    @CodigoError NVARCHAR(50) = NULL,
    @Latencia INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        INSERT INTO [dbo].[RegistrosAcceso] (
            CerraduraId, CredencialId, UsuarioId, TipoAcceso,
            ResultadoAcceso, MotivoAcceso, DireccionIP, InfoDispositivo,
            FueExitoso, CodigoError, Latencia
        )
        VALUES (
            @CerraduraId, @CredencialId, @UsuarioId, @TipoAcceso,
            CASE WHEN @FueExitoso = 1 THEN 'Exitoso' ELSE 'Denegado' END,
            @MotivoAcceso, @DireccionIP, @InfoDispositivo,
            @FueExitoso, @CodigoError, @Latencia
        );
        
        -- Actualizar contador de usos de credencial
        IF @CredencialId IS NOT NULL AND @FueExitoso = 1
        BEGIN
            UPDATE [dbo].[CredencialesAcceso]
            SET NumeroUsos = NumeroUsos + 1,
                UltimoUso = GETUTCDATE()
            WHERE CredencialId = @CredencialId;
        END
        
        -- Actualizar estadÃ­sticas de cerradura
        IF @FueExitoso = 1
        BEGIN
            UPDATE [dbo].[CerradurasInteligentes]
            SET UltimaApertura = GETUTCDATE(),
                ContadorAperturas = ContadorAperturas + 1
            WHERE CerraduraId = @CerraduraId;
        END
        
        SELECT SCOPE_IDENTITY() AS RegistroId;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END