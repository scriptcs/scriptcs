try {
    $binPath = "$env:APPDATA\scriptcs"

    Write-Host "Removing '$binPath'..." -ForegroundColor DarkYellow

    if (Test-Path $binPath) {
        Remove-Item $binPath -Recurse -Force
    }

    Write-Host "'$binPath' has been removed." -ForegroundColor DarkYellow
    Write-ChocolateySuccess 'scriptcs'
} catch {
    Write-ChocolateyFailure 'scriptcs' "$($_.Exception.Message)"
    throw 
}