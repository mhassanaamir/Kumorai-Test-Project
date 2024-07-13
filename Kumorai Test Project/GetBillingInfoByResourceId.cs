using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
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

            if (string.IsNullOrEmpty(subscriptionId))
            {
                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                await response.WriteStringAsync("Please pass a valid resourceId in the request body");
                return response;
            }

            string accessToken = await AzureAuth.GetTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            string requestUri = $"{Constants.BaseUrl}/subscriptions/{subscriptionId}/{Constants.ConsumptionPath}/{resourceId}?{Constants.ApiVersion}";
            var billingInfo = await _httpClient.GetFromJsonAsync<object>(requestUri);

            response.StatusCode = System.Net.HttpStatusCode.OK;
            await response.WriteAsJsonAsync(billingInfo);
            return response;
        }
    }
}
