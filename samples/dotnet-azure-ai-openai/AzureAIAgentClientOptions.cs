using Azure.AI.OpenAI;
using System.ClientModel.Primitives;
using System.Text.RegularExpressions;

/// <summary>
/// A specialization of <see cref="AzureOpenAIClientOptions"/> intended to allow the customization of an
/// <see cref="AzureOpenAIClient"/> to use the Azure AI Agents service. 
/// </summary>
/// <remarks>
/// As the Azure AI Agents service is distinct from the Azure OpenAI Assistants API, compatibility is subject to change
/// across API versions.
/// </remarks>
public class AzureAIAgentClientOptions : AzureOpenAIClientOptions
{
    /// <summary>
    /// The <c>api-version</c> query string parameter value to use when connecting to Azure AI Agents.
    /// </summary>
    public required string ApiVersion { get; set; }

    /// <summary>
    /// Key/value pairs that should be included on request headers. Pairs with keys that already exist in the headers
    /// will have their current values be overwritten with the newly-provided value.
    /// </summary>
    public Dictionary<string, string> AdditionalHeaders { get; } = [];

    /// <summary>
    /// Creates a new instance of <see cref="AzureAIAgentClientOptions"/> that will customize an
    /// <see cref="AzureOpenAIClient"/> for use with the Azure AI Agents service.
    /// </summary>
    /// <remarks>
    /// A <see cref="PipelinePolicy"/> will automatically be applied that performs request customizations. These
    /// customizations include:
    /// <para>
    /// <list type="bullet">
    /// <item>The <c>/openai</c> request URI path infix is removed</item>
    /// <item>The <c>include[]</c> query string parameter is automatically emplaced if not already present</item>
    /// <item>The <c>api-version</c> query string parameter is updated to the provided custom value</item>
    /// <item>Any specified additional headers are added to the request</item>
    /// </list>
    /// </para>
    /// </remarks>
    public AzureAIAgentClientOptions() : base()
    {
        AddPolicy(new AzureAIAgentTrafficPolicy(this), PipelinePosition.PerCall);
    }

    internal class AzureAIAgentTrafficPolicy : PipelinePolicy
    {
        public AzureAIAgentClientOptions ParentOptions { get; }

        public AzureAIAgentTrafficPolicy(AzureAIAgentClientOptions parentOptions)
        {
            ParentOptions = parentOptions;
        }

        public override void Process(PipelineMessage message, IReadOnlyList<PipelinePolicy> pipeline, int currentIndex)
        {
            ProcessRequest(message.Request);
            ProcessNext(message, pipeline, currentIndex);
        }

        public override async ValueTask ProcessAsync(PipelineMessage message, IReadOnlyList<PipelinePolicy> pipeline, int currentIndex)
        {
            ProcessRequest(message.Request);
            await ProcessNextAsync(message, pipeline, currentIndex);
        }

        private void ProcessRequest(PipelineRequest request)
        {
            if (request.Uri is null)
            {
                throw new ArgumentException(nameof(request.Uri));
            }
            UriBuilder uriBuilder = new(request.Uri);

            // Remove the "/openai" request URI infix
            uriBuilder.Path = Regex.Replace(uriBuilder.Path, "/openai", "");

            // Substitute the Azure AI Agents api-version where the default AOAI one is
            uriBuilder.Query = Regex.Replace(uriBuilder.Query, "api-version=[^&]*", $"api-version={ParentOptions.ApiVersion}");

            // Ensure file search citation result content is always requested on run steps
            if (!uriBuilder.Query.Contains("include[]"))
            {
                uriBuilder.Query += "&include[]=step_details.tool_calls[*].file_search.results[*].content";
            }

            request.Uri = uriBuilder.Uri;

            // Emplace custom headers
            foreach ((string key, string value) in ParentOptions.AdditionalHeaders)
            {
                request.Headers.Set(key, value);
            }
        }
    }
}