using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Threading.Tasks;

namespace Communication
{
    public interface IStatefulInterface : IService
    {
        Task<string> GetServiceDetails();

        Task<Product> GetProductById(int id);
        Task AddProduct(Product product);

        Task<Product> GetFromQueue();
        Task AddToQueue(Product product);
    }
}
