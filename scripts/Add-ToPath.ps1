<#
.SYNOPSIS
    Adds a directory containing an EXE to the PATH environment variable.

.PARAMETER ExePath
    Full path to the EXE file you want to add to PATH.

.PARAMETER Scope
    "User" (default) updates only the current user's PATH.
    "Machine" updates the system-wide PATH (requires admin rights).

.EXAMPLE
    .\Add-ToPath.ps1 -ExePath "C:\Tools\myapp.exe" -Scope User
#>

param (
    [Parameter(Mandatory = $true)]
    [string]$ExePath,

    [ValidateSet("User", "Machine")]
    [string]$Scope = "User"
)

try {
    # Validate that the file exists
    if (-not (Test-Path $ExePath -PathType Leaf)) {
        throw "The specified EXE file does not exist: $ExePath"
    }

    # Get the directory of the EXE
    $dirPath = Split-Path -Path $ExePath -Parent

    # Get current PATH
    $target = if ($Scope -eq "Machine") { "Machine" } else { "User" }
    $currentPath = [Environment]::GetEnvironmentVariable("PATH", $target)

    # Check if already in PATH
    if ($currentPath.Split(';') -contains $dirPath) {
        Write-Host "Directory is already in PATH: $dirPath" -ForegroundColor Yellow
    }
    else {
        # Append directory to PATH
        $newPath = $currentPath.TrimEnd(';') + ";" + $dirPath
        [Environment]::SetEnvironmentVariable("PATH", $newPath, $target)
        Write-Host "Added to $Scope PATH: $dirPath" -ForegroundColor Green
        Write-Host "Restart PowerShell or log out/in for changes to take effect." -ForegroundColor Cyan
    }
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
