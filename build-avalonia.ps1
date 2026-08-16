# ====================================================================
#  CmdHelper - Avalonia UI 跨平台多目标自动化发布脚本 (.NET 8)
# ====================================================================
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

$rootDir = $PSScriptRoot
$srcAvaloniaDir = Join-Path $rootDir "src-avalonia"
$releaseDir = Join-Path $rootDir "release"

if (-not (Test-Path $releaseDir)) {
    New-Item -ItemType Directory -Path $releaseDir | Out-Null
}

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host " 开始构建 CmdHelper Avalonia UI 跨平台版本 (.NET 8)" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

# 1. 编译 Windows x64 独立单文件版
Write-Host "`n[1/3] 正在发布 Windows (win-x64) 独立单文件版..." -ForegroundColor Yellow
dotnet publish "$srcAvaloniaDir\CmdHelper.Avalonia.csproj" `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -o "$releaseDir\win-x64"

if ($LASTEXITCODE -eq 0) {
    Copy-Item "$releaseDir\win-x64\CmdHelper.exe" "$releaseDir\CmdHelper-win-x64.exe" -Force
    Remove-Item -Recurse -Force "$releaseDir\win-x64"
    Write-Host "✔ Windows 单文件生成成功: release\CmdHelper-win-x64.exe" -ForegroundColor Green
} else {
    Write-Host "✖ Windows 发布失败" -ForegroundColor Red
}

# 2. 编译 Linux x64 独立单文件版 (ELF)
Write-Host "`n[2/3] 正在发布 Linux (linux-x64) 独立单文件版 (ELF)..." -ForegroundColor Yellow
dotnet publish "$srcAvaloniaDir\CmdHelper.Avalonia.csproj" `
    -c Release `
    -r linux-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -o "$releaseDir\linux-x64"

if ($LASTEXITCODE -eq 0) {
    Copy-Item "$releaseDir\linux-x64\CmdHelper" "$releaseDir\CmdHelper-linux-x64" -Force
    Remove-Item -Recurse -Force "$releaseDir\linux-x64"
    Write-Host "✔ Linux 单文件生成成功: release\CmdHelper-linux-x64 (在 Linux 机器上 chmod +x 即可直接运行)" -ForegroundColor Green
} else {
    Write-Host "✖ Linux 发布失败" -ForegroundColor Red
}

# 3. 输出汇总
Write-Host "`n[3/3] 全部构建完成！产物列表：" -ForegroundColor Cyan
Get-ChildItem -Path $releaseDir | Select-Object Name, Length, LastWriteTime | Format-Table -AutoSize
