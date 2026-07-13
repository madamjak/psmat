<#
.SYNOPSIS
    Builds or rebuilds PSMat.sln and creates PSMat folder in specified location and adds given directory to the PATH environment variable.

.PARAMETER ProgramPath
    Path to directory where PSMat is added

.PARAMETER Scope
    "User" (default) updates only the current user's PATH.
    "Machine" updates the system-wide PATH (requires admin rights).
	
.PARAMETER Overwrite
	Optional switch parameter to overwrite PSMat folder contents in ProgramPath

.PARAMETER Rebuild
	Optional switch parameter to rebuild PSMat

.EXAMPLE
    .\install-psmat.ps1 -ProgramPath "C:\Tools" -Scope User
#>

param (
    [Parameter(Mandatory = $true)]
    [string]$ProgramPath,

    [ValidateSet("User", "Machine")]
    [string]$Scope = "User",
	
	[switch]$Overwrite,
	
	[switch]$Rebuild
)

$currentDir = Get-Location

try {
	
	if($Rebuild){
		# Rebuild PSMat
		./Build-DotNet.ps1 -ProjectPath ".\..\src\PSMat.sln" -Configuration Release
		
		if ($LASTEXITCODE -ne 0){
			throw "PSMat Build failed."
		}
	}
	
	# create PSMat folder
	$appPath = Join-Path -Path $ProgramPath -ChildPath "PSMat" 
	if (-not (Test-Path $appPath)) {
		New-Item -Path $appPath -ItemType Directory
	}
	
	$binPath = ".\..\src\psmat\bin\Release\net10.0"
	if (-not (Test-Path $binPath)) {
		throw "$binPath does not exist"
	}
	
	$binaries = Get-ChildItem -Path $binPath
	if ($binaries.Count -eq 0) {
		throw "PSMat binaries not found"
	}
	
	$binPath = Join-Path -Path $binPath -ChildPath "*" 
	
	# check that target empty
	$items = Get-ChildItem -Path $appPath
	if (-Not $Overwrite -and $items.Count -ne 0) {
		throw "'$appPath' directory is not empty"
	}
	
	# Copy files (recursive, overwrite existing)
	Copy-Item -Path $binPath `
			  -Destination $appPath `
			  -Recurse `
			  -Force `
			  -ErrorAction Stop
	
	# add to PATH
	$ExePath = Join-Path -Path $appPath -ChildPath "PSMat.exe"
	
	.\Add-ToPath.ps1 -ExePath $ExePath -Scope $Scope
    
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
finally {
	Set-Location $currentDir
}