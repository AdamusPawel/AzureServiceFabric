using Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using ProductActorService.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProductAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : Controller
    {
        [HttpGet]
        public async Task<Product> GetProductById([FromQuery] int id)
        {
            var actorId = new ActorId(id);
            var proxy = ActorProxy.Create<IProductActorService>(actorId, new Uri("fabric:/ProductActorAplication/ProductActorServiceActorService"));
            var product = await proxy.GetProductAsync(new CancellationToken());

            return product;
        }

        [HttpPost]
        public async Task AddProduct([FromBody] Product product)
        {
            var actorId = new ActorId(product.Id);
            var proxy = ActorProxy.Create<IProductActorService>(actorId, new Uri("fabric:/ProductActorAplication/ProductActorServiceActorService"));
            await proxy.AddProductAsync(product, new CancellationToken());
        }

        [HttpDelete]
        public async Task DeleteActorById([FromQuery] int id)
        {
            var actorId = new ActorId(id); // we're creating new Actor's Id

            var actorServiceProxy = ActorServiceProxy.Create(new Uri("fabric:/ProductActorAplication/ProductActorServiceActorService"), actorId); // we're connecting to actor service, not actor itself

            await actorServiceProxy.DeleteActorAsync(actorId, new CancellationToken()); // allows us to delete actor and state

            // note: common use of this is when for example service proxy is on timer (e.g. one a day/week)
            // so we iterate through all actors and we delete the ones which meets conditions of being certain time old
        }
    }
}
