namespace Asteroids.Application.Client
{
    public interface INeoClientSettings
    {
        string BaseUrl { get; }

        string ApiKey { get; }
    }
}
