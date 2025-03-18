using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

public class GreetingDialog : ComponentDialog
{
    public GreetingDialog() : base(nameof(GreetingDialog))
    {
        // Define the steps of the waterfallDialog
        var waterfallSteps = new WaterfallStep[]
        {
            AskNameStepAsync, //Each Step
            DisplayGreetingStepAsync,
            FollowUpStepAsync // New step added
        };

        // Add dialogs to the dialog set.
        AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
        AddDialog(new TextPrompt(nameof(TextPrompt)));

        // Set the initial dialog to waterfallDialog.
        InitialDialogId = nameof(WaterfallDialog);
    }
    private async Task<DialogTurnResult> AskNameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        // Prompt the user for their name.
        return await stepContext.PromptAsync(nameof(TextPrompt),
            new PromptOptions { Prompt = MessageFactory.Text("What is your name?") },
            cancellationToken);
    }

    private async Task<DialogTurnResult> DisplayGreetingStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        // Retrieve the user's name from the previous step.
        var userInput = (string)stepContext.Result;
        var name = ExtractName(userInput);

        // Store the cleaned name in dialog state
        stepContext.Values["userName"] = name;

        // Respond with a greeting message.
        await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Hello, {name} nice to meet you."), cancellationToken);

        // Move to the next step (Follow-up question)
        return await stepContext.NextAsync(null, cancellationToken);
    }

    private async Task<DialogTurnResult> FollowUpStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        // Ask the user how the bot can assist
        return await stepContext.PromptAsync(nameof(TextPrompt),
            new PromptOptions { Prompt = MessageFactory.Text("How can I assist you today?")},
            cancellationToken);
    }

    private string ExtractName(string input)
    {
        // Define common patterns used to introduce a name
        string[] patterns =
        {
            @"\bmy name is\b",
            @"bit'?s\b",
            @"\bi am\b",
            @"\bthis is\b",
            @"\bcall me\b"
        };

        // Convert input to lowercase for case-insensitive matching
        string cleanedInput = input.ToLower();

        // Remove unwanted phrases
        foreach (var pattern in patterns)
        {
            cleanedInput = Regex.Replace(cleanedInput, pattern, "", RegexOptions.IgnoreCase).Trim();
        }

        // Capitalize each word in the extracted name
        return CapitalizeWords(cleanedInput);
    }

    private string CapitalizeWords(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "bro"; // Default fallback if extraction fails

        string[] words = input.Split(' ');
        for (int i = 0; i < words.Length; i++)
        {
            words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1);
        }
        return string.Join(" ", words);
    }
}