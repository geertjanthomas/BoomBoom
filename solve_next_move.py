import requests
import json

def get_neighbors(c, r, width, height):
    neighbors = []
    for dc in [-1, 0, 1]:
        for dr in [-1, 0, 1]:
            if dc == 0 and dr == 0:
                continue
            nc, nr = c + dc, r + dr
            if 0 <= nc < width and 0 <= nr < height:
                neighbors.append((nc, nr))
    return neighbors

def solve():
    try:
        response = requests.get("http://localhost:5000/api/game")
        if response.status_code != 200:
            print("Error fetching game state")
            return

        data = response.json()
        if not data["running"]:
            print("Game is not running")
            return

        width = data["width"]
        height = data["height"]
        grid_list = data["grid"]
        
        # Convert list to dict for easier access
        grid = {}
        for cell in grid_list:
            grid[(cell["column"], cell["row"])] = cell

        moves = []

        # Simple logic: Single Pass
        for c in range(width):
            for r in range(height):
                cell = grid[(c, r)]
                if not cell["exposed"]:
                    continue
                
                adjacent = cell.get("adjacent")
                if adjacent is None or adjacent == 0:
                    continue

                neighbors = get_neighbors(c, r, width, height)
                unexposed_neighbors = []
                flagged_neighbors = 0

                for nc, nr in neighbors:
                    n_cell = grid[(nc, nr)]
                    if n_cell["flagged"]:
                        flagged_neighbors += 1
                    elif not n_cell["exposed"]:
                        unexposed_neighbors.append((nc, nr))

                # Logic 1: All bombs found, rest are safe
                if adjacent == flagged_neighbors:
                    for uc, ur in unexposed_neighbors:
                        if (uc, ur) not in [m[0:2] for m in moves]:
                             moves.append((uc, ur, "click"))

                # Logic 2: Remaining hidden neighbors must be bombs
                if adjacent == flagged_neighbors + len(unexposed_neighbors):
                    for uc, ur in unexposed_neighbors:
                        if (uc, ur) not in [m[0:2] for m in moves]:
                             moves.append((uc, ur, "flag"))

        if moves:
            # Prioritize clicks over flags to progress game
            clicks = [m for m in moves if m[2] == "click"]
            if clicks:
                print(json.dumps(clicks[0]))
            else:
                print(json.dumps(moves[0]))
        else:
            # If no obvious logic, try to pick a random unexposed unflagged cell 
            # (In a real solver, we'd calculate probabilities, but this is a starter)
            # Find a corner or edge? Or just first available.
             for c in range(width):
                for r in range(height):
                     cell = grid[(c,r)]
                     if not cell["exposed"] and not cell["flagged"]:
                         print(json.dumps([c, r, "click"]))
                         return
             print("No moves found")

    except Exception as e:
        print(f"Error: {e}")

if __name__ == "__main__":
    solve()
