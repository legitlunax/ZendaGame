param(
	[ValidateSet('Web', 'Android')]
	[string]$Target = 'Web'
)
$ErrorActionPreference = 'Stop'
$engine = 'E:\Godot\Godot_v4.7.2-stable_win64_console.exe'
$project = 'E:\Godot\GenesisIdleRPG'
$folder = Join-Path $project ('build\' + $Target.ToLowerInvariant())
New-Item -ItemType Directory -Force $folder | Out-Null
if ($Target -eq 'Web') {
	& $engine --headless --path $project --export-release Web (Join-Path $folder 'index.html')
} else {
	& $engine --headless --path $project --export-debug Android (Join-Path $folder 'genesis.apk')
}
if ($LASTEXITCODE -ne 0) {
	throw 'Export failed. See engine output and PLATFORM_GUIDE.md for prerequisites.'
}
Write-Output "Export completed: $folder"