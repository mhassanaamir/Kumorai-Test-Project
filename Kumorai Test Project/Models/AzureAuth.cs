using Azure.Core;
using Azure.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kumorai_Test_Project.Models
{
    public static class AzureAuth
    {
        
        public static async Task<string> GetTokenAsync()
        {
            var clientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
            var clientSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET");
            var tenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID");

            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
            var tokenRequestContext = new TokenRequestContext([Constants.AuthBaseUrl]);
            var token = await credential.GetTokenAsync(tokenRequestContext);
            return token.Token;
        }
    }
}
