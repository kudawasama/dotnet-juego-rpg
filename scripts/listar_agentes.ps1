# Script para listar todos los comentarios @Agente* en el proyecto

# Ruta base del proyecto
$basePath = "c:\Users\jose.cespedes\Documents\GitHub\dotnet-juego-rpg"

# Buscar todos los archivos .cs y extraer comentarios con @Agente*
Get-ChildItem -Path $basePath -Recurse -Filter "*.cs" | ForEach-Object {
    $filePath = $_.FullName
    Select-String -Path $filePath -Pattern "@Agente" | ForEach-Object {
        [PSCustomObject]@{
            Archivo = $filePath
            Linea = $_.LineNumber
            Comentario = $_.Line.Trim()
        }
    }
} | Export-Csv -Path "$basePath\reporte_agentes.csv" -NoTypeInformation

Write-Host "Reporte generado: $basePath\reporte_agentes.csv"