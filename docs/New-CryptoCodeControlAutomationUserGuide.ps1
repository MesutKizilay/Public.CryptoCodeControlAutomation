param(
    [string]$OutputDirectory = (Join-Path $PSScriptRoot "output")
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Add-Type -AssemblyName System.Drawing

$script:SlideWidth = 13.333
$script:SlideHeight = 7.5
$script:FontName = "Aptos"
$script:Color = @{
    Navy       = 75 + (95 -shl 8) + (115 -shl 16)
    Slate      = 102 + (120 -shl 8) + (138 -shl 16)
    Muted      = 148 + (163 -shl 8) + (179 -shl 16)
    Border     = 217 + (225 -shl 8) + (232 -shl 16)
    Light      = 246 + (248 -shl 8) + (250 -shl 16)
    Green      = 44 + (191 -shl 8) + (111 -shl 16)
    Blue       = 90 + (141 -shl 8) + (238 -shl 16)
    Red        = 240 + (91 -shl 8) + (91 -shl 16)
    Orange     = 255 + (166 -shl 8) + (64 -shl 16)
    Cyan       = 24 + (188 -shl 8) + (212 -shl 16)
    Purple     = 117 + (103 -shl 8) + (240 -shl 16)
    White      = 255 + (255 -shl 8) + (255 -shl 16)
}
$script:MarkerColors = @(
    $script:Color.Blue,
    $script:Color.Green,
    $script:Color.Red,
    $script:Color.Orange,
    $script:Color.Cyan,
    $script:Color.Purple
)

function Inches([double]$value) {
    return [single]($value * 72.0)
}

function Add-TextBox {
    param(
        $Slide,
        [string]$Text,
        [double]$X,
        [double]$Y,
        [double]$Width,
        [double]$Height,
        [double]$FontSize = 10,
        [int]$Color = $script:Color.Navy,
        [bool]$Bold = $false,
        [int]$Alignment = 1,
        [int]$VerticalAnchor = 1
    )

    $shape = $Slide.Shapes.AddTextBox(1, (Inches $X), (Inches $Y), (Inches $Width), (Inches $Height))
    $shape.Line.Visible = 0
    $shape.Fill.Visible = 0
    $shape.TextFrame2.MarginLeft = 0
    $shape.TextFrame2.MarginRight = 0
    $shape.TextFrame2.MarginTop = 0
    $shape.TextFrame2.MarginBottom = 0
    $shape.TextFrame2.WordWrap = -1
    $shape.TextFrame2.AutoSize = 0
    $shape.TextFrame2.VerticalAnchor = $VerticalAnchor
    $shape.TextFrame2.TextRange.Text = $Text
    $shape.TextFrame2.TextRange.Font.Name = $script:FontName
    $shape.TextFrame2.TextRange.Font.Size = $FontSize
    $shape.TextFrame2.TextRange.Font.Bold = if ($Bold) { -1 } else { 0 }
    $shape.TextFrame2.TextRange.Font.Fill.ForeColor.RGB = $Color
    $shape.TextFrame2.TextRange.ParagraphFormat.Alignment = $Alignment
    return $shape
}

function Add-BaseSlide {
    param(
        $Presentation,
        [string]$Title,
        [string]$Subtitle,
        [int]$PageNumber
    )

    $slide = $Presentation.Slides.Add($Presentation.Slides.Count + 1, 12)
    $slide.FollowMasterBackground = 0
    $slide.Background.Fill.Solid()
    $slide.Background.Fill.ForeColor.RGB = $script:Color.White

    $topLine = $slide.Shapes.AddShape(1, 0, 0, (Inches $script:SlideWidth), (Inches 0.08))
    $topLine.Fill.Solid()
    $topLine.Fill.ForeColor.RGB = $script:Color.Green
    $topLine.Line.Visible = 0

    Add-TextBox $slide $Title 0.48 0.22 11.8 0.42 21 $script:Color.Navy $true | Out-Null
    Add-TextBox $slide $Subtitle 0.5 0.72 11.8 0.28 9.5 $script:Color.Slate | Out-Null

    $rule = $slide.Shapes.AddLine((Inches 0.5), (Inches 1.02), (Inches 12.83), (Inches 1.02))
    $rule.Line.ForeColor.RGB = $script:Color.Border
    $rule.Line.Weight = 0.75

    Add-TextBox $slide "Kripto Kod Kontrol Otomasyon Sistemi | Yetkili Kullanıcı Kılavuzu" 0.5 7.18 7.6 0.16 7.2 $script:Color.Muted | Out-Null
    Add-TextBox $slide ($PageNumber.ToString("00")) 12.25 0.34 0.55 0.2 8 $script:Color.Slate $true 3 | Out-Null
    return $slide
}

function Add-NumberCircle {
    param(
        $Slide,
        [int]$Number,
        [double]$X,
        [double]$Y,
        [double]$Size = 0.34,
        [int]$ColorIndex = 0
    )

    $fillColor = $script:MarkerColors[$ColorIndex % $script:MarkerColors.Count]
    $shape = $Slide.Shapes.AddShape(9, (Inches $X), (Inches $Y), (Inches $Size), (Inches $Size))
    $shape.Fill.Solid()
    $shape.Fill.ForeColor.RGB = $fillColor
    $shape.Line.Visible = -1
    $shape.Line.ForeColor.RGB = $script:Color.White
    $shape.Line.Weight = 1.5
    $shape.TextFrame2.MarginLeft = 0
    $shape.TextFrame2.MarginRight = 0
    $shape.TextFrame2.MarginTop = 0
    $shape.TextFrame2.MarginBottom = 0
    $shape.TextFrame2.VerticalAnchor = 3
    $shape.TextFrame2.TextRange.Text = [string]$Number
    $shape.TextFrame2.TextRange.Font.Name = $script:FontName
    $shape.TextFrame2.TextRange.Font.Size = 9
    $shape.TextFrame2.TextRange.Font.Bold = -1
    $shape.TextFrame2.TextRange.Font.Fill.ForeColor.RGB = $script:Color.White
    $shape.TextFrame2.TextRange.ParagraphFormat.Alignment = 2
    return $shape
}

function Add-CalloutStack {
    param(
        $Slide,
        [array]$Items,
        [double]$X,
        [double]$Y,
        [double]$Width,
        [double]$RowHeight
    )

    for ($i = 0; $i -lt $Items.Count; $i++) {
        $itemY = $Y + ($i * $RowHeight)
        Add-NumberCircle $Slide ($i + 1) $X $itemY 0.34 $i | Out-Null
        Add-TextBox $Slide $Items[$i].Title ($X + 0.47) ($itemY - 0.01) ($Width - 0.47) 0.24 10.3 $script:Color.Navy $true | Out-Null
        Add-TextBox $Slide $Items[$i].Text ($X + 0.47) ($itemY + 0.25) ($Width - 0.47) ($RowHeight - 0.27) 8.2 $script:Color.Slate | Out-Null
    }
}

function Add-BottomLegend {
    param(
        $Slide,
        [array]$Items,
        [double]$Y,
        [double]$X = 0.58,
        [double]$Width = 12.15,
        [double]$Height = 1.08
    )

    $columnWidth = $Width / $Items.Count
    for ($i = 0; $i -lt $Items.Count; $i++) {
        $itemX = $X + ($i * $columnWidth)
        Add-NumberCircle $Slide ($i + 1) $itemX $Y 0.34 $i | Out-Null
        Add-TextBox $Slide $Items[$i].Title ($itemX + 0.45) ($Y - 0.02) ($columnWidth - 0.5) 0.24 9.6 $script:Color.Navy $true | Out-Null
        Add-TextBox $Slide $Items[$i].Text ($itemX + 0.45) ($Y + 0.24) ($columnWidth - 0.52) ($Height - 0.22) 7.8 $script:Color.Slate | Out-Null
    }
}

function Add-PictureFit {
    param(
        $Slide,
        [string]$Path,
        [double]$X,
        [double]$Y,
        [double]$Width,
        [double]$Height
    )

    $image = [System.Drawing.Image]::FromFile($Path)
    try {
        $imageRatio = $image.Width / [double]$image.Height
    }
    finally {
        $image.Dispose()
    }

    $boxRatio = $Width / $Height
    if ($imageRatio -gt $boxRatio) {
        $actualWidth = $Width
        $actualHeight = $Width / $imageRatio
        $actualX = $X
        $actualY = $Y + (($Height - $actualHeight) / 2)
    }
    else {
        $actualHeight = $Height
        $actualWidth = $Height * $imageRatio
        $actualX = $X + (($Width - $actualWidth) / 2)
        $actualY = $Y
    }

    $frame = $Slide.Shapes.AddShape(5, (Inches ($actualX - 0.04)), (Inches ($actualY - 0.04)), (Inches ($actualWidth + 0.08)), (Inches ($actualHeight + 0.08)))
    $frame.Fill.Solid()
    $frame.Fill.ForeColor.RGB = $script:Color.White
    $frame.Line.Visible = -1
    $frame.Line.ForeColor.RGB = $script:Color.Border
    $frame.Line.Weight = 0.8

    $picture = $Slide.Shapes.AddPicture($Path, 0, -1, (Inches $actualX), (Inches $actualY), (Inches $actualWidth), (Inches $actualHeight))
    return [pscustomobject]@{ Shape = $picture; X = $actualX; Y = $actualY; Width = $actualWidth; Height = $actualHeight }
}

function Crop-Image {
    param(
        [string]$Source,
        [string]$Destination,
        [int]$X,
        [int]$Y,
        [int]$Width,
        [int]$Height
    )

    $sourceBitmap = [System.Drawing.Bitmap]::FromFile($Source)
    try {
        if ($X -lt 0 -or $Y -lt 0 -or ($X + $Width) -gt $sourceBitmap.Width -or ($Y + $Height) -gt $sourceBitmap.Height) {
            throw "Crop rectangle is outside image bounds: $Source"
        }

        $destinationBitmap = New-Object System.Drawing.Bitmap $Width, $Height
        try {
            $graphics = [System.Drawing.Graphics]::FromImage($destinationBitmap)
            try {
                $graphics.Clear([System.Drawing.Color]::White)
                $graphics.DrawImage(
                    $sourceBitmap,
                    (New-Object System.Drawing.Rectangle 0, 0, $Width, $Height),
                    (New-Object System.Drawing.Rectangle $X, $Y, $Width, $Height),
                    [System.Drawing.GraphicsUnit]::Pixel)
            }
            finally {
                $graphics.Dispose()
            }
            $destinationBitmap.Save($Destination, [System.Drawing.Imaging.ImageFormat]::Png)
        }
        finally {
            $destinationBitmap.Dispose()
        }
    }
    finally {
        $sourceBitmap.Dispose()
    }
}

function Get-ScreenshotBySuffix {
    param([string]$Suffix)

    $matches = @(Get-ChildItem (Join-Path $env:USERPROFILE "Pictures\Screenshots") -File -Filter "*$Suffix.png" | Sort-Object LastWriteTime -Descending)
    if ($matches.Count -eq 0) {
        throw "Screenshot ending with '$Suffix.png' was not found."
    }
    return $matches[0].FullName
}

function Add-CoverSlide {
    param($Presentation, [string]$TraceLogo, [string]$TeknosinLogo)

    $slide = $Presentation.Slides.Add($Presentation.Slides.Count + 1, 12)
    $slide.FollowMasterBackground = 0
    $slide.Background.Fill.Solid()
    $slide.Background.Fill.ForeColor.RGB = $script:Color.White

    $line = $slide.Shapes.AddShape(1, 0, 0, (Inches $script:SlideWidth), (Inches 0.1))
    $line.Fill.Solid()
    $line.Fill.ForeColor.RGB = $script:Color.Green
    $line.Line.Visible = 0

    $slide.Shapes.AddPicture($TraceLogo, 0, -1, (Inches 0.7), (Inches 0.55), (Inches 3.3), (Inches 0.71)) | Out-Null

    $accent = $slide.Shapes.AddShape(5, (Inches 0.72), (Inches 2.0), (Inches 0.18), (Inches 2.0))
    $accent.Fill.Solid()
    $accent.Fill.ForeColor.RGB = $script:Color.Green
    $accent.Line.Visible = 0

    Add-TextBox $slide "Kripto Kod Kontrol`nOtomasyon Sistemi" 1.2 2.02 8.9 1.25 31 $script:Color.Navy $true | Out-Null
    Add-TextBox $slide "Yetkili Kullanıcı Kılavuzu" 1.23 3.42 7.4 0.5 17 $script:Color.Slate | Out-Null
    Add-TextBox $slide "Siparişten üretime, kod doğrulamadan raporlamaya kadar temel kullanıcı işlemleri." 1.23 4.05 8.6 0.5 11 $script:Color.Muted | Out-Null

    $gridX = 10.3
    $gridY = 2.0
    for ($row = 0; $row -lt 5; $row++) {
        for ($col = 0; $col -lt 5; $col++) {
            if (($row + $col) % 3 -eq 0 -or ($row -eq 2 -and $col -ge 1)) {
                $square = $slide.Shapes.AddShape(5, (Inches ($gridX + ($col * 0.42))), (Inches ($gridY + ($row * 0.42))), (Inches 0.3), (Inches 0.3))
                $square.Fill.Solid()
                $square.Fill.ForeColor.RGB = if (($row + $col) % 2 -eq 0) { $script:Color.Blue } else { $script:Color.Green }
                $square.Line.Visible = 0
            }
        }
    }

    $slide.Shapes.AddPicture($TeknosinLogo, 0, -1, (Inches 10.15), (Inches 6.72), (Inches 2.45), (Inches 0.53)) | Out-Null
    return $slide
}

$docsDirectory = $PSScriptRoot
$assetsDirectory = Join-Path $docsDirectory "guide-assets"
$rawDirectory = Join-Path $assetsDirectory "raw"
$croppedDirectory = Join-Path $assetsDirectory "cropped"
$previewDirectory = Join-Path $docsDirectory "preview"

foreach ($directory in @($OutputDirectory, $assetsDirectory, $rawDirectory, $croppedDirectory, $previewDirectory)) {
    New-Item -ItemType Directory -Force -Path $directory | Out-Null
}

Get-ChildItem $previewDirectory -File -ErrorAction SilentlyContinue | Remove-Item -Force

$sourceSuffixes = [ordered]@{
    DashboardTop    = "190231"
    DashboardCharts = "103417"
    SalesList       = "190242"
    SalesAdd        = "112354"
    Approvals       = "193354"
    Recover         = "155023"
    Scraps          = "155820"
    Lookup          = "160211"
    Datamatrix      = "160439"
    Reports         = "160706"
    Adjustments     = "161249"
    Logs            = "161505"
    Users           = "184005"
    UserAdd         = "184013"
}

$raw = @{}
foreach ($entry in $sourceSuffixes.GetEnumerator()) {
    $source = Get-ScreenshotBySuffix $entry.Value
    $destination = Join-Path $rawDirectory ($entry.Key + ".png")
    [System.IO.File]::Copy($source, $destination, $true)
    $raw[$entry.Key] = $destination
}

$cropDefinitions = @{
    DashboardTop    = @(350, 210, 1490, 710)
    DashboardCharts = @(400, 140, 1360, 850)
    SalesList       = @(320, 190, 1570, 470)
    SalesAdd        = @(1418, 108, 502, 1032)
    Approvals       = @(320, 190, 1570, 620)
    Recover         = @(350, 200, 1540, 530)
    Scraps          = @(350, 200, 1540, 820)
    Lookup          = @(390, 150, 1390, 850)
    Datamatrix      = @(350, 200, 1540, 600)
    Reports         = @(350, 200, 1540, 900)
    Adjustments     = @(440, 140, 1280, 860)
    Logs            = @(320, 190, 1570, 600)
    Users           = @(320, 190, 1570, 430)
    UserAdd         = @(1468, 108, 452, 1032)
}

$image = @{}
foreach ($entry in $cropDefinitions.GetEnumerator()) {
    $destination = Join-Path $croppedDirectory ($entry.Key + ".png")
    $rect = $entry.Value
    Crop-Image $raw[$entry.Key] $destination $rect[0] $rect[1] $rect[2] $rect[3]
    $image[$entry.Key] = $destination
}

$repoRoot = Split-Path $docsDirectory -Parent
$traceLogo = Join-Path $repoRoot "Public.CryptoCodeControlAutomation.Presentation\wwwroot\img\tekno-trace-logo.png"
$teknosinLogo = Join-Path $repoRoot "Public.CryptoCodeControlAutomation.Presentation\wwwroot\img\teknosin.png"

$pptxPath = Join-Path $OutputDirectory "Kripto_Kod_Kontrol_Otomasyon_Sistemi_Yetkili_Kullanici_Kilavuzu.pptx"
$pdfPath = Join-Path $OutputDirectory "Kripto_Kod_Kontrol_Otomasyon_Sistemi_Yetkili_Kullanici_Kilavuzu.pdf"

$powerPoint = $null
$presentation = $null
$phase = "PowerPoint başlatılıyor"

try {
    $powerPoint = New-Object -ComObject PowerPoint.Application
    $powerPoint.DisplayAlerts = 1
    $powerPoint.Visible = -1
    Start-Sleep -Seconds 3

    $phase = "Boş sunum oluşturuluyor"
    $presentation = $powerPoint.Presentations.Add()
    $presentation.PageSetup.SlideWidth = Inches $script:SlideWidth
    $presentation.PageSetup.SlideHeight = Inches $script:SlideHeight

    $phase = "Kapak oluşturuluyor"
    Add-CoverSlide $presentation $traceLogo $teknosinLogo | Out-Null

    $phase = "Dashboard özet slaytı oluşturuluyor"
    $slide = Add-BaseSlide $presentation "1. Genel durumu izleyin" "Kod havuzunun özetini ve sipariş bazlı güncel durumu tek ekranda takip edin." 1
    Add-PictureFit $slide $image.DashboardTop 0.48 1.22 8.55 5.55 | Out-Null
    Add-NumberCircle $slide 1 4.4 2.05 0.36 0 | Out-Null
    Add-NumberCircle $slide 2 4.4 3.05 0.36 1 | Out-Null
    Add-NumberCircle $slide 3 4.4 4.72 0.36 2 | Out-Null
    Add-CalloutStack $slide @(
        [pscustomobject]@{ Title = "Kod durumu özeti"; Text = "Toplam Kod, Üretilen, Fire ve Reject Kurtarma sayılarını birlikte gösterir." },
        [pscustomobject]@{ Title = "Available / Allocated / Void"; Text = "Available kullanılabilir, Allocated planlı siparişe ayrılmış, Void geçersiz veya kullanılamayacak koddur." },
        [pscustomobject]@{ Title = "Satış siparişleri"; Text = "Bir sipariş satırı seçildiğinde aşağıdaki grafikler ilgili siparişe göre güncellenir." }
    ) 9.25 1.55 3.55 1.55

    $phase = "Dashboard grafik slaytı oluşturuluyor"
    $slide = Add-BaseSlide $presentation "2. Kod dağılımını ve üretim eğilimini inceleyin" "Grafikleri tüm kodlar veya seçilen satış siparişi kapsamında değerlendirin." 2
    Add-PictureFit $slide $image.DashboardCharts 0.5 1.18 8.35 5.75 | Out-Null
    Add-NumberCircle $slide 1 4.15 2.25 0.36 0 | Out-Null
    Add-NumberCircle $slide 2 7.15 2.05 0.36 1 | Out-Null
    Add-NumberCircle $slide 3 4.2 5.1 0.36 2 | Out-Null
    Add-CalloutStack $slide @(
        [pscustomobject]@{ Title = "Kod Durum Dağılımı"; Text = "Kodların durumlara göre adet ve yüzde dağılımını gösterir." },
        [pscustomobject]@{ Title = "Durumların anlamı"; Text = "ProducedOk üretilmiş, Reject reject durumunda, Scrap fireye ayrılmış kodları ifade eder." },
        [pscustomobject]@{ Title = "Üretilen Kodlar"; Text = "Üretilen kod miktarını günlük, haftalık, aylık veya yıllık dönemlerde karşılaştırın." }
    ) 9.15 1.55 3.7 1.55

    $phase = "Satış siparişleri slaytı oluşturuluyor"
    $slide = Add-BaseSlide $presentation "3. Satış siparişlerini yönetin" "Siparişleri görüntüleyin, durumlarını takip edin ve gerekli işlemleri başlatın." 3
    Add-PictureFit $slide $image.SalesList 0.58 1.3 12.15 3.75 | Out-Null
    Add-NumberCircle $slide 1 11.65 1.72 0.36 0 | Out-Null
    Add-NumberCircle $slide 2 8.45 3.04 0.36 1 | Out-Null
    Add-NumberCircle $slide 3 11.5 3.05 0.36 2 | Out-Null
    Add-BottomLegend $slide @(
        [pscustomobject]@{ Title = "Yeni Sipariş"; Text = "Yeni satış siparişi ve kripto kod yükleme işlemini başlatır." },
        [pscustomobject]@{ Title = "Sipariş durumu"; Text = "PASİF hazırlık, AKTİF üretim, TAMAMLANDI biten süreçtir. Aynı anda yalnızca bir sipariş aktif olabilir." },
        [pscustomobject]@{ Title = "İşlemler"; Text = "Sipariş bilgilerini düzenleyin veya siparişi iptal edin." }
    ) 5.45 0.65 12.0 1.05

    $phase = "Yeni satış siparişi slaytı oluşturuluyor"
    $slide = Add-BaseSlide $presentation "4. Yeni satış siparişi oluşturun" "Ürün, miktar, raf ömrü ve kripto kod dosyasını aynı panelde tanımlayın." 4
    Add-PictureFit $slide $image.SalesAdd 9.65 1.2 3.0 5.85 | Out-Null
    Add-NumberCircle $slide 1 10.58 1.8 0.34 0 | Out-Null
    Add-NumberCircle $slide 2 10.58 2.7 0.34 1 | Out-Null
    Add-NumberCircle $slide 3 10.58 3.55 0.34 2 | Out-Null
    Add-NumberCircle $slide 4 10.58 4.35 0.34 3 | Out-Null
    Add-NumberCircle $slide 5 10.58 5.6 0.34 4 | Out-Null
    Add-CalloutStack $slide @(
        [pscustomobject]@{ Title = "Sipariş numaraları"; Text = "Sales Order No sistem tarafından önerilir; Sales Item No varsayılan olarak 1 başlar." },
        [pscustomobject]@{ Title = "Ürün bilgileri"; Text = "Material No ve GTIN alanlarına üretilecek ürünü tanımlayan değerleri girin." },
        [pscustomobject]@{ Title = "Miktarlar"; Text = "Planned Unit Quantity birim miktarı, Case Quantity siparişin toplam koli adedidir." },
        [pscustomobject]@{ Title = "Raf ömrü"; Text = "Shelf Life değerini Gün, Hafta, Ay veya Yıl birimiyle tanımlayın. SKT tutulmayacaksa 0 girilebilir." },
        [pscustomobject]@{ Title = "CSV dosyası"; Text = "Her satıra bir kod yazın. 01-21-93 veya 01-21-91-92 yapısındaki kodlar ve %5 fire payı bulunmalıdır." }
    ) 0.72 1.3 8.45 1.08

    $phase = "Sipariş onayları slaytı oluşturuluyor"
    $slide = Add-BaseSlide $presentation "5. Üretim ve sevkiyat onaylarını izleyin" "Siparişin onay sürecini üretimden sevkiyata kadar adım adım takip edin." 5
    Add-PictureFit $slide $image.Approvals 0.58 1.2 12.15 4.75 | Out-Null
    Add-NumberCircle $slide 1 11.55 2.75 0.36 0 | Out-Null
    Add-NumberCircle $slide 2 11.55 3.55 0.36 1 | Out-Null
    Add-NumberCircle $slide 3 8.85 4.35 0.36 2 | Out-Null
    Add-BottomLegend $slide @(
        [pscustomobject]@{ Title = "Onay Bekliyor"; Text = "Üretim işlemleri tamamlandığında üretim sorumlusu Üretim Onayı verir." },
        [pscustomobject]@{ Title = "Üretim Onayı"; Text = "Onaylayan kullanıcı ve tarih görünür; sipariş Sevkiyat Onayı aşamasına geçer." },
        [pscustomobject]@{ Title = "Sevkiyat Onayı"; Text = "Sevkiyat sorumlusu onay verdiğinde onay süreci tamamlanır." }
    ) 6.05 0.68 11.95 0.95

    $phase = "Reject kurtarma slaytı oluşturuluyor"
    $slide = Add-BaseSlide $presentation "6. Sağlam ürünleri üretime kazandırın" "Reject ünitesindeki okunabilir ve sağlam ürünleri ilgili planlı siparişe geri kazandırın." 6
    Add-PictureFit $slide $image.Recover 0.58 1.22 12.15 4.3 | Out-Null
    Add-NumberCircle $slide 1 3.0 2.25 0.36 0 | Out-Null
    Add-NumberCircle $slide 2 7.4 3.1 0.36 1 | Out-Null
    Add-NumberCircle $slide 3 10.65 4.2 0.36 2 | Out-Null
    Add-NumberCircle $slide 4 11.45 4.95 0.36 3 | Out-Null
    Add-BottomLegend $slide @(
        [pscustomobject]@{ Title = "Planlı sipariş"; Text = "Planlı sipariş numarasını girerek ilgili kodları işleme hazırlayın." },
        [pscustomobject]@{ Title = "Kodları okutun"; Text = "Sağlam ürünlerin kodlarını okutun; kayıtlar tarih bilgisiyle listeye eklenir." },
        [pscustomobject]@{ Title = "Listeyi kontrol edin"; Text = "Yanlış okutulan kodları Sil düğmesiyle listeden çıkarın." },
        [pscustomobject]@{ Title = "Üretime Kazandır"; Text = "Kontrol edilen kodları yeniden üretilmiş duruma alın." }
    ) 5.75 0.62 12.05 1.0

    $phase = "Fire işlemleri slaytı oluşturuluyor"
    $slide = Add-BaseSlide $presentation "7. Hasarlı ürünleri fireye ayırın" "Fiziksel olarak kullanılamayacak ürünlerin okunabilir kodlarını fire durumuna alın." 7
    Add-PictureFit $slide $image.Scraps 0.5 1.22 8.35 5.55 | Out-Null
    Add-NumberCircle $slide 1 6.15 2.1 0.36 0 | Out-Null
    Add-NumberCircle $slide 2 5.1 3.45 0.36 1 | Out-Null
    Add-NumberCircle $slide 3 4.4 4.7 0.36 2 | Out-Null
    Add-NumberCircle $slide 4 7.65 5.75 0.36 3 | Out-Null
    Add-CalloutStack $slide @(
        [pscustomobject]@{ Title = "Planlı sipariş"; Text = "Fire işleminin uygulanacağı planlı siparişi seçin." },
        [pscustomobject]@{ Title = "Kod ekleme"; Text = "Ürün kodunu okutun ve listeye ekleyin." },
        [pscustomobject]@{ Title = "Kontrol"; Text = "Okutulan kodları ve işlem tarihlerini gözden geçirin; hatalı kaydı silebilirsiniz." },
        [pscustomobject]@{ Title = "Fireye Ayır"; Text = "Kontrol edilen kodları fire durumuna geçirin." }
    ) 9.15 1.42 3.65 1.28

    $phase = "Kod sorgulama slaytı oluşturuluyor"
    $slide = Add-BaseSlide $presentation "8. Kripto kodun geçmişini sorgulayın" "Bir kodun durumunu, siparişini ve üretim bilgilerini tek ekranda görüntüleyin." 8
    Add-PictureFit $slide $image.Lookup 0.5 1.2 8.35 5.72 | Out-Null
    Add-NumberCircle $slide 1 4.4 1.72 0.36 0 | Out-Null
    Add-NumberCircle $slide 2 3.0 2.55 0.36 1 | Out-Null
    Add-NumberCircle $slide 3 3.75 4.05 0.36 2 | Out-Null
    Add-NumberCircle $slide 4 3.75 5.55 0.36 3 | Out-Null
    Add-CalloutStack $slide @(
        [pscustomobject]@{ Title = "Kod sorgulama"; Text = "Kripto kodu okutun veya girin ve Sorgula düğmesini kullanın." },
        [pscustomobject]@{ Title = "Durum ve paketleme"; Text = "Kodun mevcut durumunu ve paketleme bilgisini görüntüleyin." },
        [pscustomobject]@{ Title = "Sipariş bilgileri"; Text = "Satış siparişi, kalem, mamul ve GTIN bilgilerini doğrulayın." },
        [pscustomobject]@{ Title = "Üretim bilgileri"; Text = "Planlı sipariş, hat, tasnif ve üretim tarihlerini inceleyin." }
    ) 9.15 1.4 3.65 1.28

    $phase = "DataMatrix PDF slaytı oluşturuluyor"
    $slide = Add-BaseSlide $presentation "9. Kodlardan DataMatrix PDF oluşturun" "CSV dosyasındaki kripto kodları yazdırılabilir DataMatrix görsellerine dönüştürün." 9
    Add-PictureFit $slide $image.Datamatrix 0.58 1.24 12.15 4.65 | Out-Null
    Add-NumberCircle $slide 1 6.3 3.05 0.36 0 | Out-Null
    Add-NumberCircle $slide 2 11.65 5.2 0.36 1 | Out-Null
    Add-BottomLegend $slide @(
        [pscustomobject]@{ Title = "CSV dosyasını yükleyin"; Text = "Her satırda tek bir kripto kod bulunacak şekilde hazırlanan CSV dosyasını seçin." },
        [pscustomobject]@{ Title = "PDF Oluştur"; Text = "Geçerli kodlardan DataMatrix PDF oluşturulur; hatalı satırlar kullanıcıya bildirilir." }
    ) 6.05 1.15 11.0 0.85

    $phase = "Kripto kod havuzu slaytı oluşturuluyor"
    $slide = Add-BaseSlide $presentation "10. Kripto kod havuzunu raporlayın" "Kodları sipariş, durum ve üretim tarihi ölçütleriyle filtreleyip dışa aktarın." 10
    Add-PictureFit $slide $image.Reports 0.5 1.2 8.35 5.75 | Out-Null
    Add-NumberCircle $slide 1 2.15 1.9 0.36 0 | Out-Null
    Add-NumberCircle $slide 2 4.85 1.9 0.36 1 | Out-Null
    Add-NumberCircle $slide 3 7.25 1.9 0.36 2 | Out-Null
    Add-NumberCircle $slide 4 7.25 3.15 0.36 3 | Out-Null
    Add-NumberCircle $slide 5 4.55 5.05 0.36 4 | Out-Null
    Add-CalloutStack $slide @(
        [pscustomobject]@{ Title = "Kod"; Text = "Tam kripto kod değerini girerek tekil kayıt arayın." },
        [pscustomobject]@{ Title = "Sipariş filtreleri"; Text = "Satış siparişi veya buna bağlı planlı sipariş üzerinden kapsamı daraltın." },
        [pscustomobject]@{ Title = "Durum ve tarih"; Text = "Kod durumunu ve üretim tarihi aralığını birlikte kullanabilirsiniz." },
        [pscustomobject]@{ Title = "Filtrele / Dışa Aktar"; Text = "Sonuçları yenileyin veya en az bir filtre kullanarak CSV indirin." },
        [pscustomobject]@{ Title = "Sadece kodları indir"; Text = "Dışa aktarımda diğer sütunlar yerine yalnızca kod değerlerini alın." }
    ) 9.05 1.25 3.75 1.08

    $phase = "Üretim düzeltme slaytı oluşturuluyor"
    $slide = Add-BaseSlide $presentation "11. Üretim kayıtlarını kontrollü düzeltin" "Bu ekran canlı üretim verilerini etkiler; yalnızca yetkili kullanıcılar ve zorunlu açıklamayla işlem yapmalıdır." 11
    Add-PictureFit $slide $image.Adjustments 0.45 1.18 8.05 5.82 | Out-Null
    Add-NumberCircle $slide 1 4.1 1.72 0.36 0 | Out-Null
    Add-NumberCircle $slide 2 4.1 2.5 0.36 1 | Out-Null
    Add-NumberCircle $slide 3 4.1 3.5 0.36 2 | Out-Null
    Add-NumberCircle $slide 4 2.5 5.25 0.36 3 | Out-Null
    Add-NumberCircle $slide 5 6.1 5.25 0.36 4 | Out-Null
    Add-CalloutStack $slide @(
        [pscustomobject]@{ Title = "Sipariş ve özet"; Text = "Satış ve planlı siparişi seçerek güncel üretim özetini getirin." },
        [pscustomobject]@{ Title = "Kod durumları"; Text = "Available, Allocated, ProducedOk, Reject, Scrap ve Void adetlerini kontrol edin." },
        [pscustomobject]@{ Title = "Günlük üretim"; Text = "Üretilen kod adetlerini gün bazında inceleyin; satır seçimi tarih alanlarını doldurur." },
        [pscustomobject]@{ Title = "Üretim Adedi"; Text = "Allocated ve ProducedOk durumları arasında belirli adet için kontrollü düzeltme yapın." },
        [pscustomobject]@{ Title = "Üretim Tarihi"; Text = "Belirli adette üretilmiş kodu mevcut tarihten yeni üretim tarihine taşıyın." }
    ) 8.8 1.22 4.0 1.1

    $phase = "Müdahale geçmişi slaytı oluşturuluyor"
    $slide = Add-BaseSlide $presentation "12. Müdahale geçmişini doğrulayın" "Her müdahalenin kim tarafından, ne zaman, hangi kapsamda ve kaç kod için uygulandığını inceleyin." 12
    Add-PictureFit $slide $image.Logs 0.58 1.2 12.15 4.7 | Out-Null
    Add-NumberCircle $slide 1 11.45 1.82 0.36 0 | Out-Null
    Add-NumberCircle $slide 2 2.35 3.1 0.36 1 | Out-Null
    Add-NumberCircle $slide 3 4.45 3.1 0.36 2 | Out-Null
    Add-NumberCircle $slide 4 8.15 3.1 0.36 3 | Out-Null
    Add-NumberCircle $slide 5 10.85 3.1 0.36 4 | Out-Null
    Add-BottomLegend $slide @(
        [pscustomobject]@{ Title = "Ara"; Text = "Sipariş, kullanıcı, işlem tipi veya açıklama ile kayıt bulun." },
        [pscustomobject]@{ Title = "Tarih ve kullanıcı"; Text = "Müdahalenin ne zaman ve kim tarafından yapıldığını görün." },
        [pscustomobject]@{ Title = "İşlem tipi"; Text = "Kayda alınan müdahalenin türünü inceleyin." },
        [pscustomobject]@{ Title = "Değişim bilgisi"; Text = "Eski-yeni kod durumunu veya üretim tarihini karşılaştırın." },
        [pscustomobject]@{ Title = "Adet ve açıklama"; Text = "Etkilenen kod sayısını ve işlem gerekçesini doğrulayın." }
    ) 6.02 0.55 12.25 0.98

    $phase = "Kullanıcı listesi slaytı oluşturuluyor"
    $slide = Add-BaseSlide $presentation "13. Kullanıcıları ve yetkileri yönetin" "Kullanıcı hesaplarını, rollerini ve erişim durumlarını tek listeden yönetin." 13
    Add-PictureFit $slide $image.Users 0.58 1.25 12.15 3.45 | Out-Null
    Add-NumberCircle $slide 1 5.0 2.85 0.36 0 | Out-Null
    Add-NumberCircle $slide 2 11.55 1.65 0.36 1 | Out-Null
    Add-NumberCircle $slide 3 11.45 3.0 0.36 2 | Out-Null
    Add-NumberCircle $slide 4 7.0 3.0 0.36 3 | Out-Null
    Add-BottomLegend $slide @(
        [pscustomobject]@{ Title = "Kullanıcı listesi"; Text = "Kullanıcı adı, tam ad, rol ve hesap durumunu görüntüleyin." },
        [pscustomobject]@{ Title = "Kullanıcı Ekle"; Text = "Yeni bir kullanıcı hesabı oluşturun." },
        [pscustomobject]@{ Title = "İşlemler"; Text = "Kullanıcı bilgilerini düzenleyin veya hesabı pasif duruma getirin." },
        [pscustomobject]@{ Title = "Rol erişimi"; Text = "Operator saha ekranlarını; Supervisor bunlara ek olarak sipariş, rapor, düzeltme ve kullanıcı yönetimini kullanır." }
    ) 5.05 0.6 12.1 1.45

    $phase = "Yeni kullanıcı slaytı oluşturuluyor"
    $slide = Add-BaseSlide $presentation "14. Yeni kullanıcı oluşturun" "Hesap bilgilerini girin ve kullanıcının görevine uygun rolü belirleyin." 14
    Add-PictureFit $slide $image.UserAdd 9.85 1.18 2.8 5.85 | Out-Null
    Add-NumberCircle $slide 1 10.75 1.9 0.34 0 | Out-Null
    Add-NumberCircle $slide 2 10.75 3.05 0.34 1 | Out-Null
    Add-NumberCircle $slide 3 10.75 4.05 0.34 2 | Out-Null
    Add-NumberCircle $slide 4 10.75 5.35 0.34 3 | Out-Null
    Add-CalloutStack $slide @(
        [pscustomobject]@{ Title = "Hesap bilgileri"; Text = "Sisteme girişte kullanılacak kullanıcı adını ve kullanıcının tam adını girin." },
        [pscustomobject]@{ Title = "Şifre"; Text = "Kullanıcı için güvenli bir başlangıç şifresi belirleyin." },
        [pscustomobject]@{ Title = "Rol"; Text = "Kullanıcının görevine göre Operator veya Supervisor rolünü seçin." },
        [pscustomobject]@{ Title = "Aktif ve Kaydet"; Text = "Aktif durumdaki kullanıcı sisteme giriş yapabilir. Bilgileri kontrol ederek hesabı kaydedin." }
    ) 0.85 1.45 8.45 1.3

    if (Test-Path $pptxPath) { Remove-Item $pptxPath -Force }
    if (Test-Path $pdfPath) { Remove-Item $pdfPath -Force }

    $phase = "PowerPoint dosyası kaydediliyor"
    $presentation.SaveAs($pptxPath, 24)
    Start-Sleep -Seconds 2

    $phase = "PDF dosyası oluşturuluyor"
    $presentation.SaveAs($pdfPath, 32)
    Start-Sleep -Seconds 2
}
catch {
    Write-Error ("Belge üretimi başarısız oldu. Aşama: {0}. Hata: {1}" -f $phase, $_.Exception.Message)
    throw
}
finally {
    if ($null -ne $presentation) {
        try {
            Start-Sleep -Seconds 2
            $presentation.Close()
        }
        catch {
            Write-Warning ("Sunum kapatılırken Office çağrısı reddedildi: {0}" -f $_.Exception.Message)
        }
        finally {
            [void][System.Runtime.InteropServices.Marshal]::FinalReleaseComObject($presentation)
        }
    }
    if ($null -ne $powerPoint) {
        try {
            $powerPoint.Quit()
        }
        catch {
            Write-Warning ("PowerPoint kapatılırken Office çağrısı reddedildi: {0}" -f $_.Exception.Message)
        }
        finally {
            [void][System.Runtime.InteropServices.Marshal]::FinalReleaseComObject($powerPoint)
        }
    }
    [GC]::Collect()
    [GC]::WaitForPendingFinalizers()
}

Get-Item $pptxPath, $pdfPath | Select-Object FullName, Length, LastWriteTime
