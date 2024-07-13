using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kumorai_Test_Project.Models
{
    public static class ApiConstants
    {
        public const string BaseUrl = "https://management.azure.com/subscriptions";
        public const string AuthBaseUrl = "https://management.azure.com/.default";
        public const string ConsumptionPath = "providers/Microsoft.Consumption/usageDetails";
        public const string ApiVersion = "api-version=2023-11-01";
    }
}
