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

        private IActorTimer _actorTimer;

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

        protected override Task OnPostActorMethodAsync(ActorMethodContext actorMethodContext)
        {
            ActorEventSource.Current.ActorMessage(actor: this, message: $"{actorMethodContext.MethodName} has finished.");

            return base.OnPostActorMethodAsync(actorMethodContext);
        }

        protected override Task OnPreActorMethodAsync(ActorMethodContext actorMethodContext)
        {
            ActorEventSource.Current.ActorMessage(actor: this, message: $"{actorMethodContext.MethodName} will start soon.");

            return base.OnPreActorMethodAsync(actorMethodContext);
        }

        protected override Task OnDeactivateAsync()
        {
            if (_actorTimer != null)
            {
                UnregisterTimer(_actorTimer); // safe way to unregister timer when actor is deactivated
            }

            ActorEventSource.Current.ActorMessage(actor: this, message: "Actor deactivated.");

            return base.OnDeactivateAsync();
        }

        protected override Task OnActivateAsync()
        {
            _actorTimer = RegisterTimer(DoWork, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(15)); // will trigger after 10 sec and then every 15 sec after that

            ActorEventSource.Current.ActorMessage(actor: this, message: "Actor activated.");

            return this.StateManager.TryAddStateAsync("count", value: 0);
        }

        private Task DoWork(object work) // callback for our timer
        {
            ActorEventSource.Current.ActorMessage(actor: this, message: $"Actor is doing work");

            return Task.CompletedTask;
        }
    }
}
