Param( $outName )

if ( [string]::IsNullOrEmpty($outName) ) {
    $outName = "bin"
}

$CurrentDirectory = Split-Path $MyInvocation.MyCommand.Path -Parent
$OutBinDirectory = "$CurrentDirectory\$outName"
$Framework = "net6.0-windows"
$Profile = "Debug"

$CookInfoViewerReleaseDirectory = "$CurrentDirectory\CoonInformationViewer\bin\$Profile\$Framework"
$UpdaterReleaseDirectory = "$CurrentDirectory\Updater\bin\$Profile\$Framework"

#SavannahManager
xcopy /Y $CookInfoViewerReleaseDirectory\*.dll $OutBinDirectory\
xcopy /Y $CookInfoViewerReleaseDirectory\*.exe $OutBinDirectory\
xcopy /Y $CookInfoViewerReleaseDirectory\*.deps.json $OutBinDirectory\
xcopy /Y $CookInfoViewerReleaseDirectory\*.runtimeconfig.json $OutBinDirectory\
Copy-Item -Path $CookInfoViewerReleaseDirectory\runtimes\ -Destination "$OutBinDirectory\" -Recurse -Force

# Updater
xcopy /Y $UpdaterReleaseDirectory\*.dll "$OutBinDirectory\Updater\"
xcopy /Y $UpdaterReleaseDirectory\*.exe "$OutBinDirectory\Updater\"
xcopy /Y $UpdaterReleaseDirectory\*.deps.json "$OutBinDirectory\Updater\"
xcopy /Y $UpdaterReleaseDirectory\*.runtimeconfig.json "$OutBinDirectory\Updater\"