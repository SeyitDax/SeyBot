using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

public class SupportDialog : ComponentDialog
{
    private readonly CLUHelper _cluHelper;
    public SupportDialog(CLUHelper cluHelper) : base(nameof(OrderDialog))
    {
        _cluHelper = cluHelper;

        var steps = new WaterfallStep[]
        {

        };

        AddDialog(new WaterfallDialog(nameof(WaterfallDialog), steps));
        AddDialog(new TextPrompt(nameof(TextPrompt)));

        InitialDialogId = nameof(WaterfallDialog);
    }
}