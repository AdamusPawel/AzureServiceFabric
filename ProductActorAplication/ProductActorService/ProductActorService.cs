using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using ProductActorService.Interfaces;
using Contracts;

namespace ProductActorService
{
    [StatePersistence(StatePersistence.Persisted)]
    internal class ProductActorService : Actor, IProductActorService
    {
        private string ProductStateName = "ProductState";
        public ProductActorService(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        public async Task AddProductAsync(Product product, CancellationToken cancellationToken)
        {
            await this.StateManager.AddOrUpdateStateAsync(ProductStateName, product, updateValueFactory:(key, value) => product, cancellationToken);

            await this.StateManager.SaveStateAsync(cancellationToken);
        }

        public async Task<Product> GetProductAsync(CancellationToken cancellationToken)
        {
            var product = await this.StateManager.GetStateAsync<Product>(ProductStateName, cancellationToken);

            return product;
        }

        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(actor: this, message: "Actor activated.");

            return this.StateManager.TryAddStateAsync("count", value: 0);
        }
    }
}
