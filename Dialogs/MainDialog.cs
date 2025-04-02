using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Threading.Tasks;
using System.Threading;

public class MainDialog : ComponentDialog
{
    public MainDialog(GreetingDialog greetingDialog) : base(nameof(MainDialog))
    {
        AddDialog(greetingDialog);
        AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
        {
            BeginGreetingDialogStepAsync
        }));

        InitialDialogId = nameof(WaterfallDialog);
    }

    private async Task<DialogTurnResult> BeginGreetingDialogStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        return await stepContext.BeginDialogAsync(nameof(GreetingDialog), null, cancellationToken);
    }
}
