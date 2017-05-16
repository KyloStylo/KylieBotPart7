using CRM_Api.Helpers;
using CRM_Api.Models;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CRM_Api.Controllers
{
    public class CRMController : ApiController
    {
        List<CRMContact> contacts = new List<CRMContact>();
        Dynamics365Helper CRMConnection = new Dynamics365Helper();

        private void connectCRM()
        {
            CRMConnection.ConnectCRM(ConfigurationManager.AppSettings["D365.Username"], ConfigurationManager.AppSettings["D365.Password"], Dynamics365Helper.CRMRegions.NorthAmerica.ToString(), ConfigurationManager.AppSettings["D365.Uri"], true);
        }

        [Route("CRM/GetContact/{email}/")]
        [HttpGet]
        public CRMContact GetContact(string email)
        {
            if (CRMConnection != null)
            {
                connectCRM();
            }

            string fetchXML =
            @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' returntotalrecordcount='true' >
                            <entity name='contact'>
                                <attribute name='contactid' />
                                <attribute name='fullname' />
                                <attribute name='emailaddress1' />
                                <filter type = 'and'>
                                    <condition attribute = 'emailaddress1' value = " + email + @" operator = 'eq' />
                                </filter>
                            </entity>
                        </fetch>";

            EntityCollection collection = Dynamics365Helper.RetrieveXML(fetchXML);

            CRMContact contact = null;
            if (collection.Entities != null)
            {
                var entity = collection.Entities.FirstOrDefault();
                contact = new CRMContact();
                contact.Email = entity.Attributes["emailaddress1"].ToString();
                contact.FullName = entity.Attributes["fullname"].ToString();
                contact.ContactId = new Guid(entity.Attributes["contactid"].ToString());
            }

            return contact;
        }

        [Route("CRM/CreateBotChat")]
        [HttpPost]
        public Tuple<bool, Guid> CreateBotChat(JObject transcript)
        {
            BotChat chat;
            Tuple<bool, Guid> result = null;

            if (transcript != null)
            {
                chat = JsonConvert.DeserializeObject<BotChat>(transcript.ToString());

                if (CRMConnection != null)
                {
                    connectCRM();
                }

                if (CRMConnection.IsConnectionReady)
                {
                    try
                    {
                        result = Dynamics365Helper.CreateCRMBotChat(chat);
                    }
                    catch (Exception e)
                    {
                    }
                }
            }
            return result;
        }
    }
}
