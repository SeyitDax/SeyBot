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
            FollowUpStepAsync, // New step added
            HandleFollowUpResponseStepAsync
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
        userDetails.TryGetValue("Intent", out string intent);

        if (string.IsNullOrWhiteSpace(name))
        {
            if(intent == "CheckOrders")
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("Of course! But first, may I have your name?"),
                    cancellationToken
                    );
                return await stepContext.ReplaceDialogAsync(nameof(GreetingDialog), null, cancellationToken);
            }
            else if (intent == "Rejection")
            {
                name = "friend";
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("Alright friend"), cancellationToken
                    );
                stepContext.Values["UserName"] = name;
                return await stepContext.BeginDialogAsync("FollowUpStepAsync", cancellationToken);
            }
            else
            {
                // Default name prompt
                await stepContext.Context.SendActivityAsync(
                        MessageFactory.Text("Sorry, I didn't get your name. Would you like to skip?"),
                        cancellationToken
                        );
                return await stepContext.ReplaceDialogAsync(nameof(GreetingDialog), null, cancellationToken);
            }
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

        return Dialog.EndOfTurn;
    }
    private async Task<DialogTurnResult> HandleFollowUpResponseStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {

        var userResponse = stepContext.Context.Activity.Text?.ToLower();

        if (userResponse.Contains("orders"))
        {
            await stepContext.Context.SendActivityAsync("Sure! Please enter your order number.", cancellationToken: cancellationToken);
        }
        else if (userResponse.Contains("support"))
        {
            await stepContext.Context.SendActivityAsync("Okay, I'm here to help! What issue are you facing?", cancellationToken: cancellationToken);
        }
        else if (userResponse.Contains("chat"))
        {
            await stepContext.Context.SendActivityAsync("Sure, let's chat! How's your day going?", cancellationToken: cancellationToken);
        }
        else
        {
            await stepContext.Context.SendActivityAsync("I'm not sure what you meant. Can you choose an option?", cancellationToken: cancellationToken);
        }
        return await stepContext.EndDialogAsync(null, cancellationToken);
    }
}