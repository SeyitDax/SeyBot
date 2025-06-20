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
    }

    private async Task<DialogTurnResult> LearnSupportKindAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
        {
            Prompt = MessageFactory.Text("What kind of support do you need?"),
            Choices = ChoiceFactory.ToChoices(new List<string>
            {
                "Account Issues",
                "Billing & Payments",
                "Product Help",
                "Technical Support",
                "Contact Support Agent"
            })
        }, cancellationToken);
    }

    private async Task<DialogTurnResult> SendSupportAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        string userIssue = stepContext.Result as string;
        await stepContext.Context.SendActivityAsync($"Alirght, Let's handle {userIssue}!");
        return await stepContext.EndDialogAsync(userIssue,cancellationToken);
    }
}