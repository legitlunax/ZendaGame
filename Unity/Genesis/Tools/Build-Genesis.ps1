param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('Windows', 'Android', 'WebGL')]
    [string]$Platform,
    [string]$Unity = 'C:\Program Files\Unity\Hub\Editor\6000.6.7f1\Editor\Unity.exe'
)
$ErrorActionPreference = 'Stop'
$project = Split-Path -Parent $PSScriptRoot
if (-not (Test-Path -LiteralPath $Unity)) { throw "Unity editor not found: $Unity" }
# Unity must own the project exclusively. Do not kill an editor or discard unsaved scenes.
$lock = Join-Path $project 'Temp\UnityLockfile'
if (Test-Path -LiteralPath $lock) { throw 'Close this project in Unity first. Preserve unsaved work yourself; this script does not save or close scenes.' }
$targets = @{ Windows = 'Win64'; Android = 'Android'; WebGL = 'WebGL' }
$builds = Join-Path $project 'Builds'
New-Item -ItemType Directory -Force -Path $builds | Out-Null
$log = Join-Path $builds ($Platform + '-build.log')
$arguments = @('-batchmode', '-quit', '-projectPath', ('"' + $project + '"'),
    '-buildTarget', $targets[$Platform], '-executeMethod', ('GenesisBuild.' + $Platform),
    '-logFile', ('"' + $log + '"'))
$process = Start-Process -FilePath $Unity -ArgumentList $arguments -PassThru -Wait
if ($process.ExitCode -ne 0) { throw "Unity build failed ($($process.ExitCode)). See $log" }
Write-Output "Build completed. See $log and the target output under $builds."