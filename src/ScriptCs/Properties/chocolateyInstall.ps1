try { 
    $tools = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
    $nuget = "$env:ChocolateyInstall\ChocolateyInstall\nuget"
    $binPath = "$env:APPDATA\scriptcs"
    $nugetPath = "$tools\nugets"

    New-Item $binPath -ItemType Directory -Force | Out-Null

    Copy-Item "$tools\scriptcs\*" $binPath -Force

    Write-Host "Retrieving NuGet dependencies..." -ForegroundColor DarkYellow

    "NuGet.Core","Autofac.Mef","Roslyn.Compilers.CSharp","PowerArgs" | %{ .$nuget install $_ -o $nugetPath -nocache } | Out-Null

    Get-ChildItem $nugetPath -Filter "*.dll" -Recurse | %{ Copy-Item $_.FullName $binPath -Force }
    Remove-Item $nugetPath -Recurse -Force
    New-Item "$tools\scriptcs\scriptcs.exe.ignore" -ItemType File -Force | Out-Null
    
    if (Test-Path "$tools\..\lib") {
        Remove-Item "$tools\..\lib" -Recurse -Force
    }

    Install-ChocolateyPath $binPath
    Write-Host "scriptcs.exe has been installed to $binpath and has been added to your path." -ForegroundColor DarkYellow
    Write-Host "You may need to open a new console for the new path to take effect. Happy scripting!" -ForegroundColor DarkYellow
    Write-ChocolateySuccess 'scriptcs'
} catch {
    Write-ChocolateyFailure 'scriptcs' "$($_.Exception.Message)"
    throw 
}