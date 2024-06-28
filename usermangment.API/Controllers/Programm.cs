using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace EventsApiClient
{
    class Programm
    {
        private static readonly HttpClient client = new HttpClient();

        static async Task Main(string[] args)
        {
            client.BaseAddress = new Uri("http://yourdomain/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            await GetEvents();
            await AddEvent();
            await DeleteEvent(3);
            await GetEventsByDateRange(new DateTime(2024, 6, 1), new DateTime(2024, 6, 30));
            await GetEventsByCategory("Music");
        }

        static async Task GetEvents()
        {
            HttpResponseMessage response = await client.GetAsync("api/events");
            if (response.IsSuccessStatusCode)
            {
                var events = await response.Content.ReadAsStringAsync();
                Console.WriteLine(events);
            }
        }

        static async Task AddEvent()
        {
            var newEvent = new
            {
                Name = "New Event",
                EmailOfArtist = "artist@example.com",
                DateAndTime = new DateTime(2024, 8, 1, 18, 30, 0),
                Category = "Theater"
            };

            HttpResponseMessage response = await client.PostAsJsonAsync("api/events", newEvent);
            if (response.IsSuccessStatusCode)
            {
                var addedEvent = await response.Content.ReadAsStringAsync();
                Console.WriteLine(addedEvent);
            }
        }

        static async Task DeleteEvent(int id)
        {
            HttpResponseMessage response = await client.DeleteAsync($"api/events/{id}");
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Event with id {id} deleted.");
            }
        }

        static async Task GetEventsByDateRange(DateTime startDate, DateTime endDate)
        {
            HttpResponseMessage response = await client.GetAsync($"api/events/filter-by-date?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}");
            if (response.IsSuccessStatusCode)
            {
                var events = await response.Content.ReadAsStringAsync();
                Console.WriteLine(events);
            }
        }

        static async Task GetEventsByCategory(string category)
        {
            HttpResponseMessage response = await client.GetAsync($"api/events/filter-by-category?category={category}");
            if (response.IsSuccessStatusCode)
            {
                var events = await response.Content.ReadAsStringAsync();
                Console.WriteLine(events);
            }
        }
    }
}
