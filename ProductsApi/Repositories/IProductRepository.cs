using ProductsApi.Models;

namespace ProductsApi.Repositories;

public interface IProductRepository
{
    Task<int> CreateAsync(string name, string? description, decimal price);
    Task<Product?> GetByIdAsync(int id);
    Task<IEnumerable<Product>> GetAllAsync();
    Task<bool> UpdateAsync(int id, string name, string? description, decimal price);
    Task<bool> DeleteAsync(int id);
}
