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
        
        grid = {}
        for cell in grid_list:
            grid[(cell["column"], cell["row"])] = cell

        virtual_flags = []

        # Pass 1: Identify "Must be Bomb" cells (Virtual Flags)
        for c in range(width):
            for r in range(height):
                cell = grid[(c, r)]
                if not cell["exposed"]: # Only consider exposed cells for logic
                    continue
                
                adjacent = cell.get("adjacent")
                if adjacent is None or adjacent == 0:
                    continue

                neighbors = get_neighbors(c, r, width, height)
                unexposed_neighbors = []
                flagged_count = 0

                for nc, nr in neighbors:
                    n_cell = grid[(nc, nr)]
                    if n_cell["flagged"]:
                        flagged_count += 1
                    elif not n_cell["exposed"]:
                        unexposed_neighbors.append((nc, nr))

                # If remaining hidden neighbors MUST be bombs
                if adjacent == flagged_count + len(unexposed_neighbors):
                    for uc, ur in unexposed_neighbors:
                        if (uc, ur) not in [(vf[0], vf[1]) for vf in virtual_flags]:
                            virtual_flags.append((uc, ur))

        # Pass 2: Identify "Must be Safe" cells using Real + Virtual Flags
        click_moves = []

        for c in range(width):
            for r in range(height):
                cell = grid[(c, r)]
                if not cell["exposed"]: # Only consider exposed cells for logic
                    continue
                
                adjacent = cell.get("adjacent")
                if adjacent is None or adjacent == 0:
                    continue

                neighbors = get_neighbors(c, r, width, height)
                unexposed_neighbors = []
                flagged_count = 0

                for nc, nr in neighbors:
                    n_cell = grid[(nc, nr)]
                    
                    # Check if flagged OR is a virtual flag
                    is_virtual_flag = (nc, nr) in virtual_flags
                    
                    if n_cell["flagged"] or is_virtual_flag:
                        flagged_count += 1
                    elif not n_cell["exposed"]:
                        unexposed_neighbors.append((nc, nr))

                # If all bombs are accounted for (Real or Virtual), rest are safe
                if adjacent == flagged_count:
                    for uc, ur in unexposed_neighbors:
                        # Don't click if we previously thought it was a bomb (contradiction check)
                        if (uc, ur) not in virtual_flags:
                            if (uc, ur, "click") not in click_moves:
                                click_moves.append((uc, ur, "click"))

        # Execute Moves
        # Priority 1: Safe Clicks
        if click_moves:
            print(json.dumps({"column": click_moves[0][0], "row": click_moves[0][1], "action": click_moves[0][2]}))
            return

        # Priority 2: Flagging (if no safe clicks found, we finalize the virtual flags)
        if virtual_flags:
            # Only flag unflagged cells
            for vf_c, vf_r in virtual_flags:
                n_cell = grid[(vf_c, vf_r)]
                if not n_cell["flagged"]:
                    print(json.dumps({"column": vf_c, "row": vf_r, "action": "flag"}))
                    return

        # Priority 3: Random (Guess)
        for c in range(width):
            for r in range(height):
                cell = grid[(c,r)]
                if not cell["exposed"] and not cell["flagged"]:
                    print(json.dumps({"column": c, "row": r, "action": "click"}))
                    return
        print("No moves found")

    except Exception as e:
        print(f"Error: {e}")

if __name__ == "__main__":
    solve()