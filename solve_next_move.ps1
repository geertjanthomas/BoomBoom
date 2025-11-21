$response = Invoke-RestMethod -Uri "http://localhost:5000/api/game"
if (-not $response.running) {
    Write-Output "Game is not running"
    exit
}

$width = $response.width
$height = $response.height
$grid = $response.grid

function Get-Cell($c, $r) {
    return $grid | Where-Object { $_.column -eq $c -and $_.row -eq $r }
}

function Get-Neighbors($c, $r) {
    $neighbors = @()
    foreach ($dc in -1..1) {
        foreach ($dr in -1..1) {
            if ($dc -eq 0 -and $dr -eq 0) { continue }
            $nc = $c + $dc
            $nr = $r + $dr
            if ($nc -ge 0 -and $nc -lt $width -and $nr -ge 0 -and $nr -lt $height) {
                $neighbors += [PSCustomObject]@{ column = $nc; row = $nr }
            }
        }
    }
    return $neighbors
}

$virtualFlags = @()

# Pass 1: Identify "Must be Bomb" cells (Virtual Flags)
foreach ($cell in $grid) {
    if (-not $cell.exposed) { continue }
    if ($null -eq $cell.adjacent -or $cell.adjacent -eq 0) { continue }

    $neighbors = Get-Neighbors $cell.column $cell.row
    $unexposed = @()
    $flaggedCount = 0

    foreach ($n in $neighbors) {
        $nCell = Get-Cell $n.column $n.row
        if ($nCell.flagged) {
            $flaggedCount++
        }
        elseif (-not $nCell.exposed) {
            $unexposed += $nCell
        }
    }

    # If remaining hidden neighbors MUST be bombs
    if ($cell.adjacent -eq ($flaggedCount + $unexposed.Count)) {
        foreach ($u in $unexposed) {
             $exists = $virtualFlags | Where-Object { $_.column -eq $u.column -and $_.row -eq $u.row }
             if (-not $exists) {
                $virtualFlags += $u
             }
        }
    }
}

# Pass 2: Identify "Must be Safe" cells using Real + Virtual Flags
$clickMoves = @()

foreach ($cell in $grid) {
    if (-not $cell.exposed) { continue }
    if ($null -eq $cell.adjacent -or $cell.adjacent -eq 0) { continue }

    $neighbors = Get-Neighbors $cell.column $cell.row
    $unexposed = @()
    $flaggedCount = 0

    foreach ($n in $neighbors) {
        $nCell = Get-Cell $n.column $n.row
        
        # Check if flagged OR is a virtual flag
        $isVirtualFlag = $virtualFlags | Where-Object { $_.column -eq $n.column -and $_.row -eq $n.row }
        
        if ($nCell.flagged -or $isVirtualFlag) {
            $flaggedCount++
        }
        elseif (-not $nCell.exposed) {
            $unexposed += $nCell
        }
    }

    # If all bombs are accounted for (Real or Virtual), rest are safe
    if ($cell.adjacent -eq $flaggedCount) {
        foreach ($u in $unexposed) {
            # Don't click if we previously thought it was a bomb (contradiction check, though standard logic shouldn't overlap like that unless board is invalid)
            $isVirtual = $virtualFlags | Where-Object { $_.column -eq $u.column -and $_.row -eq $u.row }
            if (-not $isVirtual) {
                $exists = $clickMoves | Where-Object { $_.column -eq $u.column -and $_.row -eq $u.row }
                if (-not $exists) {
                    $clickMoves += @{ column = $u.column; row = $u.row; action = "click" }
                }
            }
        }
    }
}

# Execute Moves
# Priority 1: Safe Clicks
if ($clickMoves.Count -gt 0) {
    Write-Output (ConvertTo-Json $clickMoves[0] -Compress)
    exit
}

# Priority 2: Flagging (if no safe clicks found, we finalize the virtual flags)
if ($virtualFlags.Count -gt 0) {
    # Only flag unflagged cells
    foreach ($vf in $virtualFlags) {
        if (-not $vf.flagged) {
             $move = @{ column = $vf.column; row = $vf.row; action = "flag" }
             Write-Output (ConvertTo-Json $move -Compress)
             exit
        }
    }
}

# Priority 3: Random (Guess)
foreach ($cell in $grid) {
    if (-not $cell.exposed -and -not $cell.flagged) {
        $move = @{ column = $cell.column; row = $cell.row; action = "click" }
        Write-Output (ConvertTo-Json $move -Compress)
        exit
    }
}
