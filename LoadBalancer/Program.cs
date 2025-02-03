using System;
using System.Threading.Tasks;

class Program
{
    // Main entry point for the program
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Load Balancer started. Press 'Enter' to send a request.");

        while (true)
        {
            Console.ReadLine(); // Wait for user input to send a request

            // Get the next available backend server using the LoadBalancer
            var backend = await LoadBalancer.GetNextAvailableBackendAsync();
            if (backend == null)
            {
                Console.WriteLine("All backends are unavailable. Please try again later.");
                continue;
            }

            // Send request to the selected backend
            Console.WriteLine($"Sending request to {backend}");
            await LoadBalancer.SendRequestAsync(backend); // Use LoadBalancer's method
        }
    }
}
