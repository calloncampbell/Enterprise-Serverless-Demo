using EnterpriseServerless.FunctionApp.Abstractions.Models;
using System;
using System.Threading.Tasks;

namespace EnterpriseServerless.FunctionApp.Abstractions.Interfaces
{
    public interface ICallLoggingService
    {
        Task CreateCallLogAsync(CallLog item);
    }
}
