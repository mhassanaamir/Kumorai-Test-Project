using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Kumorai_Test_Project.Models;

namespace Kumorai_Test_Project
{
    public class GetBillingInfoByResourceId
    {
        private readonly ILogger<GetBillingInfoByResourceId> _logger;
        private readonly HttpClient _httpClient;

        public GetBillingInfoByResourceId(ILogger<GetBillingInfoByResourceId> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        [Function(nameof(GetBillingInfoByResourceId))]
        public async Task<HttpResponseData> Run(
       [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req, FunctionContext executionContext)
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string resourceId = data?.ResourceId;
            string subscriptionId = data?.SubscriptionId;

            var response = req.CreateResponse();

            if (string.IsNullOrEmpty(subscriptionId) || string.IsNullOrEmpty(resourceId))
            {
                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                await response.WriteStringAsync("ResourceId and SubscriptionId are required");
                return response;
            }

            string accessToken = await AzureAuth.GetTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            string requestUri = $"{Constants.BaseUrl}/subscriptions/{subscriptionId}/{Constants.ConsumptionPath}/{resourceId}?{Constants.ApiVersion}";
            var billingInfo = await _httpClient.GetFromJsonAsync<object>(requestUri);

            response.StatusCode = System.Net.HttpStatusCode.OK;
            await response.WriteAsJsonAsync(billingInfo);

            _logger.LogInformation($"Successfully ran {nameof(GetBillingInfoByResourceId)}");
            return response;
        }
    }
}
