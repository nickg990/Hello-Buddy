<#
.SYNOPSIS
    Parses exercises.md into an idempotent exercise-import.sql for the migrate job.

.DESCRIPTION
    Reads the physiotherapist exercise markdown (one exercise per '## Heading'
    block) and emits SQL that:
      * upserts each distinct ExerciseCategory (keyed on CategoryKey),
      * upserts each Exercise (keyed on ExerciseKey), and
      * rebuilds that exercise's ExerciseInstruction rows.

    The SQL is pure DML wrapped in a transaction, so it is safe to re-run and is
    the data source baked into the migrate image. The delete-vs-upsert behaviour
    (replace vs update) is decided at run time by migrate.sh, NOT here.

    Media filenames from the markdown are stored verbatim in ImageUrl / VideoUrl.
    Each markdown heading becomes its own Exercise (view variants are kept).

.PARAMETER MarkdownPath
    Source markdown. Defaults to exercises.md next to this script.

.PARAMETER OutputPath
    Destination SQL. Defaults to exercise-import.sql next to this script.
#>
[CmdletBinding()]
param(
    [string]$MarkdownPath = (Join-Path $PSScriptRoot 'exercises.md'),
    [string]$OutputPath = (Join-Path $PSScriptRoot 'exercise-import.sql')
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path $MarkdownPath)) {
    throw "Markdown source not found: $MarkdownPath"
}

# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------
function ConvertTo-SqlString {
    # NULL for empty/whitespace; otherwise a single-quoted, escaped literal.
    param([string]$Value)
    if ([string]::IsNullOrWhiteSpace($Value)) { return 'NULL' }
    return "'" + ($Value -replace "'", "''") + "'"
}

function ConvertTo-SqlInt {
    param([string]$Value)
    if ([string]::IsNullOrWhiteSpace($Value)) { return 'NULL' }
    $m = [regex]::Match($Value, '\d+')
    if ($m.Success) { return $m.Value }
    return 'NULL'
}

function ConvertTo-CamelKey {
    # "Alternating Poles - Above View" -> "alternatingPolesAboveView"
    param([string]$Title)
    $clean = $Title -replace "[\u2013\u2014]", ' '   # en/em dash -> space
    $clean = $clean -replace "'", ''                  # drop apostrophes
    $clean = $clean -replace '[^A-Za-z0-9]+', ' '     # non-alnum -> space
    $words = @($clean.Trim() -split '\s+' | Where-Object { $_ -ne '' })
    if ($words.Count -eq 0) { return 'exercise' }
    $sb = [System.Text.StringBuilder]::new()
    for ($i = 0; $i -lt $words.Count; $i++) {
        $w = $words[$i].ToLowerInvariant()
        if ($i -eq 0) {
            [void]$sb.Append($w)
        }
        else {
            [void]$sb.Append($w.Substring(0, 1).ToUpperInvariant())
            if ($w.Length -gt 1) { [void]$sb.Append($w.Substring(1)) }
        }
    }
    $key = $sb.ToString()
    if ($key.Length -gt 100) { $key = $key.Substring(0, 100) }
    return $key
}

function ConvertTo-SnakeKey {
    # "Pole and Obstacle Work" -> "pole_and_obstacle_work"
    param([string]$Name)
    $clean = $Name -replace "[\u2013\u2014]", ' '
    $clean = $clean -replace "'", ''
    $clean = $clean -replace '[^A-Za-z0-9]+', '_'
    $key = $clean.Trim('_').ToLowerInvariant()
    if ($key.Length -gt 100) { $key = $key.Substring(0, 100) }
    return $key
}

# ---------------------------------------------------------------------------
# Parse markdown into exercise records
# ---------------------------------------------------------------------------
$lines = Get-Content -LiteralPath $MarkdownPath -Encoding UTF8

$exercises = [System.Collections.Generic.List[object]]::new()
$current = $null
$inInstructions = $false

function Complete-Block {
    param($block)
    if ($null -eq $block) { return }
    # Trim trailing blank instruction lines.
    while ($block.Instructions.Count -gt 0 -and
        [string]::IsNullOrWhiteSpace($block.Instructions[$block.Instructions.Count - 1])) {
        $block.Instructions.RemoveAt($block.Instructions.Count - 1)
    }
    $exercises.Add($block)
}

foreach ($line in $lines) {
    if ($line -match '^##\s+(.*\S)\s*$') {
        Complete-Block $current
        $current = [pscustomobject]@{
            Title        = $Matches[1].Trim()
            Category     = ''
            Summary      = ''
            Reps         = ''
            Sets         = ''
            Hold         = ''
            Video        = ''
            Image        = ''
            Instructions = [System.Collections.Generic.List[string]]::new()
        }
        $inInstructions = $false
        continue
    }
    if ($null -eq $current) { continue }

    if ($inInstructions) {
        $current.Instructions.Add($line.Trim())
        continue
    }

    switch -regex ($line) {
        '^Category:\s*(.*)$' { $current.Category = $Matches[1].Trim(); continue }
        '^Summary:\s*(.*)$' { $current.Summary = $Matches[1].Trim(); continue }
        '^Reps:\s*(.*)$' { $current.Reps = $Matches[1].Trim(); continue }
        '^Sets:\s*(.*)$' { $current.Sets = $Matches[1].Trim(); continue }
        '^Hold seconds:\s*(.*)$' { $current.Hold = $Matches[1].Trim(); continue }
        '^Video:\s*(.*)$' { $current.Video = $Matches[1].Trim(); continue }
        '^Image:\s*(.*)$' { $current.Image = $Matches[1].Trim(); continue }
        '^Instructions:\s*$' { $inInstructions = $true; continue }
        default { continue }
    }
}
Complete-Block $current

if ($exercises.Count -eq 0) {
    throw "No exercises parsed from $MarkdownPath"
}

# ---------------------------------------------------------------------------
# Assign unique keys and collect categories
# ---------------------------------------------------------------------------
$usedKeys = @{}
$categories = [ordered]@{}   # CategoryKey -> CategoryName

foreach ($ex in $exercises) {
    $key = ConvertTo-CamelKey $ex.Title
    if ($usedKeys.ContainsKey($key)) {
        $n = $usedKeys[$key] + 1
        $usedKeys[$key] = $n
        $suffixed = ($key + $n)
        if ($suffixed.Length -gt 100) { $suffixed = $suffixed.Substring(0, 100) }
        $key = $suffixed
    }
    else {
        $usedKeys[$key] = 1
    }
    $ex | Add-Member -NotePropertyName ExerciseKey -NotePropertyValue $key -Force

    if (-not [string]::IsNullOrWhiteSpace($ex.Category)) {
        $catKey = ConvertTo-SnakeKey $ex.Category
        if (-not $categories.Contains($catKey)) { $categories[$catKey] = $ex.Category.Trim() }
        $ex | Add-Member -NotePropertyName CategoryKey -NotePropertyValue $catKey -Force
    }
    else {
        $ex | Add-Member -NotePropertyName CategoryKey -NotePropertyValue $null -Force
    }
}

# ---------------------------------------------------------------------------
# Emit SQL
# ---------------------------------------------------------------------------
$sb = [System.Text.StringBuilder]::new()
[void]$sb.AppendLine('-- Hello Buddy exercise library import.')
[void]$sb.AppendLine('-- GENERATED FILE - do not edit by hand.')
[void]$sb.AppendLine('-- Source: exercises.md  Generator: Generate-ExerciseImportSql.ps1')
[void]$sb.AppendLine("-- Exercises: $($exercises.Count)  Categories: $($categories.Count)")
[void]$sb.AppendLine('--')
[void]$sb.AppendLine('-- Idempotent: exercises upsert on ExerciseKey, categories on CategoryKey,')
[void]$sb.AppendLine('-- instructions are rebuilt per exercise. Wrapped in a single transaction.')
[void]$sb.AppendLine()
[void]$sb.AppendLine('SET NAMES utf8mb4;')
[void]$sb.AppendLine('START TRANSACTION;')
[void]$sb.AppendLine()

[void]$sb.AppendLine('-- Categories -------------------------------------------------------------')
foreach ($catKey in $categories.Keys) {
    $name = $categories[$catKey]
    [void]$sb.AppendLine(
        "INSERT INTO ExerciseCategory (CategoryKey, CategoryName, IsActive) VALUES ($(ConvertTo-SqlString $catKey), $(ConvertTo-SqlString $name), TRUE)")
    [void]$sb.AppendLine(
        '  ON DUPLICATE KEY UPDATE CategoryName=VALUES(CategoryName), IsActive=TRUE;')
}
[void]$sb.AppendLine()

[void]$sb.AppendLine('-- Exercises --------------------------------------------------------------')
foreach ($ex in $exercises) {
    $keyLit = ConvertTo-SqlString $ex.ExerciseKey
    $titleLit = ConvertTo-SqlString $ex.Title
    $summaryLit = ConvertTo-SqlString $ex.Summary
    $instrText = ($ex.Instructions -join "`n")
    $instrLit = ConvertTo-SqlString $instrText
    $repsLit = ConvertTo-SqlInt $ex.Reps
    $setsLit = ConvertTo-SqlInt $ex.Sets
    $holdLit = ConvertTo-SqlInt $ex.Hold
    $imageLit = ConvertTo-SqlString $ex.Image
    $videoLit = ConvertTo-SqlString $ex.Video

    [void]$sb.AppendLine("-- $($ex.Title)")
    if ($ex.CategoryKey) {
        $catLit = ConvertTo-SqlString $ex.CategoryKey
        [void]$sb.AppendLine('INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)')
        [void]$sb.AppendLine("SELECT ec.ExerciseCategoryId, $keyLit, $titleLit, $summaryLit, $instrLit, $repsLit, $setsLit, $holdLit, TRUE, $imageLit, $videoLit")
        [void]$sb.AppendLine("  FROM ExerciseCategory ec WHERE ec.CategoryKey=$catLit")
    }
    else {
        [void]$sb.AppendLine('INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)')
        [void]$sb.AppendLine("VALUES (NULL, $keyLit, $titleLit, $summaryLit, $instrLit, $repsLit, $setsLit, $holdLit, TRUE, $imageLit, $videoLit)")
    }
    [void]$sb.AppendLine('  ON DUPLICATE KEY UPDATE')
    [void]$sb.AppendLine('    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),')
    [void]$sb.AppendLine('    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),')
    [void]$sb.AppendLine('    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);')

    # Rebuild instructions for this exercise.
    [void]$sb.AppendLine("DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey=$keyLit;")
    $step = 0
    foreach ($instruction in $ex.Instructions) {
        if ([string]::IsNullOrWhiteSpace($instruction)) { continue }
        $step++
        $stepLit = ConvertTo-SqlString $instruction
        [void]$sb.AppendLine("INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, $step, $stepLit FROM Exercise e WHERE e.ExerciseKey=$keyLit;")
    }
    [void]$sb.AppendLine()
}

[void]$sb.AppendLine('COMMIT;')

# UTF-8 without BOM so the MySQL client reads clean utf8mb4.
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText($OutputPath, $sb.ToString(), $utf8NoBom)

Write-Host "Wrote $OutputPath" -ForegroundColor Green
Write-Host "  Exercises: $($exercises.Count)  Categories: $($categories.Count)"
