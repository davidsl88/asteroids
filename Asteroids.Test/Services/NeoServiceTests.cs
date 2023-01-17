using Asteroids.Application.Client;
using Microsoft.AspNetCore.WebUtilities;
using Moq;
using RichardSzalay.MockHttp;

namespace Asteroids.Test.Services
{
    [TestFixture]
    public class NeoServiceTests
    {
        private string BaseUrl;

        private string ApiKey;

        private int Days;

        // Without this empty constructor, the tests fail. Looks like it is a problem with NUnit
        public NeoServiceTests()
        {
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            BaseUrl = "https://api.nasa.gov/neo/rest/v1/feed";
            ApiKey = "DEMO_KEY";
            Days = 0;
        }

        [Test]
        public async Task GetNeoListAsync_ShouldSucceed()
        {
            var startDate = DateTime.Today.ToString("yyyy-MM-dd");
            var endDate = DateTime.Today.AddDays(Days).ToString("yyyy-MM-dd");

            var endpointUrl = BuildEndpointUrl(BaseUrl, ApiKey, startDate, endDate);

            var mockHttp = new MockHttpMessageHandler();

            var request = CreateRequest(mockHttp, endpointUrl, startDate);

            var neos = await GetNeoList(mockHttp);

            Assert.Multiple(() =>
            {
                Assert.That(mockHttp.GetMatchCount(request), Is.EqualTo(1));
                Assert.That(neos, Has.Count.EqualTo(3));
                Assert.That(neos.First().Name, Is.EqualTo("495615 (2015 PQ291)"));
                Assert.That(neos.Last().Name, Is.EqualTo("(2016 WY7)"));
            });
        }

        [Test]
        public async Task GetNeoListAsync_ShouldReturnEmptyListWhenDatesDoNotMatch()
        {
            var startDate = DateTime.Today.ToString("yyyy-MM-dd");
            var endDate = DateTime.Today.AddDays(Days).ToString("yyyy-MM-dd");

            var endpointUrl = BuildEndpointUrl(BaseUrl, ApiKey, startDate, endDate);

            var mockHttp = new MockHttpMessageHandler();

            var request = CreateRequest(mockHttp, endpointUrl);

            var neos = await GetNeoList(mockHttp);

            Assert.Multiple(() =>
            {
                Assert.That(mockHttp.GetMatchCount(request), Is.EqualTo(1));
                Assert.That(neos, Is.Empty);
            });
        }

        [Test]
        public async Task GetNeoListAsync_ShouldReturnEmptyListWhenResponseIsNotSuccessful()
        {
            var startDate = DateTime.Today.ToString("yyyy-MM-dd");
            var endDate = DateTime.Today.AddDays(Days).ToString("yyyy-MM-dd");

            var endpointUrl = BuildEndpointUrl(BaseUrl, ApiKey, startDate, endDate);

            var mockHttp = new MockHttpMessageHandler();

            var request = mockHttp
                .When(endpointUrl)
                .Respond(System.Net.HttpStatusCode.NotFound, "application/json", "");

            var neos = await GetNeoList(mockHttp);
            
            Assert.Multiple(() =>
            {
                Assert.That(mockHttp.GetMatchCount(request), Is.EqualTo(1));
                Assert.That(neos, Is.Empty);
            });
        }

        private async Task<List<Domain.Entities.Neo>> GetNeoList(MockHttpMessageHandler mockHttp)
        {
            var _neoClientSettings = CreateNeoClientSettings(BaseUrl, ApiKey);
            var client = mockHttp.ToHttpClient();

            var _neoService = new NeoService(client, _neoClientSettings.Object);

            return await _neoService.GetNeoListAsync(Days);
        }

        private static string BuildEndpointUrl(string baseUrl, string apiKey, string startDate, string endDate)
        {
            var parameters = new Dictionary<string, string>
            {
                { "start_date", startDate },
                { "end_date", endDate },
                { "api_key", apiKey }
            };

            return QueryHelpers.AddQueryString(baseUrl, parameters);
        }

        private static Mock<INeoClientSettings> CreateNeoClientSettings(string baseUrl, string apiKey)
        {
            var _neoClientSettings = new Mock<INeoClientSettings>();

            _neoClientSettings.Setup(p => p.BaseUrl).Returns(baseUrl);
            _neoClientSettings.Setup(p => p.ApiKey).Returns(apiKey);

            return _neoClientSettings;
        }

        private static MockedRequest CreateRequest(MockHttpMessageHandler mockHttp, string endpointUrl, string? startDate = null)
        {
            startDate ??= "2022-11-16";

            var jsonResponse = "{\r\n  \"links\": {\r\n\"next\": \"http://api.nasa.gov/neo/rest/v1/feed?start_date=2022-11-17&end_date=2022-11-17&detailed=false&api_key=zdUP8ElJv1cehFM0rsZVSQN7uBVxlDnu4diHlLSb\",\r\n    \"prev\": \"http://api.nasa.gov/neo/rest/v1/feed?start_date=2022-11-15&end_date=2022-11-15&detailed=false&api_key=zdUP8ElJv1cehFM0rsZVSQN7uBVxlDnu4diHlLSb\",\r\n    \"self\": \"http://api.nasa.gov/neo/rest/v1/feed?start_date=2022-11-16&end_date=2022-11-16&detailed=false&api_key=zdUP8ElJv1cehFM0rsZVSQN7uBVxlDnu4diHlLSb\"\r\n  },\r\n  \"element_count\": 5,\r\n  \"near_earth_objects\": {\r\n    \"" + startDate + "\": [\r\n      {\r\n        \"links\": {\r\n          \"self\": \"http://api.nasa.gov/neo/rest/v1/neo/2495615?api_key=zdUP8ElJv1cehFM0rsZVSQN7uBVxlDnu4diHlLSb\"\r\n        },\r\n        \"id\": \"2495615\",\r\n        \"neo_reference_id\": \"2495615\",\r\n        \"name\": \"495615 (2015 PQ291)\",\r\n        \"nasa_jpl_url\": \"http://ssd.jpl.nasa.gov/sbdb.cgi?sstr=2495615\",\r\n        \"absolute_magnitude_h\": 17.7,\r\n        \"estimated_diameter\": {\r\n          \"kilometers\": {\r\n            \"estimated_diameter_min\": 0.7665755735,\r\n            \"estimated_diameter_max\": 1.7141150923\r\n          },\r\n          \"meters\": {\r\n            \"estimated_diameter_min\": 766.5755735311,\r\n            \"estimated_diameter_max\": 1714.1150923063\r\n          },\r\n          \"miles\": {\r\n            \"estimated_diameter_min\": 0.4763278307,\r\n            \"estimated_diameter_max\": 1.065101409\r\n          },\r\n          \"feet\": {\r\n            \"estimated_diameter_min\": 2515.0118046636,\r\n            \"estimated_diameter_max\": 5623.7373594423\r\n          }\r\n        },\r\n        \"is_potentially_hazardous_asteroid\": false,\r\n        \"close_approach_data\": [\r\n          {\r\n            \"close_approach_date\": \"2022-11-16\",\r\n            \"close_approach_date_full\": \"2022-Nov-16 04:52\",\r\n            \"epoch_date_close_approach\": 1668574320000,\r\n            \"relative_velocity\": {\r\n              \"kilometers_per_second\": \"21.5720053013\",\r\n              \"kilometers_per_hour\": \"77659.2190847498\",\r\n              \"miles_per_hour\": \"48254.4186585214\"\r\n            },\r\n            \"miss_distance\": {\r\n              \"astronomical\": \"0.2310777035\",\r\n              \"lunar\": \"89.8892266615\",\r\n              \"kilometers\": \"34568732.248091545\",\r\n              \"miles\": \"21480014.177471521\"\r\n            },\r\n            \"orbiting_body\": \"Earth\"\r\n          }\r\n        ],\r\n        \"is_sentry_object\": false\r\n      },\r\n      {\r\n        \"links\": {\r\n          \"self\": \"http://api.nasa.gov/neo/rest/v1/neo/3578826?api_key=zdUP8ElJv1cehFM0rsZVSQN7uBVxlDnu4diHlLSb\"\r\n        },\r\n        \"id\": \"3578826\",\r\n        \"neo_reference_id\": \"3578826\",\r\n        \"name\": \"(2011 SE16)\",\r\n        \"nasa_jpl_url\": \"http://ssd.jpl.nasa.gov/sbdb.cgi?sstr=3578826\",\r\n        \"absolute_magnitude_h\": 24.7,\r\n        \"estimated_diameter\": {\r\n          \"kilometers\": {\r\n            \"estimated_diameter_min\": 0.0305179233,\r\n            \"estimated_diameter_max\": 0.0682401509\r\n          },\r\n          \"meters\": {\r\n            \"estimated_diameter_min\": 30.5179232594,\r\n            \"estimated_diameter_max\": 68.2401509401\r\n          },\r\n          \"miles\": {\r\n            \"estimated_diameter_min\": 0.0189629525,\r\n            \"estimated_diameter_max\": 0.0424024508\r\n          },\r\n          \"feet\": {\r\n            \"estimated_diameter_min\": 100.1244233463,\r\n            \"estimated_diameter_max\": 223.8850168104\r\n          }\r\n        },\r\n        \"is_potentially_hazardous_asteroid\": false,\r\n        \"close_approach_data\": [\r\n          {\r\n            \"close_approach_date\": \"2022-11-16\",\r\n            \"close_approach_date_full\": \"2022-Nov-16 19:36\",\r\n            \"epoch_date_close_approach\": 1668627360000,\r\n            \"relative_velocity\": {\r\n              \"kilometers_per_second\": \"13.2722797582\",\r\n              \"kilometers_per_hour\": \"47780.2071293677\",\r\n              \"miles_per_hour\": \"29688.762591023\"\r\n            },\r\n            \"miss_distance\": {\r\n              \"astronomical\": \"0.4091867731\",\r\n              \"lunar\": \"159.1736547359\",\r\n              \"kilometers\": \"61213469.687933297\",\r\n              \"miles\": \"38036286.2980496186\"\r\n            },\r\n            \"orbiting_body\": \"Earth\"\r\n          }\r\n        ],\r\n        \"is_sentry_object\": false\r\n      },\r\n      {\r\n        \"links\": {\r\n          \"self\": \"http://api.nasa.gov/neo/rest/v1/neo/3618025?api_key=zdUP8ElJv1cehFM0rsZVSQN7uBVxlDnu4diHlLSb\"\r\n        },\r\n        \"id\": \"3618025\",\r\n        \"neo_reference_id\": \"3618025\",\r\n        \"name\": \"(2012 VF82)\",\r\n        \"nasa_jpl_url\": \"http://ssd.jpl.nasa.gov/sbdb.cgi?sstr=3618025\",\r\n        \"absolute_magnitude_h\": 20.8,\r\n        \"estimated_diameter\": {\r\n          \"kilometers\": {\r\n            \"estimated_diameter_min\": 0.1838886721,\r\n            \"estimated_diameter_max\": 0.411187571\r\n          },\r\n          \"meters\": {\r\n            \"estimated_diameter_min\": 183.8886720703,\r\n            \"estimated_diameter_max\": 411.1875710413\r\n          },\r\n          \"miles\": {\r\n            \"estimated_diameter_min\": 0.1142630881,\r\n            \"estimated_diameter_max\": 0.2555000322\r\n          },\r\n          \"feet\": {\r\n            \"estimated_diameter_min\": 603.309310875,\r\n            \"estimated_diameter_max\": 1349.040630575\r\n          }\r\n        },\r\n        \"is_potentially_hazardous_asteroid\": false,\r\n        \"close_approach_data\": [\r\n          {\r\n            \"close_approach_date\": \"2022-11-16\",\r\n            \"close_approach_date_full\": \"2022-Nov-16 07:58\",\r\n            \"epoch_date_close_approach\": 1668585480000,\r\n            \"relative_velocity\": {\r\n              \"kilometers_per_second\": \"14.176045679\",\r\n              \"kilometers_per_hour\": \"51033.7644442416\",\r\n              \"miles_per_hour\": \"31710.3965792567\"\r\n            },\r\n            \"miss_distance\": {\r\n              \"astronomical\": \"0.3320707516\",\r\n              \"lunar\": \"129.1755223724\",\r\n              \"kilometers\": \"49677077.128659092\",\r\n              \"miles\": \"30867904.3640037896\"\r\n            },\r\n            \"orbiting_body\": \"Earth\"\r\n          }\r\n        ],\r\n        \"is_sentry_object\": false\r\n      },\r\n      {\r\n        \"links\": {\r\n          \"self\": \"http://api.nasa.gov/neo/rest/v1/neo/3764822?api_key=zdUP8ElJv1cehFM0rsZVSQN7uBVxlDnu4diHlLSb\"\r\n        },\r\n        \"id\": \"3764822\",\r\n        \"neo_reference_id\": \"3764822\",\r\n        \"name\": \"(2016 WY7)\",\r\n        \"nasa_jpl_url\": \"http://ssd.jpl.nasa.gov/sbdb.cgi?sstr=3764822\",\r\n        \"absolute_magnitude_h\": 22.77,\r\n        \"estimated_diameter\": {\r\n          \"kilometers\": {\r\n            \"estimated_diameter_min\": 0.0742258153,\r\n            \"estimated_diameter_max\": 0.1659739687\r\n          },\r\n          \"meters\": {\r\n            \"estimated_diameter_min\": 74.2258153001,\r\n            \"estimated_diameter_max\": 165.9739686963\r\n          },\r\n          \"miles\": {\r\n            \"estimated_diameter_min\": 0.0461217691,\r\n            \"estimated_diameter_max\": 0.1031314109\r\n          },\r\n          \"feet\": {\r\n            \"estimated_diameter_min\": 243.5230238691,\r\n            \"estimated_diameter_max\": 544.5340354577\r\n          }\r\n        },\r\n        \"is_potentially_hazardous_asteroid\": false,\r\n        \"close_approach_data\": [\r\n          {\r\n            \"close_approach_date\": \"2022-11-16\",\r\n            \"close_approach_date_full\": \"2022-Nov-16 20:36\",\r\n            \"epoch_date_close_approach\": 1668630960000,\r\n            \"relative_velocity\": {\r\n              \"kilometers_per_second\": \"17.739438632\",\r\n              \"kilometers_per_hour\": \"63861.979075168\",\r\n              \"miles_per_hour\": \"39681.3502759009\"\r\n            },\r\n            \"miss_distance\": {\r\n              \"astronomical\": \"0.3266975359\",\r\n              \"lunar\": \"127.0853414651\",\r\n              \"kilometers\": \"48873255.504888533\",\r\n              \"miles\": \"30368432.7677984354\"\r\n            },\r\n            \"orbiting_body\": \"Earth\"\r\n          }\r\n        ],\r\n        \"is_sentry_object\": false\r\n      },\r\n      {\r\n        \"links\": {\r\n          \"self\": \"http://api.nasa.gov/neo/rest/v1/neo/3805274?api_key=zdUP8ElJv1cehFM0rsZVSQN7uBVxlDnu4diHlLSb\"\r\n        },\r\n        \"id\": \"3805274\",\r\n        \"neo_reference_id\": \"3805274\",\r\n        \"name\": \"(2018 HC1)\",\r\n        \"nasa_jpl_url\": \"http://ssd.jpl.nasa.gov/sbdb.cgi?sstr=3805274\",\r\n        \"absolute_magnitude_h\": 26.7,\r\n        \"estimated_diameter\": {\r\n          \"kilometers\": {\r\n            \"estimated_diameter_min\": 0.0121494041,\r\n            \"estimated_diameter_max\": 0.0271668934\r\n          },\r\n          \"meters\": {\r\n            \"estimated_diameter_min\": 12.14940408,\r\n            \"estimated_diameter_max\": 27.1668934089\r\n          },\r\n          \"miles\": {\r\n            \"estimated_diameter_min\": 0.0075492874,\r\n            \"estimated_diameter_max\": 0.0168807197\r\n          },\r\n          \"feet\": {\r\n            \"estimated_diameter_min\": 39.8602508817,\r\n            \"estimated_diameter_max\": 89.1302305717\r\n          }\r\n        },\r\n        \"is_potentially_hazardous_asteroid\": false,\r\n        \"close_approach_data\": [\r\n          {\r\n            \"close_approach_date\": \"2022-11-16\",\r\n            \"close_approach_date_full\": \"2022-Nov-16 11:26\",\r\n            \"epoch_date_close_approach\": 1668597960000,\r\n            \"relative_velocity\": {\r\n              \"kilometers_per_second\": \"22.4174418496\",\r\n              \"kilometers_per_hour\": \"80702.790658636\",\r\n              \"miles_per_hour\": \"50145.5756734174\"\r\n            },\r\n            \"miss_distance\": {\r\n              \"astronomical\": \"0.2615288186\",\r\n              \"lunar\": \"101.7347104354\",\r\n              \"kilometers\": \"39124154.206176382\",\r\n              \"miles\": \"24310622.1251907916\"\r\n            },\r\n            \"orbiting_body\": \"Earth\"\r\n          }\r\n        ],\r\n        \"is_sentry_object\": false\r\n      }\r\n    ]\r\n  }\r\n}";

            var request = mockHttp
                .When(endpointUrl)
                .Respond("application/json", jsonResponse);

            return request;
        }
    }
}