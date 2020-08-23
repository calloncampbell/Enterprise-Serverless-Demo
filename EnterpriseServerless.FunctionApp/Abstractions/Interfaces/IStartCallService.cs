using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseServerless.FunctionApp.Abstractions.Interfaces
{
    public interface IStartCallService
    {
        void SetHostUrl(string requestHost, bool isHttps);
        Task<string> GetNumberDetailsAsync(string requestBody);
    }
}
