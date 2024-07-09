using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AzureFunc_AAD_Auth
{
    public class PrintConfig
    {
        private readonly ILogger<PrintConfig> _logger;
		private readonly IConfiguration _configuration;
        private readonly BearerJwtTokenAuthOptions _jwtTokenAuthOptions;

		public PrintConfig(ILogger<PrintConfig> logger, IConfiguration configuration, IOptions<BearerJwtTokenAuthOptions> jwtTokenAuthOptions)
        {
            _logger = logger;
            _configuration = configuration;
            _jwtTokenAuthOptions = jwtTokenAuthOptions.Value;
        }

        [Function("PrintConfig")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            // Read configuration data
            //string kjwtSectioneyName = "BearerJwtTokenAuthOptions:Audience";
            var configSection = _configuration["BearerJwtTokenAuthOptions:Audience"];


			//response.WriteString(message ?? $"Please create a key-value with the key '{keyName}' in Azure App Configuration.");

			return new OkObjectResult($"Audience is: {configSection}");
        }
    }
}
