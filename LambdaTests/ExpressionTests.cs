using LinqKit;
using System.Linq.Expressions;
using System.Security.Cryptography;

namespace LambdaTests;

[TestClass]
public class ExpressionTests
{

    // Sample data
    private static List<Product> products =
    [
        new Product { Id = 1, Name = "Product 1", Price = 10, InStock = true },
        new Product { Id = 2, Name = "Product 2", Price = 20, InStock = false },
        new Product { Id = 3, Name = "Product 3", Price = 30, InStock = true },
    ];

    [TestMethod]
    public void TestMethod1()
    {
        // Search criteria
        string searchName = "Product";
        decimal? minPrice = 15;
        decimal? maxPrice = 25;
        bool? inStock = true;
        var predicate = PredicateBuilder.New<Product>();
    }

    [TestMethod]
    public void TestExpress()
    {
        var products = new List<Product>
        {
            new Product{  Id=1,Name="Product 1",Price=10,InStock=true},
            new Product { Id = 2, Name = "Product 2", Price = 20, InStock = false },
            new Product { Id = 3, Name = "Product 3", Price = 30, InStock = true },
        };

        var parameter = Expression.Parameter(typeof(Product));

        var property = Expression.Property(parameter, "Price");

        var constant = Expression.Constant(15m);

        var comparison = Expression.GreaterThan(property, constant);

        var lambda = Expression.Lambda<Func<Product, bool>>(comparison, parameter);

        var filterProducts = products.AsQueryable().Where(lambda);

        foreach (var product in filterProducts)
        {
            Console.WriteLine($"{product.Name} - {product.Price}");
        }
    }


    [TestMethod]
    public void CreateToken()
    {
        Guid id = Guid.NewGuid();
        Console.WriteLine(id.ToString());
    }

    [TestMethod]
    public void CreateASEToken()
    {
        Guid guid = Guid.NewGuid();

        byte[] bytes = guid.ToByteArray();

        using (SHA256 sha = SHA256.Create())
        {
            byte[] hashBytes = sha.ComputeHash(bytes);

            string token = Convert.ToBase64String(hashBytes);

            // Optional: Remove any special characters to make the token URL-safe
            token = token.Replace('+', '-').Replace('/', '_').Replace("=", "");
            Console.WriteLine(token);
        }
    }
}


internal class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public bool InStock { get; set; }
}