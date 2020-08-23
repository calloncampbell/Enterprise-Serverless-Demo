using EnterpriseServerless.FunctionApp.ResponseModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseServerless.FunctionApp.Services
{
    public class CallResponseService
    {
        /// <summary>
        /// Hangups the response.
        /// </summary>
        /// <returns>Response.</returns>
        public static Response HangupResponse()
        {
            return new Response
            {
                Hangup = new TwilioHangup()
            };
        }

        /// <summary>
        /// Rejects the response.
        /// </summary>
        /// <returns>Response.</returns>
        public static Response RejectResponse()
        {
            return new Response
            {
                Reject = new TwilioReject()
            };
        }

        /// <summary>
        /// Nots the in service response.
        /// </summary>
        /// <returns>Response.</returns>
        public static Response NotInServiceResponse()
        {
            return new Response
            {
                Saying = new TwilioSay
                {
                    Loop = 1,
                    Text = "The number you are calling is not in service",
                    Voice = "alice"
                }
            };
        }

        /// <summary>
        /// Disconnects the call response.
        /// </summary>
        /// <returns>Response.</returns>
        public static Response DisconnectCallResponse()
        {
            return new Response
            {
                Saying = new TwilioSay { Text = "Sorry we cannot complete your call due to a system error", Voice = "alice" }
            };
        }

        /// <summary>
        /// Says the response.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>Response.</returns>
        public static Response SayResponse(string text)
        {
            return new Response
            {
                Saying = new TwilioSay { Text = text, Voice = "alice" }
            };
        }
    }
}
