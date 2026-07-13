try {
    # Run the EXE and wait for it to finish
    # & "C:\Path\To\MyApp.exe" "arg1" "arg2"

	$currentDir = Get-Location
	Set-Location ".\src\PisaciStroj.Testy\bin\Release\net10.0"
    # Or capture its output
    $rawOutput = & "./PSMat.Testy.exe"
    
	# Ensure output is treated as a single string
    if ($rawOutput -is [array]) {
        # PowerShell sometimes already splits lines into an array
        $lines = $rawOutput
    }
    else {
        # Split manually on both Windows (\r\n) and Unix (\n) newlines
        $lines = $rawOutput -split "(\r?\n)"
        # Remove empty entries caused by split
        $lines = $lines | Where-Object { $_.Trim() -ne "" }
    }

    # Print each line with a prefix
    foreach ($line in $lines) {
        Write-Host "$line"
    }
	
	Set-Location $currentDir
	
	if ($LASTEXITCODE = 1){
		Write-Error "Unit tests failed"
	}
}
catch {
    Write-Error "Error running EXE: $_"
}
