using Asteroids.Application.Client;
using Asteroids.Domain.Entities;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json.Linq;

namespace Asteroids.Application.Services
{
    public class NeoService : INeoService
    {
        private readonly HttpClient _client;

        private readonly INeoClientSettings _neoClientSettings;

        public NeoService(HttpClient client, INeoClientSettings neoClientSettings)
        {
            _neoClientSettings = neoClientSettings;
            _client = client;
            _client.BaseAddress = new Uri(_neoClientSettings.BaseUrl);
        }

        public async Task<List<Neo>> GetNeoListAsync(int days)
        {
            var neos = new List<Neo>();

            var startDate = DateTime.Today;
            var endDate = startDate.AddDays(days);

            var endpointUrl = BuildEndpointUrl(startDate, endDate);

            using var responseBody = await _client.GetAsync(endpointUrl);

            if (responseBody.IsSuccessStatusCode)
            {
                var response = await responseBody.Content.ReadAsStringAsync();
                var jsonResponse = JObject.Parse(response);

                for (int i = 0; i <= days; i++)
                {
                    var neoDay = jsonResponse.SelectToken("near_earth_objects." + startDate.AddDays(i).ToString("yyyy-MM-dd"));

                    if (neoDay == null)
                    {
                        return new List<Neo>();
                    }

                    foreach (var neoData in neoDay)
                    {
                        if (neoData == null)
                        {
                            continue;
                        }

                        var neo = BuildNeo(neoData);

                        neos.Add(neo);
                    }
                }
            }

            return neos.OrderByDescending(x => x.Diameter).Take(3).ToList();
        }

        private string BuildEndpointUrl(DateTime startDate, DateTime endDate)
        {
            var parameters = new Dictionary<string, string>
            {
                { "start_date", startDate.ToString("yyyy-MM-dd") },
                { "end_date", endDate.ToString("yyyy-MM-dd") },
                { "api_key", _neoClientSettings.ApiKey }
            };

            return QueryHelpers.AddQueryString(_client.BaseAddress?.AbsoluteUri, parameters);
        }

        private static Neo BuildNeo(JToken neoData)
        {
            var name = neoData.SelectToken("name").ToString();
            var maxDiameter = (decimal)neoData.SelectToken("estimated_diameter.kilometers.estimated_diameter_max");
            var minDiameter = (decimal)neoData.SelectToken("estimated_diameter.kilometers.estimated_diameter_min");
            var velocity = (decimal)neoData.SelectToken("close_approach_data[0].relative_velocity.kilometers_per_hour");
            var date = (DateTime)neoData.SelectToken("close_approach_data[0].close_approach_date");

            var neo = new Neo
            {
                Name = name,
                Diameter = (maxDiameter + minDiameter) / 2,
                Velocity = velocity,
                Date = date
            };

            return neo;
        }
    }
}
