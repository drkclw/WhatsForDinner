param(
    [string[]]$Paths = @('infra')
)

$ErrorActionPreference = 'Stop'

$patterns = @(
    'Microsoft\.CognitiveServices/accounts',
    'kind\s*[:=]\s*[\''\"]OpenAI[\''\"]',
    'kind\s*[:=]\s*[\''\"]AIServices[\''\"]'
)

$matches = foreach ($path in $Paths) {
    if (-not (Test-Path $path)) {
        continue
    }

    Get-ChildItem -Path $path -Recurse -File | Select-String -Pattern $patterns -ErrorAction SilentlyContinue
}

if ($matches) {
    $matches | ForEach-Object {
        Write-Error ("Disallowed OpenAI-related resource reference found in {0}:{1}" -f $_.Path, $_.LineNumber)
    }
}

Write-Host 'No OpenAI resource references were detected in scanned paths.'