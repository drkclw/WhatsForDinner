param(
    [string[]]$Paths = @('infra')
)

$ErrorActionPreference = 'Stop'

$patterns = @(
    'Microsoft\.CognitiveServices/accounts',
    'kind\s*[:=]\s*[\''\"]OpenAI[\''\"]',
    'kind\s*[:=]\s*[\''\"]AIServices[\''\"]'
)

$pathMatches = foreach ($path in $Paths) {
    if (-not (Test-Path $path)) {
        continue
    }

    Get-ChildItem -Path $path -Recurse -File | Select-String -Pattern $patterns -ErrorAction SilentlyContinue
}

if ($pathMatches) {
    $pathMatches | ForEach-Object {
        Write-Error ("Disallowed OpenAI-related resource reference found in {0}:{1}" -f $_.Path, $_.LineNumber)
    }
}

Write-Output 'No OpenAI resource references were detected in scanned paths.'