# 1. プロジェクトファイルを修正（アイコン参照を削除）
$csprojContent = @'
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net6.0-windows</TargetFramework>
    <Nullable>enable</Nullable>
    <UseWPF>true</UseWPF>
  </PropertyGroup>

</Project>
'@
$csprojContent | Out-File -FilePath "MomotetsuGame.csproj" -Encoding UTF8

# 2. NuGetパッケージを正しくインストール
Write-Host "NuGetパッケージを再インストールしています..." -ForegroundColor Green

# まず、既存のパッケージ参照をクリア
dotnet restore

# パッケージを一つずつ追加
dotnet add package Microsoft.Extensions.DependencyInjection --version 7.0.0
dotnet add package Microsoft.Xaml.Behaviors.Wpf --version 1.1.39
dotnet add package ReactiveUI.WPF --version 18.3.1  # .NET 6.0対応バージョン
dotnet add package SkiaSharp.Views.WPF --version 2.88.6
dotnet add package Serilog --version 3.1.1
dotnet add package Serilog.Sinks.File --version 5.0.0
dotnet add package Newtonsoft.Json --version 13.0.3

# 3. プロジェクトを再ビルド
Write-Host "`nプロジェクトをビルドしています..." -ForegroundColor Green
dotnet build