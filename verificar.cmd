@echo off
REM Compila toda la solucion y ejecuta las pruebas automatizadas.
REM El resultado completo queda en verificacion.log (en esta misma carpeta).
chcp 65001 >nul
cd /d "%~dp0"

echo.
echo  Hotel Fast and Furious - verificacion
echo  -------------------------------------
echo  1) Compilando la solucion. La primera vez descarga paquetes NuGet: puede tardar unos minutos.
echo.

powershell -NoProfile -ExecutionPolicy Bypass -Command ^
  "$log = 'verificacion.log';" ^
  "('===== ' + (Get-Date) + ' =====') | Out-File $log -Encoding utf8;" ^
  "dotnet --version 2>&1 | Tee-Object -FilePath $log -Append;" ^
  "dotnet build HotelSystem.slnx -c Debug 2>&1 | Tee-Object -FilePath $log -Append;" ^
  "$build = $LASTEXITCODE;" ^
  "if ($build -ne 0) { 'COMPILACION CON ERRORES' | Tee-Object -FilePath $log -Append; exit 1 };" ^
  "Write-Host ''; Write-Host ' 2) Ejecutando las pruebas automatizadas...'; Write-Host '';" ^
  "dotnet test HotelSystem.Tests\HotelSystem.Tests.csproj --no-build --logger 'console;verbosity=normal' 2>&1 | Tee-Object -FilePath $log -Append;" ^
  "if ($LASTEXITCODE -ne 0) { 'HAY PRUEBAS FALLIDAS' | Tee-Object -FilePath $log -Append; exit 2 };" ^
  "'TODO CORRECTO: compila y pasan todas las pruebas' | Tee-Object -FilePath $log -Append"

echo.
if %ERRORLEVEL%==0 (
  echo  Listo: la solucion compila y todas las pruebas pasan.
) else (
  echo  Hubo errores. Revise verificacion.log o compartalo para corregirlos.
)
echo.
pause
