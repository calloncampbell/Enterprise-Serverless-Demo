using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace EnterpriseServerless.FunctionApp.Abstractions.Interfaces
{
    public interface IPostCallService
    {
        Task<string> ProcessPostCallAsync(HttpRequest req);
    }
}
