using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Collections.Generic;
using KylieBot7.Helpers;
using KylieBot7.Dialogs;
using KylieBot7.Models;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Autofac;
using System;
using System.Linq;

namespace KylieBot7
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        static MessagesController()
        {
            LocalUser = new Models.User();
            MessageCount = 0;
        }

        private static Models.User localUser;
        public static Models.User LocalUser { get => localUser; set => localUser = value; }

        private static int messageCount;
        public static int MessageCount { get => messageCount; set => messageCount = value; }

        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            MessageCount++;
            if (MessageCount > 1)
            {
                await BotLogger.Log(activity);
            }

            if (activity.Type == ActivityTypes.Message)
            {
                await Conversation.SendAsync(activity, () => new RootDialog());
            }
            else
            {
                await HandleSystemMessageAsync(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private async Task<Activity> HandleSystemMessageAsync(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData) { }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));
                List<User> memberList = new List<User>();

                using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, message))
                {
                    var client = scope.Resolve<IConnectorClient>();
                    var activityMembers = await client.Conversations.GetConversationMembersAsync(message.Conversation.Id);

                    foreach (var member in activityMembers)
                    {
                        memberList.Add(new User() { Id = member.Id, Name = member.Name });
                    }

                    if (message.MembersAdded != null && message.MembersAdded.Any(o => o.Id == message.Recipient.Id))
                    {
                        LocalUser.Id = message.From.Id;
                        LocalUser.Name = message.From.Name;
                        var intro = message.CreateReply("Hello **" + message.From.Name + "**! I am **Kylie Bot (KB)**. \n\n What can I assist you with?");
                        await connector.Conversations.ReplyToActivityAsync(intro);
                    }
                }

                if (message.MembersAdded != null && message.MembersAdded.Any() && memberList.Count > 2)
                {
                    var added = message.CreateReply(message.MembersAdded[0].Name + " joined the conversation");
                    await connector.Conversations.ReplyToActivityAsync(added);
                }

                if (message.MembersRemoved != null && message.MembersRemoved.Any())
                {
                    var removed = message.CreateReply(message.MembersRemoved[0].Name + " left the conversation");
                    await connector.Conversations.ReplyToActivityAsync(removed);
                }
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate) { }
            else if (message.Type == ActivityTypes.Typing) { }
            else if (message.Type == ActivityTypes.Ping)
            {
                Activity reply = message.CreateReply();
                reply.Type = ActivityTypes.Ping;
                return reply;
            }
            return null;
        }
    }
}