#Requires -Version 5
<#
  BURAK Oyunu - Eksiklik Tarayici (deterministik, salt-okunur)
  Mekanik taramayi modelden CIKARIR: dosyalari gezer, sikistirilmis bir
  "olgu sayfasi" basar. Hicbir dosyaya yazmaz. Cikti birkac KB; dosya dokumu DEGIL.
  Kullanim: powershell -NoProfile -File scan.ps1 [-ProjectRoot <yol>]
  NOT: Bu dosya bilerek SALT-ASCII yazildi (PowerShell 5.1 BOM'suz UTF-8'i
       sistem kod sayfasiyla okur; ozel karakterler ayristirmayi bozar).
#>
[CmdletBinding()]
param(
    [string]$ProjectRoot
)

$ErrorActionPreference = 'Stop'

# Proje kokunu coz
if (-not $ProjectRoot) {
    if ($PSScriptRoot) { $ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path }
    else { $ProjectRoot = (Get-Location).Path }
}
$scriptsDir = Join-Path $ProjectRoot 'Assets\Scripts'
$editorDir  = Join-Path $ProjectRoot 'Assets\Editor'
$testsDir   = Join-Path $ProjectRoot 'Assets\Tests'

if (-not (Test-Path $scriptsDir)) {
    Write-Output ("HATA: Assets\Scripts bulunamadi (" + $scriptsDir + "). -ProjectRoot dogru mu?")
    exit 1
}

# Dosyalari bir kez oku
$runtime = @(Get-ChildItem -Path $scriptsDir -Recurse -Filter *.cs -ErrorAction SilentlyContinue)
$editor  = @(Get-ChildItem -Path $editorDir  -Recurse -Filter *.cs -ErrorAction SilentlyContinue)
$tests   = @(Get-ChildItem -Path $testsDir   -Recurse -Filter *.cs -ErrorAction SilentlyContinue)

$doc = @{}
foreach ($f in ($runtime + $editor + $tests)) { $doc[$f.FullName] = (Get-Content $f.FullName -Raw) }

function Rel([string]$full) { return $full.Replace($ProjectRoot + '\', '').Replace('\', '/') }
function LineCount([string]$t) { if (-not $t) { return 0 }; return ($t -split "`n").Count }

$allRuntime = ($runtime | ForEach-Object { $doc[$_.FullName] }) -join "`n"

function FindHits([string]$pattern, [object[]]$files) {
    $out = @()
    foreach ($f in $files) {
        $lines = $doc[$f.FullName] -split "`n"
        for ($i = 0; $i -lt $lines.Count; $i++) {
            if ($lines[$i] -match $pattern) { $out += ((Rel $f.FullName) + ':' + ($i + 1)) }
        }
    }
    return $out
}

$nl = [Environment]::NewLine
$o = New-Object System.Collections.Generic.List[string]
function Emit($s) { $o.Add([string]$s) }

Emit "=== BURAK EKSIKLIK OLGU SAYFASI ==="
Emit ("Kok: " + $ProjectRoot)
Emit ("Tarih: " + (Get-Date -Format 'yyyy-MM-dd HH:mm'))
Emit ""

# 1) ENVANTER
Emit "## 1. ENVANTER"
$mono = @(); $so = @()
foreach ($f in $runtime) {
    $t = $doc[$f.FullName]
    foreach ($m in [regex]::Matches($t, 'class\s+(\w+)\s*:\s*MonoBehaviour')) { $mono += $m.Groups[1].Value }
    foreach ($m in [regex]::Matches($t, 'class\s+(\w+)\s*:\s*ScriptableObject')) { $so += $m.Groups[1].Value }
}
$totalLines = 0
foreach ($f in $runtime) { $totalLines += LineCount $doc[$f.FullName] }
Emit ("Runtime .cs: " + $runtime.Count + " dosya, ~" + $totalLines + " satir | Editor: " + $editor.Count + " | Test: " + $tests.Count)
Emit ("MonoBehaviour: " + (($mono | Sort-Object -Unique) -join ', '))
Emit ("ScriptableObject: " + (($so | Sort-Object -Unique) -join ', '))
Emit ""

# 2) SerializeField capraz-kontrolu
Emit "## 2. SerializeField CAPRAZ-KONTROL (SceneSetup.SetField -> [SerializeField])"
$serFields = New-Object System.Collections.Generic.HashSet[string]
foreach ($f in $runtime) {
    foreach ($m in [regex]::Matches($doc[$f.FullName], '\[SerializeField\][^;]*?\b(\w+)\s*(=|;)')) {
        [void]$serFields.Add($m.Groups[1].Value)
    }
}
$setFieldNames = @()
$sceneSetup = $editor | Where-Object { $_.Name -eq 'SceneSetup.cs' } | Select-Object -First 1
if ($sceneSetup) {
    foreach ($m in [regex]::Matches($doc[$sceneSetup.FullName], 'SetField\([^,]+,\s*"([^"]+)"')) {
        $setFieldNames += $m.Groups[1].Value
    }
    $uniqSet = @($setFieldNames | Sort-Object -Unique)
    $missing = @($uniqSet | Where-Object { -not $serFields.Contains($_) })
    Emit ("SetField cagrisi: " + $uniqSet.Count + " benzersiz alan | Hicbir [SerializeField] ile eslesmeyen: " + $missing.Count)
    if ($missing.Count -gt 0) { Emit ("  ESLESMEYEN (KRITIK): " + ($missing -join ', ')) }
    else { Emit "  Tum SetField alanlari bir [SerializeField] ile eslesiyor (OK)" }
} else {
    Emit "SceneSetup.cs bulunamadi - capraz-kontrol atlandi"
}
Emit ""

# 3) Event kablolamasi
Emit "## 3. EVENT KABLOLAMASI (bildirim vs +=/-= vs Invoke)"
$declared = @()
foreach ($f in $runtime) {
    foreach ($m in [regex]::Matches($doc[$f.FullName], 'event\s+Action(?:<[^>]*>)?\s+(\w+)\s*;')) {
        $declared += $m.Groups[1].Value
    }
}
$declared = $declared | Sort-Object -Unique
foreach ($ev in $declared) {
    $subs    = ([regex]::Matches($allRuntime, [regex]::Escape($ev) + '\s*(\+=|-=)')).Count
    $invokes = ([regex]::Matches($allRuntime, [regex]::Escape($ev) + '\s*\?\.\s*Invoke')).Count
    $flag = ''
    if ($subs -eq 0)    { $flag = '  <-- ABONESI YOK (olu/eksik kanca)' }
    if ($invokes -eq 0) { $flag = $flag + '  <-- HIC TETIKLENMIYOR' }
    Emit ($ev + ": abone=" + $subs + ", invoke=" + $invokes + $flag)
}
Emit ""

# 4) Ozellik varlik bayraklari
Emit "## 4. OZELLIK VARLIK BAYRAKLARI (VAR/YOK)"
function Feature([string]$label, [string]$pattern, [string]$note, [object[]]$files) {
    if (-not $files) { $files = $runtime }
    $hits = FindHits $pattern $files
    if ($hits.Count -gt 0) {
        $where = ($hits | Select-Object -First 2) -join ','
        Emit ("VAR  " + $label + " (" + $where + ")")
    } else {
        Emit ("YOK  " + $label + " - " + $note)
    }
}
$snakeFile = $runtime | Where-Object { $_.Name -eq 'SnakeController.cs' }
Feature 'Dokunmatik/swipe input' 'Touchscreen|Touch\.current|primaryTouch|Gyroscope|Accelerometer' 'DirectionInput yalniz klavye; Android icin dokunmatik gerek'
Feature 'Duraklat (pause)' 'Time\.timeScale|\bPause\b|Duraklat' 'oyun duraklatilamiyor'
Feature 'Muzik caliniyor' 'musicSource\.Play|musicSource\.clip\s*=' 'musicSource atanmis ama hic Play edilmiyor -> sessiz'
Feature 'Yuksek skor ekranda' 'BestScore' 'en iyi skor UI gosterimi'
Feature 'Ayarlar menusu' 'Settings(Panel|Menu)|sesAc|sesKapat|volume\s*=\s*[a-zA-Z]' 'ses/zorluk ayar ekrani yok'
Feature 'Ilk-oynama yonergesi' 'Nasil Oyna|Yonerge|Tutorial|Ogretici|Talimat|Ipucu' 'cocuga nasil oynanacagi anlatilmiyor'
Feature 'Yilan hareket interpolasyonu' 'Lerp|Slerp|SmoothDamp|MoveTowards' 'yilan hucreden hucreye animasyonsuz zipliyor (cila firsati)' $snakeFile
Emit ""

# 5) Yarim is izleri
Emit "## 5. YARIM IS IZLERI"
$todo = FindHits 'TODO|FIXME|HACK|NotImplementedException' ($runtime + $editor)
if ($todo.Count -gt 0) { Emit ("Isaret: " + ($todo -join '; ')) } else { Emit "TODO/FIXME/NotImplemented yok (temiz)" }
Emit ""

# 6) GameConfig alan kullanimi
Emit "## 6. GAMECONFIG ALAN KULLANIMI"
$cfg = $runtime | Where-Object { $_.Name -eq 'GameConfig.cs' } | Select-Object -First 1
if ($cfg) {
    $cfgFields = @()
    foreach ($m in [regex]::Matches($doc[$cfg.FullName], 'public\s+[\w<>\[\]]+\s+(\w+)\s*(=|;)')) { $cfgFields += $m.Groups[1].Value }
    $cfgFields = $cfgFields | Sort-Object -Unique
    $orphans = @()
    foreach ($fld in $cfgFields) {
        $usedElsewhere = ([regex]::Matches($allRuntime, '\.' + [regex]::Escape($fld) + '\b')).Count
        if ($usedElsewhere -eq 0) { $orphans += $fld }
    }
    Emit ("GameConfig alani: " + $cfgFields.Count + " | Kodda referanssiz (oksuz): " + (@($orphans).Count))
    if ($orphans.Count -gt 0) { Emit ("  OKSUZ: " + ($orphans -join ', ')) }
} else { Emit "GameConfig.cs bulunamadi" }
Emit ""

# 7) Test kapsam haritasi
Emit "## 7. TEST KAPSAM HARITASI (runtime sinifi/enum -> testte adi geciyor mu)"
$allTests = ($tests | ForEach-Object { $doc[$_.FullName] }) -join "`n"
$classes = @()
foreach ($f in $runtime) {
    foreach ($m in [regex]::Matches($doc[$f.FullName], 'class\s+(\w+)')) { $classes += $m.Groups[1].Value }
    foreach ($m in [regex]::Matches($doc[$f.FullName], 'enum\s+(\w+)'))  { $classes += $m.Groups[1].Value }
}
$classes = $classes | Sort-Object -Unique
$covered = @(); $uncovered = @()
foreach ($c in $classes) {
    if ($allTests -match ('\b' + [regex]::Escape($c) + '\b')) { $covered += $c } else { $uncovered += $c }
}
Emit ("Kapsanan: " + ($covered -join ', '))
Emit ("KAPSANMAYAN: " + ($uncovered -join ', '))
Emit ""

# 8) Hafif allocation sezgisi
Emit "## 8. HAFIF ALLOCATION SEZGISI (koleksiyon tahsisi noktalari)"
$alloc = FindHits 'new\s+List<|new\s+Queue<|new\s+\w+\[' $runtime
if ($alloc.Count -gt 0) { Emit ("Tahsis (sicak yolda mi model degerlendirir): " + ($alloc -join '; ')) }
else { Emit "Belirgin koleksiyon tahsisi yok" }
Emit ""

Emit "=== OLGU SAYFASI SONU ==="

$o -join $nl | Write-Output
exit 0
