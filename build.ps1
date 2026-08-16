# ====================================================================
#  CmdHelper - WPF 单文件便携版自动化构建脚本 (.NET Framework 4.6.2)
# ====================================================================
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

$rootDir = $PSScriptRoot
$srcWpfDir = Join-Path $rootDir "src-wpf"
$webDir = Join-Path $rootDir "web-singlefile"
$releaseDir = Join-Path $rootDir "release"

if (-not (Test-Path $releaseDir)) {
    New-Item -ItemType Directory -Path $releaseDir | Out-Null
}

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host " 开始构建 CmdHelper 极速助手 (.NET 4.6.2 便携单文件)" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

# 1. 编译 WPF 原生项目
Write-Host "[1/3] 正在编译 WPF 原生客户端..." -ForegroundColor Yellow
dotnet build "$srcWpfDir\CmdHelper.csproj" -c Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "✖ 编译失败，请检查错误。" -ForegroundColor Red
    exit $LASTEXITCODE
}

# 2. 复制产物
Write-Host "[2/3] 正在整理 Release 产物..." -ForegroundColor Yellow
$binReleaseDir = Join-Path $srcWpfDir "bin\Release\net462"
$portableExe = Join-Path $binReleaseDir "CmdHelper_Portable.exe"
$targetExe = Join-Path $releaseDir "CmdHelper.exe"

if (Test-Path $portableExe) {
    Copy-Item $portableExe $targetExe -Force
} else {
    $rawExe = Join-Path $binReleaseDir "CmdHelper.exe"
    Copy-Item $rawExe $targetExe -Force
}

# 同步单文件 Web 版
$targetHtml = Join-Path $releaseDir "CmdHelper_Web.html"
Copy-Item (Join-Path $webDir "index.html") $targetHtml -Force

# 复制基础命令库
Copy-Item (Join-Path $rootDir "data\commands.json") (Join-Path $releaseDir "commands.json") -Force

Write-Host "[3/3] 构建完成！产物位于 release 目录：" -ForegroundColor Green
Get-ChildItem -Path $releaseDir | Select-Object Name, Length, LastWriteTime | Format-Table -AutoSize

Write-Host "`n✔ 绿色单文件 EXE: release\CmdHelper.exe (兼容 Windows Server 2016+ / Win10 / Win11)" -ForegroundColor Green
Write-Host "✔ 离线单文件 Web: release\CmdHelper_Web.html (任意浏览器秒开)`n" -ForegroundColor Green
