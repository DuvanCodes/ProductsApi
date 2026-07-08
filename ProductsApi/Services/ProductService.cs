using ProductsApi.Dtos;
using ProductsApi.Models;
using ProductsApi.Repositories;

namespace ProductsApi.Services;

public class ProductService(IProductRepository repository) : IProductService
{
    public Task<IEnumerable<Product>> GetAllAsync() => repository.GetAllAsync();

    public Task<Product?> GetByIdAsync(int id) => repository.GetByIdAsync(id);

    public async Task<Product> CreateAsync(ProductCreateDto dto)
    {
        var id = await repository.CreateAsync(dto.Name, dto.Description, dto.Price);
        return (await repository.GetByIdAsync(id))!;
    }

    public Task<bool> UpdateAsync(int id, ProductUpdateDto dto) =>
        repository.UpdateAsync(id, dto.Name, dto.Description, dto.Price);

    public Task<bool> DeleteAsync(int id) => repository.DeleteAsync(id);
}
