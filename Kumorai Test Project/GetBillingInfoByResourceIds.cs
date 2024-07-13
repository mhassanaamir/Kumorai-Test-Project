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
                return Models.HttpResponse.CreateResponse(req, "ResourceIds and SubscriptionId are required", System.Net.HttpStatusCode.BadRequest);
            }

            try
            {
                string accessToken = await AzureAuth.GetTokenAsync();
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var billingInfoTaskList = new List<Task>();

                foreach (var resourceId in resourceIds)
                {
                    string requestUri = $"{ApiConstants.BaseUrl}/{subscriptionId}/{ApiConstants.ConsumptionPath}/{resourceId}?{ApiConstants.ApiVersion}";
                    var billingInfoTask = _httpClient.GetFromJsonAsync<object>(requestUri);
                    billingInfoTaskList.Add(billingInfoTask);   // Add each task in list then use when all for optimization
                }

                await Task.WhenAll(billingInfoTaskList);

                _logger.LogInformation($"Successfully ran {nameof(GetBillingInfoByResourceIds)}");
                return Models.HttpResponse.CreateResponse(req, billingInfoTaskList, System.Net.HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred: {0}", ex.Message);
                return Models.HttpResponse.CreateResponse(req, $"An error occurred. {ex.Message}", System.Net.HttpStatusCode.InternalServerError);
            }
        }
    }
}
