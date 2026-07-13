<#
.SYNOPSIS
    Builds a .NET Core / .NET application using the dotnet CLI.

.DESCRIPTION
    This script validates the project path, checks if the dotnet CLI is installed,
    restores dependencies, and builds the project in the specified configuration.

.PARAMETER ProjectPath
    Path to the .csproj or .sln file.

.PARAMETER Configuration
    Build configuration (Debug or Release). Default is Release.

.EXAMPLE
    ./Build-DotNet.ps1 -ProjectPath "./MyApp/MyApp.csproj" -Configuration Release
#>

param (
    [Parameter(Mandatory = $true)]
    [string]$ProjectPath,

    [Parameter(Mandatory = $false)]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release"
)

# Function to check if dotnet CLI is installed
function Test-DotNetInstalled {
    try {
        $null = dotnet --version 2>$null
        return $true
    }
    catch {
        return $false
    }
}

$currentDir = Get-Location

# Go to the solution directory
$FolderPath = Split-Path -Path $ProjectPath
Set-Location $FolderPath

$ProjectPath = Split-Path -Path $ProjectPath -Leaf

# Remove ALL bin and obj folders recursively
Get-ChildItem -Path . -Include bin,obj -Recurse -Directory |
    ForEach-Object {
        try {
            Remove-Item $_.FullName -Recurse -Force -ErrorAction Stop
            Write-Host "Deleted $($_.FullName)" -ForegroundColor Green
        }
        catch {
            Write-Host "Failed to delete $($_.FullName): $_" -ForegroundColor Red
        }
    }

# Validate dotnet CLI
if (-not (Test-DotNetInstalled)) {
    Write-Error "The .NET SDK (dotnet CLI) is not installed or not in PATH."
    exit 1
}

# Validate project path
if (-not (Test-Path $ProjectPath)) {
    Write-Error "The specified project or solution file does not exist: $ProjectPath"
    exit 1
}

Write-Host "Restoring dependencies for $ProjectPath..." -ForegroundColor Cyan
dotnet restore $ProjectPath
# if (-not (dotnet restore $ProjectPath)) {
    # Write-Error "Restore failed."
    # exit 1
#}

if ($LASTEXITCODE -ne 0){
	Write-Error "Restore failed."
}

Write-Host "Building project in $Configuration mode..." -ForegroundColor Cyan
dotnet build -v:detailed -p:WarningLevel=5 $ProjectPath --configuration $Configuration --no-restore
# if (-not (dotnet build -v:detailed -p:WarningLevel=5 $ProjectPath --configuration $Configuration --no-restore)) {
    # Write-Error "Build failed."
    # exit 1
#}

if ($LASTEXITCODE -ne 0){
	Write-Error "Build failed."
}
else{
	Write-Host "Build succeeded!" -ForegroundColor Green
}

Set-Location $currentDir

