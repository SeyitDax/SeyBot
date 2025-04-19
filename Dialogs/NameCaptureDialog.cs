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

        AddDialog(new WaterfallDialog(nameof(WaterfallDialog)), new WaterfallStep[]
        {

        });

        AddDialog(new TextPrompt(nameof(TextPrompt)));
        InitialDialogId = (nameof(WaterfallDialog));
    }
}