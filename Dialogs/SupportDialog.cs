using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

public class SupportDialog : ComponentDialog
{
    private readonly CLUHelper _cluHelper;
    public SupportDialog(CLUHelper cluHelper) : base(nameof(SupportDialog))
    {
        _cluHelper = cluHelper;

        var steps = new WaterfallStep[]
        {
            LearnSupportKindAsync,
            SendSupportAsync
        };

        AddDialog(new WaterfallDialog(nameof(WaterfallDialog), steps));
        AddDialog(new TextPrompt(nameof(TextPrompt)));
        AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
    }

    private async Task<DialogTurnResult> LearnSupportKindAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        var heroCard = new HeroCard
        {
            Text = "What kind of support do you need? Please select one of the options below.",
            Buttons = new List<CardAction>
            {
            new CardAction(ActionTypes.ImBack, "Account Issues", value: "Account Issues"),
            new CardAction(ActionTypes.ImBack, "Billing & Payments", value: "Billing & Payments"),
            new CardAction(ActionTypes.ImBack, "Product Help", value: "Product Help"),
            new CardAction(ActionTypes.ImBack, "Technical Support", value: "Technical Support"),
            new CardAction(ActionTypes.ImBack, "Contact Support Agent", value: "Contact Support Agent")
            }
        };

        var message = MessageFactory.Attachment(heroCard.ToAttachment());
        await stepContext.Context.SendActivityAsync(message, cancellationToken);

        return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
        {
            Prompt = MessageFactory.Text("Please choose the type of support you need:")
        }, cancellationToken);
    }
    private async Task<DialogTurnResult> SendSupportAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        string supportType = (stepContext.Result as string)?.Trim()?.ToLowerInvariant();

        // Map possible intent values to known support types
        var knownChoices = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "account issues", "Account Issues" },
        { "billing & payments", "Billing & Payments" },
        { "product help", "Product Help" },
        { "technical support", "Technical Support" },
        { "contact support agent", "Contact Support Agent" },
        { "humansupport", "Contact Support Agent" } // For CLU intent
    };
        var intentToSupportType = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "BillingIssue", "Billing & Payments" },
        { "AccountProblem", "Account Issues" },
        { "ProductHelp", "Product Help" },
        { "TechIssue", "Technical Support" },
        { "HumanSupport", "Contact Support Agent" }
    };

        async Task<DialogTurnResult> RespondAndEnd(string type)
        {
            if (type == "Contact Support Agent")
            {
                await stepContext.Context.SendActivityAsync("📞 Connecting you to a human support agent...");
                // TODO: Call API, notify human, or route to live channel
            }
            else
            {
                await stepContext.Context.SendActivityAsync($"Alrigt! I will assist you with {type}. Please provide more details about your issue.");
            }
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        if (!string.IsNullOrEmpty(supportType) && knownChoices.TryGetValue(supportType, out var mappedType))
        {
            return await RespondAndEnd(mappedType);
        }

        var cluResult = await _cluHelper.ExtractUserDetailsAsync(supportType);
        if (cluResult.TryGetValue("Intent", out string intent) && intentToSupportType.TryGetValue(intent, out mappedType))
        {
            Console.WriteLine("Intent is: " + intent);
            return await RespondAndEnd(mappedType);
        }

        await stepContext.Context.SendActivityAsync("I didn't catch that. Could you please specify the type of support you need?");
        return await stepContext.ReplaceDialogAsync(nameof(SupportDialog), null, cancellationToken);
    }


    /*
    private async Task<DialogTurnResult> SendSupportAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        var supportType = (stepContext.Result as string)?.Trim()?.ToLowerInvariant();

        var knownChoices = new HashSet<string>
        {
            "account issues",
            "billing & payments",
            "product help",
            "technical support",
            "contact support agent"
        };

        if (knownChoices.Contains(supportType))
        {
            if (supportType == "contact support agent")
            {
                await stepContext.Context.SendActivityAsync("📞 Connecting you to a human support agent...");
                // TODO: Call API, notify human, or route to live channel
            }
            else
            {
                await stepContext.Context.SendActivityAsync($"Alrigt! I will assist you with {supportType}. Please provide more details about your issue.");
            }
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
        else
        {
            var cluResult = await _cluHelper.ExtractUserDetailsAsync(supportType);
            if (cluResult.TryGetValue("Intent", out string intent))
            {
                intent = intent.ToLowerInvariant();

                Console.WriteLine("Intent is: " + intent);

                if (knownChoices.Contains(intent))
                {
                    if (intent == "humansupport")
                    {
                        await stepContext.Context.SendActivityAsync("Connecting you to a support agent. Please hold on...");
                        // Here you would typically connect to a live agent or escalate the issue
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync($"Alrigt! I will assist you with {intent}. Please provide more details about your issue.");
                    }
                    return await stepContext.EndDialogAsync(null, cancellationToken);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync("I didn't catch that. Could you please specify the type of support you need?");
                    return await stepContext.ReplaceDialogAsync(nameof(SupportDialog), null, cancellationToken);
                }
            }
            else
            {
                await stepContext.Context.SendActivityAsync("I didn't catch that. Could you please specify the type of support you need?");
                return await stepContext.ReplaceDialogAsync(nameof(SupportDialog), null, cancellationToken);
            }
        }
        */
}