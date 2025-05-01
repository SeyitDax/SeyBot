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
        _cluHelper = cluHelper;

        // Define the steps of the waterfallDialog
        var waterfallSteps = new WaterfallStep[]
        {
            DisplayGreetingStepAsync,
            FollowUpStepAsync, // New step added
            HandleFollowUpResponseStepAsync
        };

        // Add dialogs to the dialog set.
        AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
        AddDialog(new TextPrompt(nameof(TextPrompt)));

        // Set the initial dialog to waterfallDialog.
        InitialDialogId = nameof(WaterfallDialog);
    }
    private async Task<DialogTurnResult> DisplayGreetingStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        var name = await stepContext.BeginDialogAsync(nameof(NameCaptureDialog), null, cancellationToken);
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


        return await stepContext.PromptAsync(nameof(TextPrompt), 
            new PromptOptions {  Prompt = MessageFactory.Text("Please choose an option.")
        }, cancellationToken);
    }
    private async Task<DialogTurnResult> HandleFollowUpResponseStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        var prediction = await _cluHelper.ExtractUserDetailsAsync((stepContext.Result as string)?.ToLower());
        prediction.TryGetValue("Intent", out string intent);

        switch (intent)
        {
            case "Order":
                 await stepContext.Context.SendActivityAsync("Sure! Please enter your order number.");
                break;
            case "Support":
                await stepContext.Context.SendActivityAsync("Okay, I'm here to help! What issue are you facing?");
                break;
            case "Chat":
                await stepContext.Context.SendActivityAsync("Sure, let's chat! How's your day going?");
                break;
            default:
                await stepContext.Context.SendActivityAsync("I'm not sure what you meant.");
                break;
        }

        return await stepContext.EndDialogAsync(intent, cancellationToken);
    }
}