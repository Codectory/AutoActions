function buildVS
{
    param
    (
        [parameter(Mandatory=$true)]
        [String] $path,
		
		[parameter(Mandatory=$true)]
        [String] $version,

        [parameter(Mandatory=$false)]
        [bool] $nuget = $true
        
    )
    process
    {
        $msBuildExe = 'C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\msbuild.exe'

        Write-Host "Building x64 $($path)" -foregroundcolor green
        & "$($msBuildExe)" "$($path)" /t:Build /m /property:Configuration=Release /property:Platform=x64
		Write-Host "Building x86 $($path)" -foregroundcolor green
        & "$($msBuildExe)" "$($path)" /t:Build /m /property:Configuration=Release /property:Platform=x86
		
		$x64zip = ".\Releases\Release_AutoActions_$($version)_x64.zip"
		$x86zip = ".\Releases\Release_AutoActions_$($version)_x86.zip"



        Write-Host "Creating Zip x64 $($x64zip)" -foregroundcolor green

        if (Test-Path $x64zip -PathType leaf)
        {del $x64zip}

        Get-ChildItem -Path ".\Source\Release_x64" | 
        Where-Object {$_.PsIsContainer -eq $true -or $_.Extension -eq ".exe" -or $_.Extension -eq ".config" -or $_.Extension -eq ".dll" -or $_.Name -eq "UpdateData.json"   } | Compress-Archive -DestinationPath $x64zip

	    Write-Host "Creating Zip x86 $($x86ip)" -foregroundcolor green
        if (Test-Path $x86zip -PathType leaf)
        {del $x86zip}

        Get-ChildItem -Path ".\Source\Release_x86" | 
        Where-Object {$_.PsIsContainer -eq $true -or $_.Extension -eq ".exe" -or $_.Extension -eq ".config" -or $_.Extension -eq ".dll"  -or $_.Name -eq "UpdateData.json"  } | Compress-Archive -DestinationPath $x86zip
        Write-Host "Finished!" -foregroundcolor green

    }
}



buildVS .\Source\AutoActions.sln