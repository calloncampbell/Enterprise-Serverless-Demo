using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace EnterpriseServerless.FunctionApp.Abstractions.Interfaces
{
    public interface IMediaFileService
    {
        Task<IActionResult> GetMediaFileAsync(IQueryCollection query);
    }
}
