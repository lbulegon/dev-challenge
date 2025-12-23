@echo off
chcp 65001 >nul
echo ========================================
echo   Executando Desafio Umbler
echo ========================================
echo.
echo Diretório: %CD%
echo.
echo Comando: dotnet run --project src/Desafio.Umbler/Desafio.Umbler.csproj
echo.
echo ========================================
echo.

dotnet run --project src/Desafio.Umbler/Desafio.Umbler.csproj

pause

