using Microsoft.Extensions.Configuration;

namespace Asteroids.Application.Client
{
    public class NeoClientSettings : INeoClientSettings
    {
        public NeoClientSettings(IConfiguration configuration)
        {
            BaseUrl = configuration.GetSection("ClientApi:NeoClient:BaseUrl").Value ?? throw new ArgumentNullException("BaseUrl is not defined.", nameof(BaseUrl));
            ApiKey = configuration.GetSection("ClientApi:NeoClient:ApiKey").Value ?? throw new ArgumentNullException("ApiKey is not defined.", nameof(ApiKey));
        }

        public string BaseUrl { get; set; }

        public string ApiKey { get; set; }
    }
}
