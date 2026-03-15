/*
    Post-Deployment Script
    Ejecuta los scripts de seed de datos iniciales del sistema.
    Este archivo es el punto de entrada; se ejecuta automaticamente
    al hacer deploy del proyecto de base de datos.
*/

:r .\Seed_EstadosDispositivo.sql
:r .\Seed_EstadosHabitacion.sql
