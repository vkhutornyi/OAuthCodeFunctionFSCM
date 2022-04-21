#r "Newtonsoft.Json"

using System.Net;
using System.Text;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives; 
using Newtonsoft.Json;

public static string AAD_TENANTID       = Environment.GetEnvironmentVariable("AAD_TENANTID");
public static string CLIENT_ID          = Environment.GetEnvironmentVariable("CLIENT_ID");
public static string CLIENT_SECRET      = Environment.GetEnvironmentVariable("CLIENT_SECRET");
public static string BASE_URI           = Environment.GetEnvironmentVariable("BASE_URI"); 

public static async Task<IActionResult> Run(HttpRequest req, ILogger log)
{
    HttpClient client = new HttpClient();

    switch(req.Method)
    {
        case "GET":
        {
            log.LogInformation("C# HTTP trigger function processed a GET request.");
            string AuthCode = "";
            string State = "";
            AuthCode = req.Query["code"];
            State = req.Query["state"];

            if(AuthCode != null && State != null)
            {
                string resp = "";
                try
                {
                    var requestBodyAuth = new {code = AuthCode, state = State};
                    resp = await PostAuthCodeToBCAsync(JsonConvert.SerializeObject(new {_contract = requestBodyAuth}),log);
                    log.LogInformation($"Response from FSC='{resp.ToString()}'");
                    //dynamic respData = JsonConvert.DeserializeObject(resp);
                    //log.LogInformation($"Response JSON from FSC={respData}");
                    string responseStringMessage = ""; 
                    //switch(respData?.value.ToString())
                    switch(resp.ToString())
                    {
                        case "\"OK\"": 
                            responseStringMessage = "<h1>Authorization successfully <span style=\"background-color: #ccffcc;\">passed</span>.</h1><h2>Please refresh the Square Integrations page.</h2><p>You can close this tab.</p>";
                            log.LogInformation($"OK: {resp}");
                            //return new OkObjectResult(responseStringMessage);
                            return new ContentResult { Content = responseStringMessage, ContentType = "text/html" };
                            break; 
                        case "\"FAILED\"":
                            responseStringMessage = "<h1>Authorization <span style=\"background-color: #ff0000;\">failed</span>.</h1><h2>Failed to retrieve an access token.</h2><p>You can close this tab.</p>";
                            log.LogInformation($"FAILED: {resp}");
                            //return new NotFoundObjectResult(responseStringMessage);
                            return new ContentResult { Content = responseStringMessage, ContentType = "text/html" };
                            break;
                    }
                    return new NotFoundObjectResult($"Undefined response from FSC {resp}");
                }
                catch(Exception ex) 
                {
                    return new BadRequestObjectResult(ex.Message + ": " + resp); 
                }
            }
            else
            {
                string badRequest = "<h1>You cannot execute this Azure Function directly.</h1><p><span style=\"color: #ff0000;\">Authorization denied.</span></p><p>You chose to restrict access to the app.</p>";
                return new ContentResult { Content = badRequest, ContentType = "text/html" };
                //return new BadRequestObjectResult("You cannot execute this Azure Function directly. Authorization denied. You chose to restrict access to the app.");
            }
            break;
        }
        case "POST" :  
        {
            log.LogInformation("C# HTTP trigger function processed a POST request.");
            try{
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                string resp = await PostEventToBCAsync(JsonConvert.SerializeObject(new { _contract = requestBody}), log);
                log.LogInformation(resp);
                return new OkObjectResult(resp);
            }
            catch(Exception ex)
            {
                return new BadRequestObjectResult(ex.Message); 
            }
            break; 
        }
        default:
        {
            return new BadRequestObjectResult($"HTTPMethod {req.Method} is not supported!"); 
        }
    }
    
}

public static async Task<string> PostEventToBCAsync(string jsonBody, ILogger log)
{
    HttpClient client = new HttpClient(); 
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {await GetBearerTokenAsync()}");
    var data = new StringContent(jsonBody, Encoding.UTF8, "application/json");
    string postUri = $"{BASE_URI}/api/Services/SQRWebServiceGroup/SQRNotificationService/SquareNotificationService";
    log.LogInformation(postUri);
    log.LogInformation(jsonBody);
    var response = await client.PostAsync(postUri, data);
    var responseString = await response.Content.ReadAsStringAsync(); 
    return responseString; 
}
public static async Task<string> PostAuthCodeToBCAsync(string jsonBody, ILogger log)
{
    HttpClient client = new HttpClient(); 
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {await GetBearerTokenAsync()}");
    var data = new StringContent(jsonBody, Encoding.UTF8, "application/json");
    string postUri = $"{BASE_URI}/api/Services/SQRWebServiceGroup/SQRAuthorizationService/SquareAuthorizationService";
    log.LogInformation(postUri);
    log.LogInformation(data.ReadAsStringAsync().Result);
    var response = await client.PostAsync(postUri, data);
    var responseString = await response.Content.ReadAsStringAsync();
    return responseString; 
}

public static async Task<string> GetBearerTokenAsync()
{
    HttpClient client = new HttpClient();    
    string SCOPE = $"{BASE_URI}/.default";

    //client.DefaultRequestHeaders.Add("Authorization", $"Basic {EncodeTo64(CLIENT_ID + ":" + CLIENT_SECRET)}");
    var grantValues = new Dictionary<string, string>
    {
        { "grant_type", "client_credentials" },
        { "client_id", CLIENT_ID },
        { "client_secret", CLIENT_SECRET },
        { "scope", SCOPE }
    };

    var grantContent = new FormUrlEncodedContent(grantValues); 

    var response = await client.PostAsync($"https://login.microsoftonline.com/{AAD_TENANTID}/oauth2/v2.0/token", grantContent);

    var responseString = await response.Content.ReadAsStringAsync();
    dynamic TokenData = JsonConvert.DeserializeObject(responseString);

    return TokenData != null ? TokenData?.access_token : "";
}

public static string EncodeTo64(string toEncode)
{
    byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(toEncode);
    string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);
    return returnValue;
}