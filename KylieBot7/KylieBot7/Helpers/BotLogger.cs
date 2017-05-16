using KylieBot7.Models;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace KylieBot7.Helpers
{
    public static class BotLogger
    {
        public static async Task Log(Activity activity)
        {
            var x = activity;
            BotChat chat = new BotChat(x.From.Name.ToString() + "(" + x.From.Id.ToString() + ")", x.Text, x.ChannelId.ToString(), x.Timestamp.Value);
            if (MessagesController.LocalUser.existingChatID != null)
            {
                chat.existingChatID = MessagesController.LocalUser.existingChatID;
            }
            if (MessagesController.LocalUser.CRMContactId != null)
            {
                chat.regardingId = MessagesController.LocalUser.CRMContactId;
            }

            HttpClient cons = new HttpClient();
            cons.BaseAddress = new Uri("");
            cons.DefaultRequestHeaders.Accept.Clear();
            cons.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            using (cons)
            {
                var content = new StringContent(JsonConvert.SerializeObject(chat), Encoding.UTF8, "application/json");
                HttpResponseMessage res = await cons.PostAsync("CRM/CreateBotChat", content);
                if (res.IsSuccessStatusCode)
                {
                    Tuple<bool, Guid> result = await res.Content.ReadAsAsync<Tuple<bool, Guid>>();
                    if (MessagesController.LocalUser.existingChatID == Guid.Empty && result.Item1 && result.Item2 != Guid.Empty)
                    {
                        MessagesController.LocalUser.existingChatID = result.Item2;
                    }
                }
            }
        }
    }
}