/*
 * Ref: https://blog.darkloop.com/post/functions-policy-based-auth-for-functions-v4-in-proc-and-isolated
 */

using DarkLoop.Azure.Functions.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

var host = new HostBuilder()
	.ConfigureFunctionsWebApplication(builder => {
		builder.UseFunctionsAuthorization();
	})
	.ConfigureServices((hostContext, services) =>
	{
		
		var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
				.AddEnvironmentVariables()
				.Build();

		var sectionDetails = config.GetSection("BearerJwtTokenAuthOptions");
		var bearerJwtTokenAuthOptions = new BearerJwtTokenAuthOptions();
		sectionDetails.Bind(bearerJwtTokenAuthOptions);
		Console.WriteLine($"Authority: {bearerJwtTokenAuthOptions.Authority}");
		Console.WriteLine($"Audience: {bearerJwtTokenAuthOptions.Audience}");

		services
			.AddFunctionsAuthentication(JwtFunctionsBearerDefaults.AuthenticationScheme)
			.AddJwtFunctionsBearer(options =>
			{
				// this line is here to bypass the token validation
				// and test the functionality of this library.
				// you can create a dummy token by executing the GetTestToken function in HelperFunctions.cs
				// THE FOLLOWING LINE SHOULD BE REMOVED IN A REAL-WORLD SCENARIO
				//options.SecurityTokenValidators.Add(new TestTokenValidator());

				// this is what you should look for in a real-world scenario
				// comment the lines if you cloned this repository and want to test the library
				options.Authority = bearerJwtTokenAuthOptions.Authority;// "https://login.microsoftonline.com/53b7cac7-14be-46d4-be43-f2ad9244d901/v2.0";
				options.Audience = bearerJwtTokenAuthOptions.Audience;// "833fecc2-b972-408c-9a37-50ebc22923d4";
				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
				};
			});

		services
			.AddFunctionsAuthorization(options =>
			{

				// Add your policies here
				// Terrorism Line Of Business
				options.AddPolicy("TerrorismSubmissionWrite", policy => policy.RequireRole("terrorism.submission.write"));
				options.AddPolicy("TerrorismEnrichedReadWrite", policy => policy.RequireRole("terrorism.enriched.readwrite"));

				// PRCB Line Of Business
				options.AddPolicy("PrcbSubmissionWrite", policy => policy.RequireRole("prcb.submission.write"));
				options.AddPolicy("PrcbEnrichedReadWrite", policy => policy.RequireRole("prcb.enriched.readwrite"));
			});

		/*
		// Add authorization services
        services
            .AddFunctionsAuthentication(JwtBearerDefaults.AuthenticationScheme) 
            // or just ASP.NET Core's .AddAuthentication(... 
            .AddJwtBearer(options =>
            {
                options.Authority = "https://login.microsoftonline.com/your-tenant-id";
                options.Audience = "your-client-id";
                // Other JWT configuration options...
            });

        // Define custom authorization policies
        services.AddFunctionsAuthorization(options =>
        {
            options.AddPolicy("OnlyAdmins", policy => policy.RequireRole("Admin"));
        });
		*/
		services.AddApplicationInsightsTelemetryWorkerService();
		services.ConfigureFunctionsApplicationInsights();
	})
	.Build();

host.Run();

public class BearerJwtTokenAuthOptions : AuthenticationSchemeOptions
{
	public string Authority { get; set; }
	public string WellKnownDiscoveryUrl { get; set; }

	public string TokenIssuer { get; set; }

	public string TokenSignedKeyCN { get; set; }

	public string Audience { get; set; }
}