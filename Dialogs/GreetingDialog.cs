using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

public class GreetingDialog : ComponentDialog
{
    private readonly CLUHelper _cluHelper;

    public GreetingDialog(CLUHelper cluHelper) : base(nameof(GreetingDialog))
    {
        _cluHelper = cluHelper ?? throw new ArgumentNullException(nameof(cluHelper));

        // Define the steps of the waterfallDialog
        var waterfallSteps = new WaterfallStep[]
        {
            AskNameStepAsync, //Each Step
            ValidateNameStepAsync,
            DisplayGreetingStepAsync,
            FollowUpStepAsync // New step added
        };

        // Add dialogs to the dialog set.
        AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
        AddDialog(new TextPrompt(nameof(TextPrompt)));

        // Set the initial dialog to waterfallDialog.
        InitialDialogId = nameof(WaterfallDialog);
    }
    private async Task<DialogTurnResult> AskNameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        // Prompt the user for their name.
        return await stepContext.PromptAsync(nameof(TextPrompt),
            new PromptOptions { Prompt = MessageFactory.Text("Would you be kind enough to let me know your name?") },
            cancellationToken);
    }
    private async Task<DialogTurnResult> ValidateNameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        var userInput = (string)stepContext.Result;
        var userDetails = await _cluHelper.ExtractUserDetailsAsync(userInput);

        userDetails.TryGetValue("PersonName", out string name);

        if (string.IsNullOrWhiteSpace(name))
        {
            // If the name is invalid, ask again
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("I didn't quite catch that. Plase enter your real name."), cancellationToken);
            return await stepContext.ReplaceDialogAsync(nameof(WaterfallDialog), null, cancellationToken);
        }

        // Store the cleaned name
        stepContext.Values["UserName"] = name;
        return await stepContext.NextAsync(name, cancellationToken);
    }
    private async Task<DialogTurnResult> DisplayGreetingStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        var name = (string)stepContext.Values["UserName"];
        await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Hello, {name}! Pleased to meet you."), cancellationToken);
        return await stepContext.NextAsync(null, cancellationToken);
    }
    private async Task<DialogTurnResult> FollowUpStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        var heroCard = new HeroCard
        {
            Text = "How can I assist you today?",
            Buttons = new List<CardAction>
        {
            new CardAction(ActionTypes.ImBack, "Check Orders", value: "orders"),
            new CardAction(ActionTypes.ImBack, "Get Support", value: "support"),
            new CardAction(ActionTypes.ImBack, "Chat", value: "chat")
        }
        };

        var message = MessageFactory.Attachment(heroCard.ToAttachment());

        await stepContext.Context.SendActivityAsync(message, cancellationToken);

        return await stepContext.NextAsync(null, cancellationToken);
    }
}