## Sample: Azure AI Agents via the Azure OpenAI .NET Client Library

This small sample demonstrates the configuration of the .NET `Azure.AI.OpenAI` client library for use with the Azure AI Agents service and provides a reusable `AzureAIAgentClientOptions` class to simplify this configuration.

### Usage

1. Set the `AZURE_AI_AGENTS_ENDPOINT` environment variable
2. `dotnet run`

### Code Explanation

The included `AzureAIAgentClientOptions` class inherits from `AzureOpenAIClientOptions` and automatically uses a custom `PipelinePolicy`, a concept from `System.ClientModel`, to customize Azure OpenAI traffic to achieve compatibility with Azure AI Agents.

Specific changes enacted by the wrapper include:

- The ability to provide an out-of-band, custom `api-version` query string parameter that will replace the built-in value attached to Azure OpenAI
- The ability to provide custom headers
- Removal of request URI construction specific to Azure OpenAI, specifically the infix of `/openai` in Assistants endpoint request URIs
- Ensured use of the `include[]` query string parameter for compatibility with `file_search` citation content