//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Azure.WebJobs;
//using Microsoft.Azure.WebJobs.Extensions.Http;
//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.Logging;
//using ImageOverlayFunctionApp;
//using Newtonsoft.Json;
//using System.IO;

//public static class ImageOverlayFunction
//{
//    [FunctionName("ImageOverlayFunction")]
//    public static async Task<IActionResult> Run(
//        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
//        ILogger log)
//    {
//        log.LogInformation("Processing image overlay request with Magick.NET.");

//        ImageOverlayService.ApplyOverlayToAllProgramsAsync();

//        string name = req.Query["name"];

//        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
//        dynamic data = JsonConvert.DeserializeObject(requestBody);
//        name = name ?? data?.name;

//        string responseMessage = string.IsNullOrEmpty(name)
//                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
//                : $"Hello, {name}. This HTTP triggered function executed successfully.";

//        return new OkObjectResult(responseMessage);

      
//    }
//}
