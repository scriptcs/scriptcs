try {
    $paths = @(
        "$env:APPDATA\scriptcs",
        "$env:LOCALAPPDATA\scriptcs"
    )

    $paths | foreach {
        if (Test-Path $_) {
            Remove-Item $_ -Recurse -Force
        }

        Write-Host "'$_' has been removed." -ForegroundColor DarkYellow
    }

    Write-ChocolateySuccess 'scriptcs'
} catch {
    Write-ChocolateyFailure 'scriptcs' "$($_.Exception.Message)"
    throw 
}