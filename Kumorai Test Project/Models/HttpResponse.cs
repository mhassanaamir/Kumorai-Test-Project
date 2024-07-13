using Microsoft.Azure.Functions.Worker.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kumorai_Test_Project.Models
{
    public static class HttpResponse
    {
        public static HttpResponseData CreateResponse(HttpRequestData req, object content, System.Net.HttpStatusCode statusCode) // HttpResponse wrapper
        {
            var response = req.CreateResponse(statusCode);
            _ = response.WriteAsJsonAsync(content).ConfigureAwait(false);
            return response;
        }
    }
}
