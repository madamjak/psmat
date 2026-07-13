<#
.SYNOPSIS
    Removes a directory from the PATH environment variable.

.PARAMETER DirPath
    Full path to the directory you want to remove from PATH.

.PARAMETER Scope
    "User" (default) updates only the current user's PATH.
    "Machine" updates the system-wide PATH (requires admin rights).

.EXAMPLE
    .\Remove-FromPath.ps1 -DirPath "C:\Tools" -Scope User
#>

param (
    [Parameter(Mandatory = $true)]
    [string]$DirPath,

    [ValidateSet("User", "Machine")]
    [string]$Scope = "User"
)

try {
    # Normalize the directory path (remove trailing slash, resolve full path)
    $DirPath = (Resolve-Path $DirPath).Path.TrimEnd('\')

    # Get current PATH
    $target = if ($Scope -eq "Machine") { "Machine" } else { "User" }
    $currentPath = [Environment]::GetEnvironmentVariable("PATH", $target)

    if (-not $currentPath) {
        Write-Host "PATH is empty for $Scope scope." -ForegroundColor Yellow
        return
    }

    # Split PATH into entries, remove matching ones (case-insensitive)
    $updatedEntries = $currentPath.Split(';') |
        Where-Object { $_.TrimEnd('\') -ne $DirPath -and $_ -ne "" }

    if ($updatedEntries.Count -eq ($currentPath.Split(';').Count)) {
        Write-Host "Directory not found in PATH: $DirPath" -ForegroundColor Yellow
    }
    else {
        # Join back into a single PATH string
        $newPath = ($updatedEntries -join ';').TrimEnd(';')
        [Environment]::SetEnvironmentVariable("PATH", $newPath, $target)
        Write-Host "Removed from $Scope PATH: $DirPath" -ForegroundColor Green
        Write-Host "Restart PowerShell or log out/in for changes to take effect." -ForegroundColor Cyan
    }
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
