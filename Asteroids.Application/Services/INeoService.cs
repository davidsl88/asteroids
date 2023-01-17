using Asteroids.Domain.Entities;

namespace Asteroids.Application.Services
{
    public interface INeoService
    {
        Task<List<Neo>> GetNeoListAsync(int days);
    }
}
