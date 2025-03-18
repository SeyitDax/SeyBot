using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Threading;
using System.Threading.Tasks;

public class WelcomeDialog : ComponentDialog
{
    public WelcomeDialog()
        : base(nameof(WelcomeDialog))
    {
        // Define the steps in your dialog
        var waterfallSteps = new WaterfallStep[]
        {
            AskNameStepAsync,
            WelcomeStepAsync,
        };

        // Add the waterfall dialog to the bot
        AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
        AddDialog(new TextPrompt(nameof(TextPrompt)));
    }

    private async Task<DialogTurnResult> AskNameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        // Ask the user for their name
        return await stepContext.PromptAsync(nameof(TextPrompt),
            new PromptOptions { Prompt = MessageFactory.Text("What's your name?") }, cancellationToken);
    }

    private async Task<DialogTurnResult> WelcomeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        // Greet the user with their name
        var name = (string)stepContext.Result;
        await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Welcome, {name}! How can I assist you today?"), cancellationToken);
        return await stepContext.EndDialogAsync(cancellationToken);
    }
}
