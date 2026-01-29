# ------------------------------------------------------------
# Fix OpenAPI version in generated Swagger JSON
# Changes "openapi": "3.0.4" → "3.0.3"
# ------------------------------------------------------------

Write-Host "🔧 Fixing OpenAPI version in Swagger output..."

# Get project root (folder where this script lives)
$projectRoot = $PSScriptRoot

# Build output directory (adjust if needed)
$buildDir = Join-Path $projectRoot "bin\Release\net10.0"

# Possible swagger file names
$swaggerFiles = @(
    "swagger.json",
    "openapi.json",
    "HotelApi.json"
)

$found = $false

foreach ($file in $swaggerFiles) {
    $swaggerPath = Join-Path $buildDir $file

    Write-Host "🔍 Checking: $swaggerPath"

    if (Test-Path $swaggerPath) {
        Write-Host "📄 Found Swagger file: $swaggerPath"

        (Get-Content $swaggerPath) `
            -replace '"openapi"\s*:\s*"3\.0\.4"', '"openapi": "3.0.3"' |
            Set-Content $swaggerPath

        Write-Host "✅ Successfully changed OpenAPI version to 3.0.3"
        $found = $true
        break
    }
}

if (-not $found) {
    Write-Host "⚠️  Could not find a Swagger JSON file in:"
    Write-Host "   $buildDir"
    Write-Host "   Searched for: $($swaggerFiles -join ', ')"
}
