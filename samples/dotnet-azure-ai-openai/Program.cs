using System.ClientModel.Primitives;
using Azure.AI.OpenAI;
using Azure.Identity;
using OpenAI.Assistants;

#pragma warning disable OPENAI001

string rawEndpoint = Environment.GetEnvironmentVariable("AZURE_AI_AGENTS_ENDPOINT")
    ?? throw new ArgumentException("The AZURE_AI_AGENTS_ENDPOINT environment variable must be set.");
Uri endpoint = new(rawEndpoint);

AzureAIAgentClientOptions agentClientOptions = new()
{
    ApiVersion = "my-api-version",
    AdditionalHeaders =
    {
        ["foo"] = "bar",
    },
};

// Be sure to use the AzureAIAgentClientOptions instance when instantiating the AzureOpenAIClient
AzureOpenAIClient aoaiClient = new(endpoint, new DefaultAzureCredential(), agentClientOptions);

// Then just retrieve an AssistantClient and use it like normal
AssistantClient assistantClient = aoaiClient.GetAssistantClient();

await foreach (ThreadRun run in assistantClient.GetRunsAsync("thread-id"))
{

}