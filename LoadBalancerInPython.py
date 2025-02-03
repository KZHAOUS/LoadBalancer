import requests
import time
from itertools import cycle

class LoadBalancer:
    def __init__(self):
        self.backends = [
            "http://localhost:5001",  # Backend 1
            "http://localhost:5002",  # Backend 2
            "http://localhost:5003"   # Backend 3
        ]
        self._current_index = 0
        self.timeout = 2  # Timeout in seconds
        self.backend_cycle = cycle(self.backends)  # Create an infinite cycle for round-robin
        
    def get_next_available_backend(self):
        max_attempts = len(self.backends)
        attempts = 0
        
        while attempts < max_attempts:
            backend = next(self.backend_cycle)
            if self.is_backend_available(backend):
                return backend
            attempts += 1
        
        return None  # All backends are unavailable

    def is_backend_available(self, backend):
        try:
            response = requests.get(backend, timeout=self.timeout)
            return response.status_code == 200
        except requests.Timeout:
            print(f"Timeout while calling {backend}. Skipping...")
        except requests.RequestException as e:
            print(f"Error contacting {backend}: {str(e)}")
        
        return False
    
    def send_request(self, backend):
        try:
            response = requests.get(backend, timeout=self.timeout)
            if response.status_code == 200:
                print(f"Success: {backend} responded with {response.text}")
            else:
                print(f"Error: {backend} returned {response.status_code}")
        except requests.Timeout:
            print(f"Timeout while calling {backend}. Request skipped.")
        except requests.RequestException as e:
            print(f"Request failed: {backend}. Error: {str(e)}")


def main():
    load_balancer = LoadBalancer()
    
    print("Load Balancer started. Press 'Enter' to send a request.")
    
    while True:
        input()  # Wait for user input to send a request
        
        backend = load_balancer.get_next_available_backend()  # Get the next available backend server
        
        if backend is None:
            print("All backends are unavailable. Please try again later.")
            continue
        
        print(f"Sending request to {backend}")
        load_balancer.send_request(backend)  # Send request to the selected backend


if __name__ == "__main__":
    main()
