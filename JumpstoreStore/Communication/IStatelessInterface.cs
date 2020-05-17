using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Threading.Tasks;

namespace Communication
{
    public interface IStatelessInterface : IService
    {
        Task<string> GetServiceDetails();
    }
}
