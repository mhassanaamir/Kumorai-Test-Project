using Kumorai_Test_Project.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http.Json;

namespace Kumorai_Test_Project
{
    public class GetBillingInfoBySubscriptionId
    {
        private readonly ILogger<GetBillingInfoBySubscriptionId> _logger;
        private readonly HttpClient _httpClient;

        public GetBillingInfoBySubscriptionId(ILogger<GetBillingInfoBySubscriptionId> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        [Function(nameof(GetBillingInfoBySubscriptionId))]
        public async Task<HttpResponseData> Run(
       [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req, FunctionContext executionContext)
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string subscriptionId = data?.SubscriptionId;

            var response = req.CreateResponse();

            if (string.IsNullOrEmpty(subscriptionId))
            {
                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                await response.WriteStringAsync("SubscriptionId is required");
                return response;
            }

            string accessToken = await AzureAuth.GetTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            string requestUri = $"{Constants.BaseUrl}/subscriptions/{subscriptionId}/{Constants.ConsumptionPath}/?{Constants.ApiVersion}";
            var billingInfo = await _httpClient.GetFromJsonAsync<object>(requestUri);

            response.StatusCode = System.Net.HttpStatusCode.OK;
            await response.WriteAsJsonAsync(billingInfo);

            _logger.LogInformation($"Successfully ran {nameof(GetBillingInfoBySubscriptionId)}");
            return response;
        }
    }
}
