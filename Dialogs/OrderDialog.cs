using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

public class OrderDialog : ComponentDialog
{
    private readonly CLUHelper _cluHelper;
    public OrderDialog(CLUHelper cluHelper) : base(nameof(OrderDialog))
    {
        _cluHelper = cluHelper;

        var steps = new WaterfallStep[]
        {
            AskOrderNumberAsync,
            ConfirmOrderAsync
        };

        AddDialog(new WaterfallDialog(nameof(WaterfallDialog), steps));
        AddDialog(new TextPrompt(nameof(TextPrompt), OrderNumberValidatorAsync));

        InitialDialogId = nameof(WaterfallDialog);
    }

    private async Task<DialogTurnResult> AskOrderNumberAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
        {
            Prompt = MessageFactory.Text("Please enter your order number.")
        }, cancellationToken);
    }

    private async Task<bool> OrderNumberValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
    {
        var input = promptContext.Recognized.Value?.Trim();

        // Regex: accepts ORD12345, #1234, 654321
        var pattern = @"^(#|ORD)?\d{4,10}$";
        var isValid = Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase);

        if (isValid)
        {
            promptContext.Recognized.Value = Regex.Match(input, @"\d+").Value;
            return true;
        }

        // Try CLU fallback
        var cluResult = await _cluHelper.ExtractUserDetailsAsync(input);
        if (cluResult.TryGetValue("ordernumber", out var orderNumber))
        {
            promptContext.Recognized.Value = orderNumber;
            return true;
        }

        // Both failed
        await promptContext.Context.SendActivityAsync(
            "I couldn't find a valid order number. Please try again with something like 'ORD12345' or '#54321'.");

        return false;
    }

    private async Task<DialogTurnResult> ConfirmOrderAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        var orderNumber = stepContext.Result as string;
        await stepContext.Context.SendActivityAsync($"Thanks! Looking up order #{orderNumber}...");
        return await stepContext.EndDialogAsync(orderNumber, cancellationToken);
    }
}