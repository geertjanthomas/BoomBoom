import requests
import sys
import argparse

def start_new_game(difficulty="Intermediate"):
    url = "http://localhost:5000/api/game/new"
    headers = {"Content-Type": "application/json"}
    payload = {"difficulty": difficulty}

    try:
        # Check if server is reachable first (optional, but good for specific error msg)
        try:
            requests.get("http://localhost:5000/api/game", timeout=2)
        except requests.exceptions.ConnectionError:
             print("Error: The BoomBoom game application is not running.")
             print("Please start the game executable first.")
             sys.exit(1)

        response = requests.post(url, json=payload, headers=headers)
        response.raise_for_status()
        print(f"New game started successfully! Difficulty: {difficulty}")
        
    except requests.exceptions.HTTPError as e:
        print(f"Server returned an error: {e}")
        print(f"Response content: {response.text}")
        sys.exit(1)
    except Exception as e:
        print(f"An unexpected error occurred: {e}")
        sys.exit(1)

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Start a new BoomBoom game.")
    parser.add_argument("difficulty", nargs="?", default="Intermediate", 
                        choices=["Beginner", "Intermediate", "Expert", "Custom"],
                        help="Difficulty level (default: Intermediate)")
    
    args = parser.parse_args()
    start_new_game(args.difficulty)
