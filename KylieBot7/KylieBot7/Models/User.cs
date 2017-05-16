using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KylieBot7.Models
{
    [Serializable]
    public class User
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string Token { get; set; }
        public bool WantsToBeAuthenticated { get; set; }
        public string AADEmail { get; set; }
        public string AADUsername { get; set; }
        public Guid existingChatID { get; set; }
        public Guid CRMContactId { get; set; }

    }
}