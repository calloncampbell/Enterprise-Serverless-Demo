using EnterpriseServerless.FunctionApp.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EnterpriseServerless.FunctionApp.Abstractions.Interfaces
{
    public interface ICallLoggingService
    {
        Task CreateCallLogAsync(CallLog item);
        Task UpdateCallLogAsync(string callSid);
        Task<ICollection<CallLog>> GetCallLogsAsync();
        Task DeleteCallLogAsync(string callSid);
    }
}
