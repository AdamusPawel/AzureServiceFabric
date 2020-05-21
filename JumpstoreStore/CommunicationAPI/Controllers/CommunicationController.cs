using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Communication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace CommunicationAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CommunicationController : ControllerBase
    {
        [HttpGet]
        [Route("stateless")]
        public async Task<string> StatelessGet()
        {
            var statelessProxy = ServiceProxy.Create<IStatelessInterface>(
                new Uri("fabric:/JumpstoreStore/CustomerAnalytics")); // ref to server's Uri

            var serviceName = await statelessProxy.GetServiceDetails();

            return serviceName;
        }

        [HttpGet]
        [Route("stateful")]
        public async Task<string> StatefulGet([FromQuery] int productId)
        {
            var partitionId = productId % 3; // it will cause productId to be split betweeen partitions
            var statefulProxy = ServiceProxy.Create<IStatefulInterface>(
                new Uri("fabric:/JumpstoreStore/ProductCatalogue"),
                new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(partitionId));

            var serviceName = await statefulProxy.GetServiceDetails();

            return serviceName;
        }

        [HttpPost] // we want  to post products
        [Route("addproduct")]
        public async Task AddProduct([FromBody] Product product)
        {
            var partitionId = product.Id % 3; // we will split added products over partitions
            var statefulProxy = ServiceProxy.Create<IStatefulInterface>(
                new Uri("fabric:/JumpstoreStore/ProductCatalogue"),
                new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(partitionId));

            await statefulProxy.AddProduct(product); // this is void return so we just need to pass product to be added
        }

        [HttpGet] // we want  to get products
        [Route("getproduct")]
        public async Task<Product> GetProduct([FromQuery] int productId)
        {
            var partitionId = productId % 3; // we generate partitionId in the same manner like in addproduct - so that guarantee we will be routed to correct partition
            var statefulProxy = ServiceProxy.Create<IStatefulInterface>(
                new Uri("fabric:/JumpstoreStore/ProductCatalogue"),
                new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(partitionId));

            var product = await statefulProxy.GetProductById(partitionId); // get product by id

            return product; // return that product
        }

        [HttpPost] // we want  to post products
        [Route("addtoqueue")]
        public async Task AddToQueue([FromQuery] int partitionId, [FromBody] Product product)
        {
            var statefulProxy = ServiceProxy.Create<IStatefulInterface>(
                new Uri("fabric:/JumpstoreStore/ProductCatalogue"),
                new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(partitionId));

            await statefulProxy.AddToQueue(product); // this is void return so we just need to pass product to be added
        }

        [HttpGet] // we want  to get products
        [Route("getfromqueue")]
        public async Task<Product> GetFromQueue([FromQuery] int partitionId)
        {
            var statefulProxy = ServiceProxy.Create<IStatefulInterface>(
                new Uri("fabric:/JumpstoreStore/ProductCatalogue"),
                new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(partitionId));

            var product = await statefulProxy.GetFromQueue(); // get product by id

            return product; // return that product
        }
    }
}
