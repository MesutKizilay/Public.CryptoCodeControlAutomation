Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$outputDirectory = Join-Path $PSScriptRoot "output"
$previewDirectory = Join-Path $PSScriptRoot "preview"
$presentationPath = Join-Path $outputDirectory "Kripto_Kod_Kontrol_Otomasyon_Sistemi_Yetkili_Kullanici_Kilavuzu.pptx"

New-Item -ItemType Directory -Force -Path $previewDirectory | Out-Null
Get-ChildItem $previewDirectory -File -ErrorAction SilentlyContinue | Remove-Item -Force

$powerPoint = $null
$presentation = $null

try {
    $powerPoint = New-Object -ComObject PowerPoint.Application
    $powerPoint.DisplayAlerts = 1
    $powerPoint.Visible = -1
    Start-Sleep -Seconds 3

    $presentation = $powerPoint.Presentations.Open($presentationPath, 0, 1, 0)
    Start-Sleep -Seconds 2
    $presentation.Export($previewDirectory, "PNG", 1600, 900)
    Start-Sleep -Seconds 3
}
finally {
    if ($null -ne $presentation) {
        try { $presentation.Close() } catch { }
        [void][System.Runtime.InteropServices.Marshal]::FinalReleaseComObject($presentation)
    }
    if ($null -ne $powerPoint) {
        try { $powerPoint.Quit() } catch { }
        [void][System.Runtime.InteropServices.Marshal]::FinalReleaseComObject($powerPoint)
    }
    [GC]::Collect()
    [GC]::WaitForPendingFinalizers()
}

Get-ChildItem $previewDirectory -File | Sort-Object Name | Select-Object Name, Length
