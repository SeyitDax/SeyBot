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
            BeginNameCaptureStepAsync,
            GreetUserStepAsync,
            FollowUpStepAsync, // New step added
            HandleFollowUpResponseStepAsync
        };

        // Add dialogs to the dialog set.
        AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
        AddDialog(new TextPrompt(nameof(TextPrompt)));

        // Registering the NameCaptureDialog so BeginDialogAsync can find it
        AddDialog(new NameCaptureDialog(_cluHelper));
        AddDialog(new OrderDialog(_cluHelper));
        AddDialog(new SupportDialog(_cluHelper));
        AddDialog(new ChatDialog(_cluHelper));

        // Set the initial dialog to waterfallDialog.
        InitialDialogId = nameof(WaterfallDialog);
    }

    private Task<DialogTurnResult> BeginNameCaptureStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        // return the Prompt via your child dialog
        return stepContext.BeginDialogAsync(nameof(NameCaptureDialog), null, cancellationToken);
    }
    private async Task<DialogTurnResult> GreetUserStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        // the result of NameCaptureDialog.EndDialogAsync(name) is now in stepContext.Result
        var name = (string)stepContext.Result;

        await stepContext.Context.SendActivityAsync(
            MessageFactory.Text($"Hello, {name}! Pleased to meet you."),
            cancellationToken); 

        // advance to the next step in *this* waterfall
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
        Console.WriteLine("Message is: " + stepContext.Result as string);

        var prediction = await _cluHelper.ExtractUserDetailsAsync((stepContext.Result as string)?.ToLower());
        prediction.TryGetValue("intent", out string intent);

        Console.WriteLine("Intent is: " + intent);

        switch (intent)
        {
            case "CheckOrders":
                 return await stepContext.BeginDialogAsync(nameof(OrderDialog), null, cancellationToken);
            case "GetSupport":
                return await stepContext.BeginDialogAsync(nameof(SupportDialog), null, cancellationToken);
            case "Chat":
                return await stepContext.BeginDialogAsync(nameof(ChatDialog), null, cancellationToken);
            default:
                await stepContext.Context.SendActivityAsync("I'm not sure what you meant.");
                return await stepContext.EndDialogAsync(intent, cancellationToken);
        }
    }
}