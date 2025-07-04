using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

public class ChatDialog : ComponentDialog
{
    private readonly CLUHelper _cluHelper;
    private readonly OpenAIHelper _openAIHelper;

    public ChatDialog(CLUHelper cluHelper, OpenAIHelper openAIHelper) : base(nameof(ChatDialog))
    {
        _cluHelper = cluHelper;
        _openAIHelper = openAIHelper;

        var steps = new WaterfallStep[]
        {
            PromptForMessageAsync,
            RespondToMessageAsync
        };

        AddDialog(new WaterfallDialog(nameof(WaterfallDialog), steps));
        AddDialog(new TextPrompt(nameof(TextPrompt)));

        InitialDialogId = nameof(WaterfallDialog);
    }

    private async Task<DialogTurnResult> PromptForMessageAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
        {
            Prompt = MessageFactory.Text("What would you like to chat about?")
        }, cancellationToken);
    }

    private async Task<DialogTurnResult> RespondToMessageAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        var userMessage = stepContext.Result as string;
        string aiResponse;
        try
        {
            aiResponse = await _openAIHelper.GetChatCompletionAsync(userMessage);
        }
        catch (Exception ex)
        {
            // Optionally log ex
            aiResponse = "Sorry, something went wrong while contacting the AI service.";
        }
        await stepContext.Context.SendActivityAsync(aiResponse);
        return await stepContext.EndDialogAsync(null, cancellationToken);
    }
}