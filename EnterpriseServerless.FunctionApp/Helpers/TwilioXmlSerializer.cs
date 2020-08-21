using EnterpriseServerless.FunctionApp.ResponseModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseServerless.FunctionApp.Helpers
{
    public class TwilioXmlSerializer
    {
        public static string XmlSerialize(Response response)
        {
            StringBuilder result = new StringBuilder();
            result.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            result.AppendLine("<Response>");

            if (response.Gather != null)
            {
                result.Append(XmlSerialize(response.Gather));
            }

            if (response.Saying != null)
            {
                result.AppendLine(XmlSerialize(response.Saying));
            }

            if (response.Play != null)
            {
                result.AppendLine(XmlSerialize(response.Play));
            }

            if (response.Dialing != null)
            {
                result.Append(XmlSerialize(response.Dialing));
            }

            if (response.Hangup != null)
            {
                result.AppendLine(XmlSerialize(response.Hangup));
            }

            if (response.Reject != null)
            {
                result.AppendLine(XmlSerialize(response.Reject));
            }

            result.AppendLine("</Response>");
            return result.ToString();
        }

        public static string XmlSerialize(TwilioGather gather)
        {
            StringBuilder result = new StringBuilder();
            result.AppendLine($"<Gather numDigits=\"1\" action=\"{gather.Action}\">");

            if (gather.Saying != null)
            {
                result.AppendLine(XmlSerialize(gather.Saying));
            }

            if (gather.Play != null)
            {
                result.AppendLine(XmlSerialize(gather.Play));
            }

            if (gather.Pause != null)
            {
                result.AppendLine(XmlSerialize(gather.Pause));
            }

            if (gather.Saying2 != null)
            {
                result.AppendLine(XmlSerialize(gather.Saying2));
            }

            if (gather.Play2 != null)
            {
                result.AppendLine(XmlSerialize(gather.Play2));
            }

            if (gather.Pause2 != null)
            {
                result.AppendLine(XmlSerialize(gather.Pause2));
            }

            if (gather.Saying3 != null)
            {
                result.AppendLine(XmlSerialize(gather.Saying3));
            }

            if (gather.Play3 != null)
            {
                result.AppendLine(XmlSerialize(gather.Play3));
            }

            if (gather.Pause3 != null)
            {
                result.AppendLine(XmlSerialize(gather.Pause3));
            }

            result.AppendLine("</Gather>");
            return result.ToString();
        }

        public static string XmlSerialize(TwilioSay twSay)
        {
            return $"<Say language=\"{twSay.Language}\">{twSay.Text}</Say>";
        }

        public static string XmlSerialize(TwilioPlay twPlay)
        {
            return $"<Play>{twPlay.Text}</Play>";
        }

        public static string XmlSerialize(TwilioPause twPause)
        {
            return $"<Pause length=\"{twPause.Length}\"/>";
        }

        public static string XmlSerialize(TwilioDial twDial)
        {
            StringBuilder result = new StringBuilder();
            result.Append("<Dial");

            if (!string.IsNullOrEmpty(twDial.Record))
            {
                result.Append($" record=\"{twDial.Record}\"");
            }

            if (!string.IsNullOrEmpty(twDial.RecordingStatusCallback))
            {
                result.Append($" recordingStatusCallback=\"{twDial.RecordingStatusCallback}\"");
            }

            if (!string.IsNullOrEmpty(twDial.Action))
            {
                result.Append($" action=\"{twDial.Action}\"");
            }

            if (!string.IsNullOrEmpty(twDial.TimeLimit))
            {
                result.Append($" timeLimit=\"{twDial.TimeLimit}\"");
            }

            if (!string.IsNullOrEmpty(twDial.Timeout))
            {
                result.Append($" timeout=\"{twDial.Timeout}\"");
            }

            result.Append(">");
            if (!string.IsNullOrEmpty(twDial.Text))
            {
                result.Append($"{twDial.Text}");
            }
            else if (twDial.Number != null)
            {
                result.Append(XmlSerialize(twDial.Number));
            }

            result.AppendLine("</Dial>");
            return result.ToString();
        }

        public static string XmlSerialize(TwilioNumber twNumber)
        {
            StringBuilder result = new StringBuilder();
            result.Append("<Number");

            if (!string.IsNullOrEmpty(twNumber.Method))
            {
                result.Append($" method=\"{twNumber.Method}\"");
            }

            if (!string.IsNullOrEmpty(twNumber.Url))
            {
                result.Append($" url=\"{twNumber.Url}\"");
            }

            result.Append(">");
            result.Append($"{twNumber.Text}");
            result.AppendLine("</Number>");
            return result.ToString();
        }

        public static string XmlSerialize(TwilioHangup twHangup)
        {
            return "<Hangup/>";
        }

        public static string XmlSerialize(TwilioReject twReject)
        {
            return "<Reject/>";
        }
    }
}
