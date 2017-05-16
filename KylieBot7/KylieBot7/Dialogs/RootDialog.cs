using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using KylieBot7.Models;
using System.Threading;
using System.Net.Http;
using CRM_Api.Models;

namespace KylieBot7.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            if (string.IsNullOrEmpty(await context.GetAccessToken(AuthSettings.Scopes)))
            {
                await context.Forward(new AzureAuthDialog(AuthSettings.Scopes), this.ResumeAfterAuth, activity, CancellationToken.None);
            }
            else
            {
                context.Wait(MessageReceivedAsync);
            }

            if (!string.IsNullOrEmpty(MessagesController.LocalUser.Token) && activity.Text == "logout")
            {
                await context.Logout();
            }
        }

        private async Task ResumeAfterAuth(IDialogContext context, IAwaitable<string> result)
        {
            var message = await result;
            MessagesController.LocalUser.Token = await context.GetAccessToken(AuthSettings.Scopes);
            await context.PostAsync(message);

            await getCRMContact();
        }

        public async Task getCRMContact()
        {
            if (MessagesController.LocalUser != null)
            {
                HttpClient cons = new HttpClient();
                cons.BaseAddress = new Uri("");
                cons.DefaultRequestHeaders.Accept.Clear();
                cons.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                using (cons)
                {
                    HttpResponseMessage res = await cons.GetAsync("CRM/GetContact/'" + MessagesController.LocalUser.AADEmail.ToString() + "'/");
                    if (res.IsSuccessStatusCode)
                    {
                        CRMContact contact = await res.Content.ReadAsAsync<CRMContact>();
                        MessagesController.LocalUser.CRMContactId = contact.ContactId;
                    }
                }
            }
        }
    }
}