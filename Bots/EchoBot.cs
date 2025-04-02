// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.22.0

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SeyBot.Bots
{
    public class EchoBot : ActivityHandler
    {
        private readonly ConversationState _conversationState;
        private readonly DialogSet _dialogs;
        private readonly Dialog _mainDialog;

        public EchoBot(ConversationState conversationState, MainDialog mainDialog)
        {
            _conversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
            _mainDialog = mainDialog ?? throw new ArgumentNullException(nameof(mainDialog));

            var dialogState = conversationState.CreateProperty<DialogState>("DialogState");
            _dialogs = new DialogSet(dialogState);
            _dialogs.Add(_mainDialog);
        }
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // Create dialog context
            var dialogContext = await _dialogs.CreateContextAsync(turnContext, cancellationToken);

            if (dialogContext.ActiveDialog == null)
            {
                await dialogContext.BeginDialogAsync(nameof(MainDialog), null, cancellationToken);
            }
            else
            {
                // Continue the current dailog
                await dialogContext.ContinueDialogAsync(cancellationToken);
            }

            // Save the conversation state
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);

            // Simple  "Hello/Bye" Text
            /*
            var userMessage = turnContext.Activity.Text.ToLower();

            if (userMessage.Contains("hello"))
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("Hello! How can I assist you today?"), cancellationToken);
            }
            else if (userMessage.Contains("bye"))
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("Goodbye! Have a great day!"), cancellationToken);
            }
            else
            {
                await turnContext.SendActivityAsync(MessageFactory.Text($"You said: {turnContext.Activity.Text}"), cancellationToken);
            }
            */
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome!";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }
    }
}
