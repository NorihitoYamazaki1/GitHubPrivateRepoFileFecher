using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace GitHubPrivateRepoFileFecher;

public class Functions
{
    private readonly IHttpClientFactory _httpClientFactory;

    public Functions(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    [FunctionName(nameof(GitHubPrivateRepoFileFetcher))]
    public async Task<IActionResult> GitHubPrivateRepoFileFetcher(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]
        HttpRequest req,
        ILogger log)
    {
        log.LogInformation($"C# HTTP trigger function processed a request. RequestUri={req.Path}");

        string gitHubUri = req.Query["githuburi"];
        string gitHubAccessToken = req.Query["githubaccesstoken"];

        log.LogInformation($"GitHubPrivateRepoFileFecher function is trying to get file content from {gitHubUri}");

        if (gitHubUri == null)
        {
            return new BadRequestObjectResult(
                "Please pass the GitHub raw file content URI (raw.githubusercontent.com) in the request URI string");
        }

        if (gitHubAccessToken == null)
        {
            return new BadRequestObjectResult("Please pass the GitHub personal access token in the request URI string");
        }

        var strAuthHeader = "token " + gitHubAccessToken;
        var client = _httpClientFactory.CreateClient();

        client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3.raw");
        client.DefaultRequestHeaders.Add("Authorization", strAuthHeader);
        var response = await client.GetAsync(gitHubUri);
        var prefecturesJsonString = await response.Content.ReadAsStringAsync();
        return new OkObjectResult(prefecturesJsonString);
    }
}