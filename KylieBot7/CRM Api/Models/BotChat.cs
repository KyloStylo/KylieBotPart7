using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRM_Api.Models
{
    public class BotChat
    {
        public string chatUser { get; set; }
        public string chatMessage { get; set; }
        public string channel { get; set; }
        public DateTime timeStamp { get; set; }
        public Guid regardingId { get; set; }
        public Guid existingChatID { get; set; }
        public BotChat(string user, string message, string comChannel, DateTime stamp)
        {
            chatUser = user;
            chatMessage = message;
            channel = comChannel;
            timeStamp = stamp;
        }
    }
}