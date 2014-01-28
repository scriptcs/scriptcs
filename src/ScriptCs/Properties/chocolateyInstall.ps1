try { 
    $tools = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
    $nuget = "$env:ChocolateyInstall\ChocolateyInstall\nuget"
    $binPath = "$env:chocolatey_bin_root\scriptcs"
    $nugetPath = "$tools\nugets"

    New-Item $binPath -ItemType Directory -Force | Out-Null

    Copy-Item "$tools\scriptcs\*" $binPath -Force

    Write-Host "Retrieving NuGet dependencies..." -ForegroundColor DarkYellow

    $dependencies = @{
        "Roslyn.Compilers.CSharp" = "1.2.20906.2";
    }

    $dependencies.GetEnumerator() | %{ &nuget install $_.Name -version $_.Value -o $nugetPath }

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