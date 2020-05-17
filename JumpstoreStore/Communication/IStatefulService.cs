using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Threading.Tasks;

namespace Communication
{
    public interface IStatefulService : IService
    {
        Task<string> GetServiceDetails();
    }
}
