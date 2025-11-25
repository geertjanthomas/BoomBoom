import requests
import json
import math

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
        try:
            response = requests.get("http://localhost:5000/api/game")
            response.raise_for_status()
        except requests.exceptions.RequestException:
            print("Error connecting to API")
            return

        data = response.json()
        if not data.get("running"):
            print("Game is not running")
            return

        width = data["width"]
        height = data["height"]
        grid_list = data["grid"]
        
        # 1. Build efficient Grid Dictionary and Lists
        grid = {}         # Key (c,r) -> Cell Dict
        exposed = []      # List of exposed cell dicts
        unknown = {}      # Key (c,r) -> Cell Dict (Unexposed AND Not Flagged)
        flags = {}        # Key (c,r) -> Cell Dict (Flagged)

        for cell in grid_list:
            key = (cell["column"], cell["row"])
            grid[key] = cell
            
            if cell["exposed"]:
                exposed.append(cell)
            elif cell["flagged"]:
                flags[key] = cell
            else:
                unknown[key] = cell

        # If start of game (no exposed cells), pick center
        if not exposed:
            cx = width // 2
            cy = height // 2
            print(json.dumps({"column": cx, "row": cy, "action": "click", "reason": "First Move"}))
            return

        moves = [] # List of {"column": c, "row": r, "action": "click"|"flag"}

        # Pre-calculate neighbors for exposed cells
        # Structure: key (c,r) -> { "Cell": cell, "RemBombs": int, "Unknowns": [(c,r), ...], "UnknownKeys": set((c,r)) }
        frontier = {}

        for cell in exposed:
            # Skip empty cells (0 adjacent) or fully solved ones (handled implicitly by RemBombs=0 check logic usually, but strict 0 is trivial)
            adjacent = cell.get("adjacent")
            if adjacent is None or adjacent == 0:
                continue

            n_coords = get_neighbors(cell["column"], cell["row"], width, height)
            my_unknowns = []
            my_flags = 0

            for (nc, nr) in n_coords:
                nk = (nc, nr)
                if nk in flags:
                    my_flags += 1
                elif nk in unknown:
                    my_unknowns.append(nk)
            
            rem_bombs = adjacent - my_flags

            # Basic Logic: 100% Safe or 100% Bomb
            if rem_bombs == 0:
                # All remaining unknowns are SAFE
                for (uc, ur) in my_unknowns:
                    moves.append({"column": uc, "row": ur, "action": "click"})
            elif rem_bombs == len(my_unknowns):
                # All remaining unknowns are BOMBS
                for (uc, ur) in my_unknowns:
                    moves.append({"column": uc, "row": ur, "action": "flag"})
            else:
                # Add to frontier for advanced logic
                key = (cell["column"], cell["row"])
                frontier[key] = {
                    "Cell": cell,
                    "RemBombs": rem_bombs,
                    "Unknowns": my_unknowns,
                    "UnknownKeys": set(my_unknowns) # Set for fast subset check
                }

        # If we found basic moves, execute the first one (Prioritize CLICKS over FLAGS)
        click_move = next((m for m in moves if m["action"] == "click"), None)
        if click_move:
            click_move["reason"] = "Basic Logic"
            print(json.dumps(click_move))
            return

        flag_move = next((m for m in moves if m["action"] == "flag"), None)
        if flag_move:
            flag_move["reason"] = "Basic Logic"
            print(json.dumps(flag_move))
            return

        # 2. Advanced Logic: Set/Subset Analysis
        # Compare every pair of frontier cells
        frontier_keys = list(frontier.keys())
        frontier_keys.sort() # Consistent order

        for i in range(len(frontier_keys)):
            key_a = frontier_keys[i]
            data_a = frontier[key_a]
            
            for j in range(i + 1, len(frontier_keys)):
                key_b = frontier_keys[j]
                data_b = frontier[key_b]

                # Optimization: Check distance
                if abs(key_a[0] - key_b[0]) > 2 or abs(key_a[1] - key_b[1]) > 2:
                    continue

                set_a = data_a["UnknownKeys"]
                set_b = data_b["UnknownKeys"]

                if not set_a or not set_b:
                    continue

                # Check IsSubset: A subset of B
                if set_a.issubset(set_b):
                    diff_bombs = data_b["RemBombs"] - data_a["RemBombs"]
                    diff_keys = set_b - set_a

                    if diff_keys:
                        if diff_bombs == 0:
                            # All diff cells are SAFE
                            dk = list(diff_keys)[0]
                            print(json.dumps({"column": dk[0], "row": dk[1], "action": "click", "reason": "Subset Safe"}))
                            return
                        elif diff_bombs == len(diff_keys):
                            # All diff cells are BOMBS
                            dk = list(diff_keys)[0]
                            print(json.dumps({"column": dk[0], "row": dk[1], "action": "flag", "reason": "Subset Flag"}))
                            return

                # Check Reverse: B subset of A
                if set_b.issubset(set_a):
                    diff_bombs = data_a["RemBombs"] - data_b["RemBombs"]
                    diff_keys = set_a - set_b

                    if diff_keys:
                        if diff_bombs == 0:
                            dk = list(diff_keys)[0]
                            print(json.dumps({"column": dk[0], "row": dk[1], "action": "click", "reason": "Subset Safe"}))
                            return
                        elif diff_bombs == len(diff_keys):
                            dk = list(diff_keys)[0]
                            print(json.dumps({"column": dk[0], "row": dk[1], "action": "flag", "reason": "Subset Flag"}))
                            return

        # 3. Probability Guessing
        # Find frontier cell with lowest Bomb Density
        best_prob = 100.0
        best_move = None

        for key in frontier_keys:
            data = frontier[key]
            if len(data["Unknowns"]) > 0:
                prob = data["RemBombs"] / len(data["Unknowns"])
                if prob < best_prob:
                    best_prob = prob
                    best_move = data["Unknowns"][0] # Pick first neighbor
        
        if best_move:
            print(json.dumps({"column": best_move[0], "row": best_move[1], "action": "click", "reason": f"Probability Guess ({best_prob:.2f})"}))
            return

        # 4. Blind Guess (Detached Islands)
        if unknown:
            sorted_keys = sorted(unknown.keys())
            k = sorted_keys[0]
            print(json.dumps({"column": k[0], "row": k[1], "action": "click", "reason": "Blind Guess"}))
            return

        print("No moves found")

    except Exception as e:
        print(f"Error: {e}")

if __name__ == "__main__":
    solve()