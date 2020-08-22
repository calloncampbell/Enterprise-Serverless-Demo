using EnterpriseServerless.FunctionApp.Abstractions.Interfaces;
using EnterpriseServerless.FunctionApp.ResponseModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseServerless.FunctionApp.Services
{
	public class PostCallService : IPostCallService
	{
		private readonly ILogger<PostCallService> _logger;
		private readonly IConfigurationRoot _configuration;
		private readonly ICallLoggingService _callLoggingService;

		public PostCallService(
			ILogger<PostCallService> log,
			IConfigurationRoot configuration,
			ICallLoggingService callLoggingService)
		{
			_logger = log;
			_configuration = configuration;
			_callLoggingService = callLoggingService;
		}

		public async Task<string> ProcessPostCallAsync(HttpRequest req)
		{
			string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
			if (string.IsNullOrEmpty(requestBody))
			{
				_logger.LogWarning($"ProcessPostCallAsync: Warning - request body is null/empty");
				return string.Empty;
			}

			_logger.LogInformation($"ProcessPostCallAsync: Request body: {requestBody}");

			
			var cdrMessage = await CreateCdrMessageAsync($"ts={DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}&{requestBody}");
			if (cdrMessage == null)
			{
				_logger.LogError($"ProcessPostCallAsync: Error - cdrMessage is null");
				return string.Empty;
			}

			return JsonConvert.SerializeObject(cdrMessage);
		}

		private async Task<TwilioCall> CreateCdrMessageAsync(string data)
		{
			try
			{
				var tc = ParseEndCallParameters(data);
				if (tc == null)
				{
					_logger.LogDebug("CreateCdrMessageAsync: ParseEndCallParameters - invalid data");
					return null;
				}

				// Here is where you would connect to the Twilio API to pull down the inbound and outbound call detail record.

				await _callLoggingService.UpdateCallLogAsync(tc.CallSid);

				return tc;
			}
			catch (Exception ex)
            {
				_logger.LogError(ex, $"CreateCdrMessageAsync exception: {ex.Message}, {ex.InnerException?.Message}");
				throw;
			}
		}

		private TwilioCall ParseEndCallParameters(string data)
		{
			string[] items = data.Split('&');
			if (items.Length < 4)
			{
				_logger.LogError("TwilioController.ParseEndCallParameters: no parameter");
				return null;
			}

			var ret = new TwilioCall();
			if (items[0].StartsWith("ts"))
			{
				ret.EndTime = Convert.ToDateTime(items[0].Substring(3));
			}
			
			int found = 0;

			foreach (string item in items)
			{
				if (item.StartsWith("CallSid="))
				{
					ret.CallSid = item.Substring(8);
					found += 1;
				}

				if (item.StartsWith("DialCallSid="))
				{
					ret.DialCallSid = item.Substring(12);
					found += 1;
				}

				else if (item.StartsWith("RecordingSid="))
				{
					ret.RecordingSid = item.Substring(13);
					found += 1;
				}

				else if (item.StartsWith("RecordingUrl="))
				{
					ret.RecordingUrl = item.Substring(13);
					found += 1;
				}

				if (found == 4)
				{
					break;
				}
			}

			if (!ret.StartTime.HasValue)
			{
				ret.StartTime = DateTime.UtcNow;
			}
			return ret;
		}
	}
}
