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
                new Uri("fabric:/JumpstoreStore/CustomerAnalytics"));

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
    }
}
