using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Communication;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace ProductCatalogue
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class ProductCatalogue : StatefulService, IStatefulInterface
    {
        public ProductCatalogue(StatefulServiceContext context)
            : base(context)
        { }

        public async Task<Product> GetFromQueue()
        {
            var stateManager = this.StateManager;
            var productQueue = await stateManager.GetOrAddAsync<IReliableQueue<Product>>("productqueue");

            using (var transaction = stateManager.CreateTransaction())
            {
                var product = await productQueue.TryDequeueAsync(transaction);  // try to get value of product from reliable queue

                await transaction.CommitAsync(); // complete the transaction + remove prod from transaction's queue

                return product.Value; // return value of product
            }

            throw new ArgumentException(); // if fail, throw exception
        }

        public async Task AddToQueue(Product product)
        {
            var stateManager = this.StateManager;
            var productQueue = await stateManager.GetOrAddAsync<IReliableQueue<Product>>("productqueue");

            using (var transaction = stateManager.CreateTransaction())
            {
                await productQueue.EnqueueAsync(transaction, product);  // store product in reliable queue

                await transaction.CommitAsync(); // complete the transaction
            }
        }

        public async Task<Product> GetProductById(int id)
        {
            var stateManager = this.StateManager; // StateManager is Azure Fabric thingy, allows us to manage state
            var productDict = await stateManager.GetOrAddAsync<IReliableDictionary<int, Product>>("productdict"); // - name (unique!) is for reliable dictionary which we will be called in State Manager

            using (var transaction = stateManager.CreateTransaction())
            {
                var product = await productDict.TryGetValueAsync(transaction, id);  // try to get value of product from reliable dictionary

                return product.Value; // return value of product
            }

            throw new Exception(); // if fail, throw exception
        }

        public async Task AddProduct(Product product)
        {
            var stateManager = this.StateManager; // StateManager is Azure Fabric thingy, allows us to manage state
            var productDict = await stateManager.GetOrAddAsync<IReliableDictionary<int, Product>>("productdict"); // - name (unique!) is for reliable dictionary which we will be called in State Manager

            using (var transaction = stateManager.CreateTransaction())
            {
                await productDict.AddOrUpdateAsync(transaction, product.Id, product, (key, value) => value); // transaction of adding/updating product

                await transaction.CommitAsync(); // complete the transaction
            }
        }

        public async Task<string> GetServiceDetails()
        {
            var serviceName = this.Context.ServiceName.ToString();
            var partition = this.Context.PartitionId.ToString();

            return $"{serviceName} --- {partition}";
        }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("myDictionary");

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using (var tx = this.StateManager.CreateTransaction())
                {
                    var result = await myDictionary.TryGetValueAsync(tx, "Counter");

                    ServiceEventSource.Current.ServiceMessage(this.Context, "Current Counter Value: {0}",
                        result.HasValue ? result.Value.ToString() : "Value does not exist.");

                    await myDictionary.AddOrUpdateAsync(tx, "Counter", 0, (key, value) => ++value);

                    // If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
                    // discarded, and nothing is saved to the secondary replicas.
                    await tx.CommitAsync();
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
