using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System.Threading;
using System.Threading.Tasks;

public class MainDialog : ComponentDialog
{
    public MainDialog() 
        : base(nameof(MainDialog))
    {
        // Add dialogs to the MainDialog
        AddDialog(new GreetingDialog());
        AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
        {
            StartGreetingDialogStepAsync
        }));

        InitialDialogId = nameof(WaterfallDialog);
    }

    private async Task<DialogTurnResult> StartGreetingDialogStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        // Begin GreetingDialog
        return await stepContext.BeginDialogAsync(nameof(GreetingDialog), null, cancellationToken); 
    }
}