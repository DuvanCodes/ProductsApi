using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using ProductsApi.Models;

namespace ProductsApi.Repositories;

public class ProductRepository(IConfiguration configuration) : IProductRepository
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    private SqlConnection CreateConnection() => new(_connectionString);

    public async Task<int> CreateAsync(string name, string? description, decimal price)
    {
        using var conn = CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@Name", name);
        parameters.Add("@Description", description);
        parameters.Add("@Price", price);
        parameters.Add("@NewId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await conn.ExecuteAsync("dbo.sp_Product_Create", parameters, commandType: CommandType.StoredProcedure);

        return parameters.Get<int>("@NewId");
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        using var conn = CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<Product>(
            "dbo.sp_Product_GetById",
            new { Id = id },
            commandType: CommandType.StoredProcedure);
    }

    // TODO: paginación cuando el listado crezca
    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        using var conn = CreateConnection();
        return await conn.QueryAsync<Product>(
            "dbo.sp_Product_GetAll",
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> UpdateAsync(int id, string name, string? description, decimal price)
    {
        using var conn = CreateConnection();
        var rows = await conn.ExecuteScalarAsync<int>(
            "dbo.sp_Product_Update",
            new { Id = id, Name = name, Description = description, Price = price },
            commandType: CommandType.StoredProcedure);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = CreateConnection();
        var rows = await conn.ExecuteScalarAsync<int>(
            "dbo.sp_Product_Delete",
            new { Id = id },
            commandType: CommandType.StoredProcedure);
        return rows > 0;
    }
}
