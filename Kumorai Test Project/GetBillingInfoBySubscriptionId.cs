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
                return Models.HttpResponse.CreateResponse(req, "SubscriptionId is required", System.Net.HttpStatusCode.BadRequest);
            }

            try
            {
                string accessToken = await AzureAuth.GetTokenAsync();
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                string requestUri = $"{ApiConstants.BaseUrl}/{subscriptionId}/{ApiConstants.ConsumptionPath}/?{ApiConstants.ApiVersion}";
                var billingInfo = await _httpClient.GetFromJsonAsync<object>(requestUri);

                _logger.LogInformation($"Successfully ran {nameof(GetBillingInfoBySubscriptionId)}");
                return Models.HttpResponse.CreateResponse(req, billingInfo, System.Net.HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred: {0}", ex.Message);
                return Models.HttpResponse.CreateResponse(req, $"An error occurred. {ex.Message}", System.Net.HttpStatusCode.InternalServerError);
            }
            
        }
    }
}
