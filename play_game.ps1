$headers = @{ "Content-Type" = "application/json" }

while ($true) {
    # Get the next move from the solver script
    $moveJson = powershell -ExecutionPolicy Bypass -File solve_next_move.ps1

    if ($null -eq $moveJson -or $moveJson -eq "") {
        Write-Output "No move returned. Game Over or Stuck."
        break
    }

    if ($moveJson -match "Game is not running") {
        $statusResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/game"
        Write-Output "Game Over: $($statusResponse.Status)"
        break
    }

    try {
        $move = $moveJson | ConvertFrom-Json
        
        Write-Output "Executing: $($move.action) at ($($move.column), $($move.row))"
        
        # Send move to API
        Invoke-RestMethod -Method Post -Uri "http://localhost:5000/api/game/move" -Headers $headers -Body $moveJson
        
        # Brief pause to see action and allow state update
        Start-Sleep -Milliseconds 200
    }
    catch {
        Write-Output "Error executing move or parsing JSON: $_"
        Write-Output "Raw Output: $moveJson"
        break
    }
}
