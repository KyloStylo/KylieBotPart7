using CRM_Api.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRM_Api.Helpers
{
    public class Dynamics365Helper
    {
        private Boolean isConnectionReady = false;
        public bool IsConnectionReady { get => isConnectionReady; set => isConnectionReady = value; }

        private static CrmServiceClient crmSvc;
        public static CrmServiceClient CrmSvc { get => crmSvc; set => crmSvc = value; }

        private string connectionError;
        public string ConnectionError { get => connectionError; set => connectionError = value; }
        public enum CRMRegions
        {
            NorthAmerica,
            EMEA,
            APAC,
            SouthAmerica,
            Oceania,
            JPN,
            CAN,
            IND,
            NorthAmerica2
        };

        public void ConnectCRM(string crmUsername, string crmPassword, string crmRegion, string crmOrgId, bool isO365)
        {
            try
            {
                CrmSvc = new CrmServiceClient(crmUsername, CrmServiceClient.MakeSecureString(crmPassword), crmRegion, crmOrgId, true, true, null, isO365);
                IsConnectionReady = CrmSvc.IsReady;
            }
            catch (Exception e)
            {
                ConnectionError = e.Message.ToString();
            }
        }

        public static EntityCollection RetrieveXML(string XML)
        {
            EntityCollection queryResult = null;

            if (CrmSvc != null && CrmSvc.IsReady && XML != null)
            {
                return queryResult = CrmSvc.GetEntityDataByFetchSearchEC(XML);
            }
            else
            {
                return queryResult;
            }
        }

        public static Tuple<bool, Guid> CreateCRMBotChat(BotChat transcript)
        {
            bool success = false;
            Guid chatId = new Guid();

            if (CrmSvc != null && CrmSvc.IsReady && transcript != null)
            {
                if (transcript.existingChatID == Guid.Empty)
                {
                    Dictionary<string, CrmDataTypeWrapper> inData = new Dictionary<string, CrmDataTypeWrapper>();
                    inData.Add("subject", new CrmDataTypeWrapper("Bot chat: " + DateTime.Now.ToString() + " - " + transcript.channel.ToString(), CrmFieldType.String));
                    if (transcript.regardingId != Guid.Empty)
                    {
                        inData.Add("regardingobjectid", new CrmDataTypeWrapper(transcript.regardingId, CrmFieldType.Lookup, "contact"));
                    }
                    inData.Add("kbot_transcript", new CrmDataTypeWrapper("From: " + transcript.chatUser + Environment.NewLine +
                                                                            "Message: " + transcript.chatMessage + Environment.NewLine +
                                                                            "Time: " + transcript.timeStamp + Environment.NewLine +
                                                                            "Channel: " + transcript.channel + Environment.NewLine + Environment.NewLine,
                                                                            CrmFieldType.String));

                    try
                    {
                        chatId = CrmSvc.CreateNewRecord("kbot_botchat", inData);
                        success = true;
                    }
                    catch (Exception e)
                    {
                        success = false;
                    }
                }
                else
                {
                    chatId = transcript.existingChatID;

                    Dictionary<string, object> data = CrmSvc.GetEntityDataById("kbot_botchat", chatId, new List<string> { "kbot_transcript", "regardingobjectid" });
                    Dictionary<string, CrmDataTypeWrapper> updateData = new Dictionary<string, CrmDataTypeWrapper>();
                    if (!data.ContainsKey("regardingobjectid"))
                    {
                        if (transcript.regardingId != Guid.Empty)
                        {
                            updateData.Add("regardingobjectid", new CrmDataTypeWrapper(transcript.regardingId, CrmFieldType.Lookup, "contact"));
                        }
                    }

                    foreach (var pair in data)
                    {
                        switch (pair.Key)
                        {
                            case "kbot_transcript":
                                string original = (string)pair.Value;
                                updateData.Add("kbot_transcript", new CrmDataTypeWrapper(original + "From: " + transcript.chatUser + Environment.NewLine +
                                                                            "Message: " + transcript.chatMessage + Environment.NewLine +
                                                                            "Time: " + transcript.timeStamp + Environment.NewLine +
                                                                            "Channel: " + transcript.channel + Environment.NewLine + Environment.NewLine,
                                                                            CrmFieldType.String));
                                break;
                            default:
                                break;
                        }
                    }
                    success = crmSvc.UpdateEntity("kbot_botchat", "activityid", chatId, updateData);
                }
            }
            return new Tuple<bool, Guid>(success, chatId);
        }
    }
}