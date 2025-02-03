using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

public static class LoadBalancer
{
    private static List<string> _backends = new List<string>
    {
        "http://localhost:5001",
        "http://localhost:5002",
        "http://localhost:5003"
    };

    private static int _currentIndex = 0;
    private static readonly HttpClient _httpClient = new HttpClient();

    // Get the next available backend server using async approach.
    public static async Task<string> GetNextAvailableBackendAsync()
    {
        int maxAttempts = _backends.Count;
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            var backend = _backends[_currentIndex];

            // Try sending a health check request
            if (await IsBackendAvailableAsync(backend))
            {
                return backend;
            }

            // Move to the next backend only if the current one failed
            _currentIndex = (_currentIndex + 1) % _backends.Count;
            attempts++;
        }

        return null; // All backends are unavailable
    }

    // Check if a backend is available
    private static async Task<bool> IsBackendAvailableAsync(string backend)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2)); // Per-request timeout
            var response = await _httpClient.GetAsync(backend, cts.Token);
            return response.IsSuccessStatusCode;
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine($"Timeout while calling {backend}. Skipping...");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error contacting {backend}: {ex.Message}");
        }
        return false;
    }

    // Send request to the backend
    public static async Task SendRequestAsync(string backend)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            var response = await _httpClient.GetAsync(backend, cts.Token);

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Success: {backend} responded with {content}");
            }
            else
            {
                Console.WriteLine($"Error: {backend} returned {response.StatusCode}");
            }
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine($"Timeout while calling {backend}. Request skipped.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Request failed: {backend}. Error: {ex.Message}");
        }
    }
}
