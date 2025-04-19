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
    public OrderDialog(GreetingDialog greetingDialog) : base(nameof(OrderDialog))
    {
        // Define the steps of the waterfallDialog
        var waterfallSteps = new WaterfallStep[]
        {
          //  InquireOrdersAsync //Each Step
        };

        // Add dialogs to the dialog set.
        AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
        AddDialog(new TextPrompt(nameof(TextPrompt)));

        // Set the initial dialog to waterfallDialog.
        InitialDialogId = nameof(WaterfallDialog);
    }
 //  private async Task<DialogTurnResult> InquireOrdersAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
 //  {
 //      string selectedOption = stepContext.Result?.ToString();
 //
 //      if (selectedOption == "orders")
 //      {
 //
 //      }
 //  }
}