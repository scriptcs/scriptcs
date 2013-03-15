try { 
    $tools = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
	$nugetExe = "$env:ChocolateyInstall\ChocolateyInstall\nuget"
	$binPath="$env:appdata\scriptcs"

	if(!(Test-Path $binpath)){mkdir $binPath}
	Copy-Item "$tools\scriptcs\*" $binPath -force
	mkdir "$tools\nugets"
	"nuget.core","Roslyn.Compilers.Common","Roslyn.Compilers.CSharp" |
		% {.$nugetexe install $_ -o "$tools\nugets"}
	Get-childItem "$tools\nugets" -filter *.dll -recurse | % {Copy-Item $_.FullName $binPath }
	New-Item "$tools\scriptcs\scriptcs.exe.ignore" -type file -force
	Remove-Item $tools\..\lib -recurse -force
	Install-ChocolateyPath $binPath
	write-host "scriptcs.exe locted in $binpath is now in your path. " -foregroundcolor darkyellow
	write-host "You may need to open a new console for the new path to take effect. Happy scripting!" -foregroundcolor darkyellow
    Write-ChocolateySuccess 'scriptcs'
} catch {
    Write-ChocolateyFailure 'scriptcs' "$($_.Exception.Message)"
    throw 
}