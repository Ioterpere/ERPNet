<#
.SYNOPSIS
    Genera FilterExtensions.g.cs leyendo únicamente Generated/ErpNetApiClient.g.cs.

    Algoritmo:
    1. Extrae las clases DTO *Filter (propiedades PascalCase) del cliente generado.
    2. Extrae los métodos de cada interface (params camelCase, sin overload CT).
    3. Para cada método busca el *Filter DTO con más propiedades en común
       con los params del método (intersección, case-insensitive).
    4. Genera un extension method que pasa solo los params coincidentes como
       f.Prop; si el spec se desincroniza temporalmente los nuevos campos
       simplemente no se pasan hasta que se regenere.
#>
param(
    [Parameter(Mandatory)][string]$ClientPath,
    [Parameter(Mandatory)][string]$OutputPath
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path $ClientPath)) {
    Write-Host "→ $ClientPath no encontrado, saltando FilterExtensions."
    exit 0
}

$clientLines = Get-Content $ClientPath -Encoding UTF8

function ToPascal([string]$s) {
    if (-not $s) { return $s }
    return $s.Substring(0, 1).ToUpper() + $s.Substring(1)
}

# ── 1. Parsear clases *Filter → propiedades PascalCase ───────────
$dtoProps   = @{}   # dtoName -> List<string>  (PascalCase prop names)
$curClass   = $null
$braceDepth = 0
$bodyOpen   = $false

foreach ($line in $clientLines) {
    # Inicio de clase *Filter
    $m = [regex]::Match($line, 'public partial class (\w+Filter)\b')
    if ($m.Success) {
        $curClass   = $m.Groups[1].Value
        $dtoProps[$curClass] = [System.Collections.Generic.List[string]]::new()
        $braceDepth = 0
        $bodyOpen   = $false
        continue
    }

    if ($null -eq $curClass) { continue }

    # Contar llaves para detectar fin de clase
    $opens  = ([regex]::Matches($line, '\{')).Count
    $closes = ([regex]::Matches($line, '\}')).Count
    $braceDepth += $opens - $closes
    if (-not $bodyOpen -and $opens -gt 0) { $bodyOpen = $true }
    if ($bodyOpen -and $braceDepth -le 0) { $curClass = $null; $bodyOpen = $false; continue }

    # Propiedad auto: `public TYPE Name { get; set; }`
    $m2 = [regex]::Match($line.Trim(), '^public [\w?<>\[\].]+\s+(\w+)\s+\{\s*get;\s*set;\s*\}')
    if ($m2.Success) { $dtoProps[$curClass].Add($m2.Groups[1].Value) }
}

# ── 2. Parsear interfaces → métodos (sin overload CancellationToken) ─
$interfaces  = @{}   # ifaceName -> List<hashtable>
$curIface    = $null

foreach ($line in $clientLines) {
    $m = [regex]::Match($line, 'public partial interface (I\w+)')
    if ($m.Success) {
        $curIface = $m.Groups[1].Value
        $interfaces[$curIface] = [System.Collections.Generic.List[hashtable]]::new()
        continue
    }
    if ($null -eq $curIface) { continue }

    if ($line -notmatch 'System\.Threading\.Tasks\.Task') { continue }
    if ($line -match 'CancellationToken')                 { continue }
    if ($line -notmatch 'Async')                          { continue }
    if ($line -notmatch ';')                              { continue }

    $m2 = [regex]::Match($line.Trim(),
        '^System\.Threading\.Tasks\.(Task(?:<(\w+)>)?) (\w+)\(([^)]*)\);$')
    if (-not $m2.Success) { continue }

    $paramNames = @($m2.Groups[4].Value -split ',' |
        ForEach-Object { ($_.Trim() -split '\s+')[-1] } |
        Where-Object   { $_ })

    $interfaces[$curIface].Add(@{
        MethodName  = $m2.Groups[3].Value
        ReturnType  = $m2.Groups[1].Value
        ParamNames  = $paramNames
    })
}

# ── 3. Matching y generación de extensiones ──────────────────────
$extensions = [System.Collections.Generic.List[hashtable]]::new()

foreach ($ifaceName in ($interfaces.Keys | Sort-Object)) {
    foreach ($method in $interfaces[$ifaceName]) {
        $paramSet = [System.Collections.Generic.HashSet[string]]::new(
            [string[]]$method.ParamNames, [System.StringComparer]::OrdinalIgnoreCase)

        $bestDto     = $null
        $bestScore   = 1           # requerimos >= 2, empezamos en 1
        $bestDtoSize = [int]::MaxValue

        foreach ($dtoName in $dtoProps.Keys) {
            $score = 0
            foreach ($prop in $dtoProps[$dtoName]) {
                if ($paramSet.Contains($prop)) { $score++ }
            }
            $dtoSize = $dtoProps[$dtoName].Count
            # Primario: más props coincidentes; tiebreak: DTO más pequeño (más específico)
            if ($score -ge 2 -and ($score -gt $bestScore -or ($score -eq $bestScore -and $dtoSize -lt $bestDtoSize))) {
                $bestDto     = $dtoName
                $bestScore   = $score
                $bestDtoSize = $dtoSize
            }
        }
        if ($null -eq $bestDto) { continue }

        # Args: params del método que coinciden con props del DTO, en orden del método
        $dtoPropSet = [System.Collections.Generic.HashSet[string]]::new(
            [string[]]$dtoProps[$bestDto], [System.StringComparer]::OrdinalIgnoreCase)

        $args = @($method.ParamNames |
            Where-Object   { $dtoPropSet.Contains($_) } |
            ForEach-Object { "f.$(ToPascal $_)" })

        $extensions.Add(@{
            ClientInterface = $ifaceName
            MethodName      = $method.MethodName
            ReturnType      = $method.ReturnType
            DtoName         = $bestDto
            Args            = $args
        })
    }
}

# ── 4. Emitir código ─────────────────────────────────────────────
$sb = [System.Text.StringBuilder]::new()
[void]$sb.AppendLine('// <auto-generated/>')
[void]$sb.AppendLine('// Generado por Scripts/GenerateFilterExtensions.ps1 — no editar manualmente.')
[void]$sb.AppendLine('namespace ERPNet.ApiClient;')
[void]$sb.AppendLine('')
[void]$sb.AppendLine('public static class FilterExtensions')
[void]$sb.AppendLine('{')

$first = $true
foreach ($ext in $extensions) {
    if (-not $first) { [void]$sb.AppendLine('') }
    $first   = $false
    $argsStr = ($ext.Args + @('ct')) -join ', '
    [void]$sb.AppendLine("    public static $($ext.ReturnType) $($ext.MethodName)(")
    [void]$sb.AppendLine("        this $($ext.ClientInterface) client, $($ext.DtoName) f, CancellationToken ct = default)")
    [void]$sb.AppendLine("        => client.$($ext.MethodName)($argsStr);")
}

[void]$sb.Append('}')

$null = New-Item -ItemType Directory -Force -Path (Split-Path $OutputPath)
Set-Content -Path $OutputPath -Value $sb.ToString() -Encoding UTF8
Write-Host "→ FilterExtensions.g.cs generado ($($extensions.Count) métodos)"
