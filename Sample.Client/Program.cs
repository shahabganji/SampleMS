using System;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;
using Newtonsoft.Json.Linq;
using static System.Console;

namespace Sample.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var client = new HttpClient();
            
            var discovery = await Discover(client);
            if (discovery.IsError) return;
            
            var tokenResponse = await GetAccessToken(client, discovery);
            if(tokenResponse.IsError) return;

            client.SetBearerToken(tokenResponse.AccessToken);
            var weatherResponseMessage = await client.GetAsync("https://localhost:6001/weatherforecast");

            if (weatherResponseMessage.IsSuccessStatusCode)
            {
                var result = await weatherResponseMessage.Content.ReadAsStringAsync();
                WriteLine(JArray.Parse(result));
            }
            
            ReadKey();
        }

        private static async Task<DiscoveryDocumentResponse> Discover(HttpClient client)
        {
            var discovery = await client.GetDiscoveryDocumentAsync("https://localhost:7001");

            if (!discovery.IsError) return discovery;
            
            WriteLine(discovery.Error);
            return discovery;
        }

        private static async Task<TokenResponse> GetAccessToken(HttpClient client, DiscoveryDocumentResponse discovery)
        {
            var tokenResponse = await client.RequestClientCredentialsTokenAsync(
                new ClientCredentialsTokenRequest
                {
                    Address = discovery.TokenEndpoint,
                    ClientId = "client1", ClientSecret = "secret", Scope = "Api1"
                });

            if (tokenResponse.IsError)
            {
                WriteLine(tokenResponse.Error);
                return tokenResponse;
            }

            WriteLine(tokenResponse.Json);
            return tokenResponse;
        }
    }
}
