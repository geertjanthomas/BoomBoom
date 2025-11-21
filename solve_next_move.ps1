$ErrorActionPreference = "Stop"

# --- Helpers ---

function Get-Neighbors($c, $r, $width, $height) {
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

function Get-Key($c, $r) {
    return "$c,$r"
}

# --- Main Logic ---

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/game"
} catch {
    Write-Output "Error connecting to API"
    exit
}

if (-not $response.running) {
    Write-Output "Game is not running"
    exit
}

$width = $response.width
$height = $response.height
$rawGrid = $response.grid

# 1. Build efficient Grid Dictionary and Lists
$grid = @{}        # Key "c,r" -> Cell Object
$exposed = @()     # List of exposed cells
$unknown = @{}     # Key "c,r" -> Cell Object (Unexposed AND Not Flagged)
$flags = @{}       # Key "c,r" -> Cell Object (Flagged)

foreach ($cell in $rawGrid) {
    $key = Get-Key $cell.column $cell.row
    $grid[$key] = $cell
    
    if ($cell.exposed) {
        $exposed += $cell
    } elseif ($cell.flagged) {
        $flags[$key] = $cell
    } else {
        $unknown[$key] = $cell
    }
}

# If start of game (no exposed cells), pick center
if ($exposed.Count -eq 0) {
    $cx = [math]::Floor($width / 2)
    $cy = [math]::Floor($height / 2)
    Write-Output (ConvertTo-Json @{ column = $cx; row = $cy; action = "click" } -Compress)
    exit
}

$moves = @() # List of {c, r, type='click'|'flag'}

# Pre-calculate neighbors for exposed cells to avoid re-doing it constantly
# Structure: @{ CellKey = @{ RemBombs = int; UnknownNeighbors = @(coords) } }
$frontier = @{} 

foreach ($cell in $exposed) {
    # Skip empty cells (0 adjacent) effectively, unless we need them for something else, 
    # but logic usually applies to numbered cells.
    if ($null -eq $cell.adjacent -or $cell.adjacent -eq 0) { continue }

    $nCoords = Get-Neighbors $cell.column $cell.row $width $height
    $myUnknowns = @()
    $myFlags = 0

    foreach ($n in $nCoords) {
        $nk = Get-Key $n.column $n.row
        if ($flags.ContainsKey($nk)) {
            $myFlags++
        } elseif ($unknown.ContainsKey($nk)) {
            $myUnknowns += $n
        }
    }

    $remBombs = $cell.adjacent - $myFlags

    # Clean up already satisfied cells
    if ($remBombs -eq 0) {
        # All remaining unknowns are SAFE
        foreach ($u in $myUnknowns) {
            $moves += @{ column = $u.column; row = $u.row; action = "click" }
        }
    } elseif ($remBombs -eq $myUnknowns.Count) {
        # All remaining unknowns are BOMBS
        foreach ($u in $myUnknowns) {
            $moves += @{ column = $u.column; row = $u.row; action = "flag" }
        }
    } else {
        # Add to frontier for advanced logic
        $key = Get-Key $cell.column $cell.row
        $frontier[$key] = @{
            Cell = $cell
            RemBombs = $remBombs
            Unknowns = $myUnknowns
            UnknownKeys = ($myUnknowns | ForEach-Object { Get-Key $_.column $_.row })
        }
    }
}

# If we found basic moves, execute the first one (Prioritize CLICKS over FLAGS for safety/speed)
$clickMove = $moves | Where-Object { $_.action -eq "click" } | Select-Object -First 1
if ($clickMove) {
    Write-Output (ConvertTo-Json $clickMove -Compress)
    exit
}

$flagMove = $moves | Where-Object { $_.action -eq "flag" } | Select-Object -First 1
if ($flagMove) {
    Write-Output (ConvertTo-Json $flagMove -Compress)
    exit
}

# 2. Advanced Logic: Set/Subset Analysis
# Compare every pair of frontier cells
$frontierKeys = $frontier.Keys | Sort-Object # Sort for consistency
for ($i = 0; $i -lt $frontierKeys.Count; $i++) {
    $keyA = $frontierKeys[$i]
    $dataA = $frontier[$keyA]
    
    for ($j = $i + 1; $j -lt $frontierKeys.Count; $j++) {
        $keyB = $frontierKeys[$j]
        $dataB = $frontier[$keyB]

        # Optimization: Check distance (only compare if they share neighbors)
        if ([math]::Abs($dataA.Cell.column - $dataB.Cell.column) -gt 2 -or 
            [math]::Abs($dataA.Cell.row - $dataB.Cell.row) -gt 2) {
            continue
        }

        # Check if A is subset of B (Unexposed cells of A are all in B)
        # We use the string keys for easier comparison
        $setA = $dataA.UnknownKeys
        $setB = $dataB.UnknownKeys

        if ($setA.Count -eq 0 -or $setB.Count -eq 0) { continue }

        # Check IsSubset: All keys in A exist in B
        $isSubset = $true
        foreach ($k in $setA) {
            if ($setB -notcontains $k) { $isSubset = $false; break }
        }

        if ($isSubset) {
            # Logic: B contains everything in A.
            # The "Difference" cells are those in B but not in A.
            # The number of bombs in the Difference is RemBombs(B) - RemBombs(A).
            
            $diffBombs = $dataB.RemBombs - $dataA.RemBombs
            $diffKeys = $setB | Where-Object { $setA -notcontains $_ }
            
            if ($diffKeys.Count -gt 0) {
                if ($diffBombs -eq 0) {
                    # All difference cells are SAFE
                    foreach ($dk in $diffKeys) {
                        $parts = $dk -split ","
                        Write-Output (ConvertTo-Json @{ column = [int]$parts[0]; row = [int]$parts[1]; action = "click" } -Compress)
                        exit
                    }
                } elseif ($diffBombs -eq $diffKeys.Count) {
                    # All difference cells are BOMBS
                    foreach ($dk in $diffKeys) {
                        $parts = $dk -split ","
                        Write-Output (ConvertTo-Json @{ column = [int]$parts[0]; row = [int]$parts[1]; action = "flag" } -Compress)
                        exit
                    }
                }
            }
        }
        
        # Note: We could also check B subset A, but the outer loops will catch that pair eventually 
        # or we swap, but strictly I < J loop structure handles pairs once. 
        # However, subset check is directional. A subset B is different from B subset A.
        # So let's check the reverse too.
        
        $isSubsetRev = $true
        foreach ($k in $setB) {
            if ($setA -notcontains $k) { $isSubsetRev = $false; break }
        }
        
        if ($isSubsetRev) {
            $diffBombs = $dataA.RemBombs - $dataB.RemBombs
            $diffKeys = $setA | Where-Object { $setB -notcontains $_ }
            
            if ($diffKeys.Count -gt 0) {
                if ($diffBombs -eq 0) {
                    foreach ($dk in $diffKeys) {
                        $parts = $dk -split ","
                        Write-Output (ConvertTo-Json @{ column = [int]$parts[0]; row = [int]$parts[1]; action = "click" } -Compress)
                        exit
                    }
                } elseif ($diffBombs -eq $diffKeys.Count) {
                    foreach ($dk in $diffKeys) {
                        $parts = $dk -split ","
                        Write-Output (ConvertTo-Json @{ column = [int]$parts[0]; row = [int]$parts[1]; action = "flag" } -Compress)
                        exit
                    }
                }
            }
        }
    }
}


# 3. Probability Guessing
# If we are here, no logic moves found.
# Find the frontier cell with the lowest Bomb Density.
$bestProb = 100.0
$bestMove = $null

foreach ($key in $frontierKeys) {
    $data = $frontier[$key]
    if ($data.Unknowns.Count -gt 0) {
        $prob = $data.RemBombs / $data.Unknowns.Count
        if ($prob -lt $bestProb) {
            $bestProb = $prob
            # Pick a random neighbor from this set, or the first one
            $bestMove = $data.Unknowns[0]
        }
    }
}

if ($bestMove) {
    Write-Output (ConvertTo-Json @{ column = $bestMove.column; row = $bestMove.row; action = "click" } -Compress)
    exit
}

# 4. Blind Guess (Detached Islands)
# If no frontier exists (rare mid-game, but possible if island solved), pick random unknown
if ($unknown.Count -gt 0) {
    $keys = $unknown.Keys | Sort-Object # consistent sort
    $parts = $keys[0] -split ","
    Write-Output (ConvertTo-Json @{ column = [int]$parts[0]; row = [int]$parts[1]; action = "click" } -Compress)
    exit
}

Write-Output "No moves found"