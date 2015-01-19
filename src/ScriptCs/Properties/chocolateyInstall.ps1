try {
    $tools = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
    $nuget = "$env:ChocolateyInstall\ChocolateyInstall\nuget"
    $nugetPath = "$tools\nugets"

    Write-Host "Retrieving NuGet dependencies..." -ForegroundColor DarkYellow

    $dependencies = @{
        "Roslyn.Compilers.CSharp" = "1.2.20906.2";
    }

    $dependencies.GetEnumerator() | %{ &nuget install $_.Name -version $_.Value -o $nugetPath }

    Get-ChildItem $nugetPath -Filter "*.dll" -Recurse | %{ Copy-Item $_.FullName $tools -Force }
    Remove-Item $nugetPath -Recurse -Force

    if (Test-Path "$tools\..\lib") {
        Remove-Item "$tools\..\lib" -Recurse -Force
    }

    Write-ChocolateySuccess 'scriptcs'
} catch {
    Write-ChocolateyFailure 'scriptcs' "$($_.Exception.Message)"
    throw
}