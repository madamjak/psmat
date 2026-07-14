$currentDir = Get-Location

try {
	
	# Rebuild PSMat
	./Build-DotNet.ps1 -ProjectPath ".\..\src\PSMat.sln" -Configuration Debug
	
	if ($LASTEXITCODE -ne 0){
		throw "PSMat Build failed."
	}

}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
finally {
	Set-Location $currentDir
}