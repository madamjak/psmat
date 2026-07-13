<#
.SYNOPSIS
    Removes a directory from the PATH environment variable.

.PARAMETER DirPath
    Full path to the directory you want to remove from PATH.

.PARAMETER Scope
    "User" (default) updates only the current user's PATH.
    "Machine" updates the system-wide PATH (requires admin rights).

.PARAMETER DeleteDir
    Optional flag whether to also delete the DirPath

.EXAMPLE
    .\uninstall-psmat.ps1 -DirPath "C:\Tools\PSMat" -Scope User
#>

param (
    [Parameter(Mandatory = $true)]
    [string]$DirPath,

    [ValidateSet("User", "Machine")]
    [string]$Scope = "User",
    
    [switch]$DeleteDir
)

try {
    
	.\Remove-FromPath.ps1 -DirPath $DirPath -Scope $Scope
    
    if ($DeleteDir){
        
        
        # Remove ALL folders recursively
        Get-ChildItem -Path $DirPath -Recurse -Directory |
        ForEach-Object {
            try {
                Remove-Item $_.FullName -Recurse -Force -ErrorAction Stop
                Write-Host "Deleted $($_.FullName)" -ForegroundColor Green
            }
            catch {
                Write-Host "Failed to delete $($_.FullName): $_" -ForegroundColor Red
            }
        }
        
        Remove-Item $DirPath -Recurse -Force -ErrorAction Stop
        Write-Host "Deleted $($_.FullName)" -ForegroundColor Green
    }
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
}


