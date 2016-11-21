try {
    $tools = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"

    if (Test-Path "$tools\..\lib") {
        Remove-Item "$tools\..\lib" -Recurse -Force
    }

    # Handle upgrade from previous packages that installed to the %AppData%/scriptcs folders.
    $oldPaths = @(
        "$env:APPDATA\scriptcs",
        "$env:LOCALAPPDATA\scriptcs"
    )

    $oldPaths | foreach {
        # Remove the old user-specific scriptcs folders.
        if (Test-Path $_) {
            Remove-Item $_ -Recurse -Force
        }

        # Remove the user-specific path that got added in previous installs.
        # There's no Uninstall-ChocolateyPath yet so we need to do it manually.
        # https://github.com/chocolatey/chocolatey/issues/97
        $envPath = $env:PATH
        if ($envPath.ToLower().Contains($_.ToLower())) {
            $userPath = [Environment]::GetEnvironmentVariable("Path","User")
            if($userPath) {
                $actualPath = [System.Collections.ArrayList]($userPath).Split(";")
                $actualPath.Remove($_)
                $newPath =  $actualPath -Join ";"
                [Environment]::SetEnvironmentVariable("Path", $newPath, "User")
            }
        }

        Write-Host "'$_' has been removed." -ForegroundColor DarkYellow
    }
    Update-SessionEnvironment
    # End upgrade handling.

    Write-ChocolateySuccess 'scriptcs'
} catch {
    Write-ChocolateyFailure 'scriptcs' "$($_.Exception.Message)"
    throw
}