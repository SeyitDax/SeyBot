using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

public class NameCaptureDialog : ComponentDialog
{
    private readonly CLUHelper _cluHelper;

    public NameCaptureDialog(CLUHelper cLUHelper) : base(nameof(NameCaptureDialog))
    {
        _cluHelper = cLUHelper;

        AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
        {
            AskNameStepAsync,
            ValidateNameStepAsync
        }));

        AddDialog(new TextPrompt(nameof(TextPrompt)));
        InitialDialogId = (nameof(WaterfallDialog));
    }

    

    private async Task<DialogTurnResult> AskNameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        return await stepContext.PromptAsync(nameof(TextPrompt),
            new PromptOptions { Prompt = MessageFactory.Text("May I have your name before we proceed?") }, cancellationToken);
    }

    private async Task<DialogTurnResult> ValidateNameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        var input = (string)stepContext.Result;
        var result = await _cluHelper.ExtractUserDetailsAsync(input); 
        result.TryGetValue("PersonName", out string name);
        result.TryGetValue("Intent", out string intent);

        name ??= (intent == "Rejection") ? "friend" : null;

        if(string.IsNullOrEmpty(name))
        {
           await stepContext.Context.SendActivityAsync("Sorry, I didn't catch that.");
            return await stepContext.ReplaceDialogAsync(nameof(NameCaptureDialog), null, cancellationToken);
        }

        stepContext.Values["UserName"] = name;
        return await stepContext.EndDialogAsync(name, cancellationToken);
    }
}