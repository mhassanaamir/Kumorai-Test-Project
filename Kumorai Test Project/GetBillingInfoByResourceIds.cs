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
    public class GetBillingInfoByResourceIds
    {
        private readonly ILogger<GetBillingInfoByResourceIds> _logger;
        private readonly HttpClient _httpClient;

        public GetBillingInfoByResourceIds(ILogger<GetBillingInfoByResourceIds> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        [Function(nameof(GetBillingInfoByResourceIds))]
        public async Task<HttpResponseData> Run(
       [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req, FunctionContext executionContext)
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            IEnumerable<string> resourceIds = data?.ResourceIds?.ToObject<List<string>>();
            string subscriptionId = data?.SubscriptionId;

            var response = req.CreateResponse();

            if (string.IsNullOrEmpty(subscriptionId) || resourceIds == null || !resourceIds.Any())
            {
                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                await response.WriteStringAsync("ResourceIds and SubscriptionId are required");
                return response;
            }

            string accessToken = await AzureAuth.GetTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var billingInfoTaskList = new List<Task>();

            foreach (var resourceId in resourceIds)
            {
                string requestUri = $"{Constants.BaseUrl}/subscriptions/{subscriptionId}/{Constants.ConsumptionPath}/{resourceId}?{Constants.ApiVersion}";
                var billingInfoTask = _httpClient.GetFromJsonAsync<object>(requestUri);
                billingInfoTaskList.Add(billingInfoTask);
            }

            await Task.WhenAll(billingInfoTaskList);
            response.StatusCode = System.Net.HttpStatusCode.OK;
            await response.WriteAsJsonAsync(billingInfoTaskList);

            _logger.LogInformation($"Successfully ran {nameof(GetBillingInfoByResourceIds)}");
            return response;
        }
    }
}
