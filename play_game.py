import requests
import json
import time
import subprocess

def play_game_automatically():
    headers = {"Content-Type": "application/json"}

    while True:
        try:
            # Get the next move from the solver script
            # Using subprocess to run the Python solver and capture its output
            result = subprocess.run(["python", "solve_next_move.py"], capture_output=True, text=True, check=True)
            move_json = result.stdout.strip()

            if "Game is not running" in move_json:
                # Fetch final status to display Won/Lost like the PowerShell script does
                try:
                    status_res = requests.get("http://localhost:5000/api/game")
                    if status_res.status_code == 200:
                        print(f"Game Over: {status_res.json().get('status')}")
                    else:
                        print("Game Over (Could not fetch status)")
                except:
                    print("Game Over (Error fetching status)")
                break

            if not move_json or "No moves found" in move_json:
                print(move_json)
                print("Game Over or Stuck.")
                break

            move = json.loads(move_json)
            
            reason = move.get("reason", "N/A")
            print(f"Executing: {move['action']} at ({move['column']}, {move['row']}) - Reason: {reason}")
            
            # Send move to API
            response = requests.post("http://localhost:5000/api/game/move", headers=headers, json=move)
            response.raise_for_status() # Raise an exception for HTTP errors (4xx or 5xx)
            
            # Brief pause to see action and allow state update
            time.sleep(0.2) 

        except requests.exceptions.RequestException as e:
            print(f"API request failed: {e}")
            break
        except json.JSONDecodeError as e:
            print(f"Failed to decode JSON from solver: {e}")
            print(f"Raw solver output: {move_json}")
            break
        except subprocess.CalledProcessError as e:
            print(f"Solver script failed with error: {e.stderr}")
            break
        except Exception as e:
            print(f"An unexpected error occurred: {e}")
            break

if __name__ == "__main__":
    play_game_automatically()
