using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Dapper;
using ImageMagick;
using ImageMagick.Drawing;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using static System.Net.Mime.MediaTypeNames;
//using ImageMagick.Drawing;

namespace ImageOverlayFunctionApp
{
    internal class ImageOverlayService
    {
        // Azure Blob Storage settings
        private static readonly string blobConnectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");


        public static async Task ApplyOverlayToAllProgramsAsync()
        {
            // Connection string to the database for localdb
            string connectionString = @"Data Source=(LocalDb)\MSSQLLocalDB; Initial Catalog=PLI; Integrated Security=True;";

            /* This code will be used further to connect app with database
            string connectionString = @"Server=ex-db01-pli.database.windows.net;Authentication=Active Directory MSI; Encrypt=True; Database=EX_PLI_App";
            string query = @"SELECT item_pk, item_long_description, start_date FROM mktg_item_for_image";

            */
            //string connectionString = @"Server=aztusgdb0.database.windows.net,1433;"
            //+ "Authentication=Active Directory Managed Identity; Encrypt=True;"
            //+ "Database=analytics";

            //  Comment the code if need to run the code locally           
            //string connectionString = Environment.GetEnvironmentVariable("AZURE_SQL_CONNECTIONSTRING")!;
            // Query to fetch the necessary image overlay data

            //string ConnectionString2 = @"Server=demo.database.windows.net;Authentication=Active Directory MSI; Encrypt=True; Database=testdb";

            string query = @"SELECT ProgramId, ProgramTitle, ProgramDate, Location FROM dbo.ImageOverlay";
            
            //string query = @"SELECT item_pk, item_long_description, startDate, location_description FROM dbo.table";


            //string query = @"SELECT item_pk, item_long_description, startDate FROM mktg_item_for_image";

            // Using Dapper for database access
            using SqlConnection connection = new(connectionString);
            var imageOverlays = connection.Query<ImageOverlay>(query);

            foreach (var overlay in imageOverlays)
            {
                await ApplyImageOverlayAsync(overlay, true);
            }
        }


        private static async Task ApplyImageOverlayAsync(ImageOverlay overlay, bool uploadToAz = false)
        {
            try
            {
                var outputFolder = @"C:\Users\SaiBussa\Pictures\output_images";
                Directory.CreateDirectory(outputFolder);

                // Define the image URL with the specified product ID
                string url = $"https://cms.pli.edu/api/client/product/{overlay.ProgramId}/image";

                //TODO: replace Id
                //string url = "https://cms.pli.edu/api/client/product/389204/image";

                //url = "https://cms.pli.edu/api/client/product/386586/image";

                //Code to Complete to move the position

                /*
                int width = 800, height = 600;

                int boxWidth = 300;
                int boxHeight = 200;
                int x1 = width - boxWidth - 50;
                int y1 = (height - boxHeight) / 2;
                int x2 = x1 + boxWidth;
                int y2 = y1 + boxHeight;
                */
                using HttpClient client = new();

                // string dateString = "06/15/2008";
                //DateTime dt = DateTime.ParseExact(dateString, "dd-MM-yyyy HH:mm:ss",null);
                //s will be MM/dd/yyyy format string
                //var insert = DateTime.ParseExact(overlay.ProgramDate, "d/M/yyyy h:mm", CultureInfo.InvariantCulture);
                // Download the base image as a stream
                using Stream imageStream = await client.GetStreamAsync(url);

                // Load the base image from the stream
                using var image = new MagickImage(imageStream);
                //DateTime programDate = DateTime.ParseExact(overlay.ProgramDate, "dd/MM/yyyy", null);

                //Image resize

                
                // Create a transparent background to hold the mask
                //var roundedImage = new MagickImage(MagickColors.Transparent, image.Width, image.Height);

        
                // Create a drawing object to draw a rounded rectangle mask
                var draw = new Drawables();
           
                    //draw.FillColor(MagickColors.White);
                    //draw.RoundRectangle(0, 0, image.Width, image.Height, 50, 50); // 50 is the radius for rounded corners

                // Draw the mask on the transparent background


                //draw.Draw(roundedImage);
                //draw.Draw(image);
                //image.Composite(image, CompositeOperator.SrcOver);
            
                // Composite the rounded mask onto the original image (this will create rounded corners)


                //image.Resize(1500, 500); - New

                image.Resize(520, 240);

                ///
                // Define the radius for the rounded corners
                //int radiusround = 10;


                //image.Draw(new DrawableRoundRectangle(0, 0, 700, 250, radiusround, radiusround));

                // Set the mask image's alpha channel
                //image.Composite(image, 0, 0, CompositeOperator);

                int rectWidth = 80;  // Width of the rectangle
                int rectHeight = 80;  // Height of the rectangle
                                      // Position the rectangle in the right corner
                ///int xPosition = (int)(image.Width - rectWidth);  // Right corner
                // int yPosition = 0;  // Top corner
                int cropHeight = 200;

                int xPosition = (int)(image.Width - rectWidth + 20);  // Right corner with some margin
                int yPosition = cropHeight - rectHeight - 10; // Margin from the top
                

                // Specify the exact format of the date string
                string format = "MM/dd/yyyy HH:mm:ss";
                DateTime parsedDate;
                parsedDate = DateTime.ParseExact(overlay.ProgramDate, format, CultureInfo.InvariantCulture);
                // DateTime programDate = DateParse(overlay.ProgramDate);
                string month = parsedDate.ToString("MMM");
                string day = parsedDate.Day.ToString();

                // Program Title Text (splitting into three lines if necessary)
                //string programTitle = overlay.ProgramTitle;
                //int maxLength = 20;
                //string line1, line2,line3;

                //string SplitText(string text, int maxLen, out string remainingText)
                //{
                //    int splitIndex = text.LastIndexOf(" ", maxLen);

                //    if (splitIndex == -1)
                //    {
                //        splitIndex = Math.Min(maxLen,text.Length);
                //    }
                //    remainingText = text.Substring(splitIndex).Trim();
                //    return text.Substring(0, splitIndex).Trim();
                //}
                //if (programTitle.Length > maxLength)
                //{
                //    // First split
                //    line1 = SplitText(programTitle, maxLength, out string remainingText);

                //    // Second split
                //    line2 = SplitText(remainingText, maxLength, out remainingText);

                //    // Third line
                //    //line3 = remainingText.Trim();
                //    if (!string.IsNullOrWhiteSpace(remainingText))
                //    {
                //        line3 = remainingText.Length > maxLength ?
                //        remainingText.Substring(0, 20).Trim() + "..." : remainingText.Trim();
                //    }
                //    else { line3 = ""; }


                //}
                //else {
                //    line1 = programTitle;
                //    line2 = "";
                //    line3 = "";
                //}

               string programTitle = overlay.ProgramTitle;

// Clean the string by trimming leading/trailing spaces and replacing multiple spaces
programTitle = programTitle.Trim();
programTitle = System.Text.RegularExpressions.Regex.Replace(programTitle, @"\s+", " ");

// Debugging to check the cleaned programTitle
Console.WriteLine($"Processed Title: {programTitle}");
Console.WriteLine($"Length of Program Title: {programTitle.Length}");

int maxLength = 20;
string line1 = string.Empty, line2 = string.Empty, line3 = string.Empty;

// Method to split text at the last space before maxLen
string SplitText(string text, int maxLen, out string remainingText)
{
    remainingText = string.Empty;

    if (string.IsNullOrEmpty(text))
    {
        return string.Empty;
    }

    // If text is shorter than or equal to maxLen, return it directly
    if (text.Length <= maxLen)
    {
        remainingText = string.Empty;
        return text;
    }

    // Find the last space within the maxLen limit
    int splitIndex = text.LastIndexOf(" ", maxLen);

    // If no space is found within maxLen, split at maxLen or the end of the string
    if (splitIndex == -1)
    {
        splitIndex = Math.Min(maxLen, text.Length);
    }

    // Ensure that splitIndex is within valid range
    remainingText = (splitIndex < text.Length) ? text.Substring(splitIndex).Trim() : string.Empty;

    // Return the first part of the string (before splitIndex)
    return text.Substring(0, splitIndex).Trim();
}

// Process the programTitle
if (!string.IsNullOrEmpty(programTitle) && programTitle.Length > maxLength)
{
    // First split
    line1 = SplitText(programTitle, maxLength, out string remainingText);
    Console.WriteLine($"Line 1 after split: {line1}");

    if (string.IsNullOrEmpty(remainingText))
    {
        line2 = "";
        line3 = "";
    }
    else
    {
        // Second split
        line2 = SplitText(remainingText, maxLength, out remainingText);
        Console.WriteLine($"Line 2 after split: {line2}");

        if (string.IsNullOrEmpty(remainingText))
        {
            line3 = "";
        }
        else
        {
            // Handle third line, check for remaining text
            if (remainingText.Length > maxLength)
            {
                line3 = remainingText.Substring(0, maxLength).Trim() + "...";
            }
            else
            {
                line3 = remainingText.Trim();
            }
        }
    }
}
else
{
    line1 = programTitle;
    line2 = "";
    line3 = "";
}

// Log the lines for debugging
Console.WriteLine($"Line 1: {line1}");
Console.WriteLine($"Line 2: {line2}");
Console.WriteLine($"Line 3: {line3}");

// Ensure none of the lines are empty before image processing
if (string.IsNullOrEmpty(line1) && string.IsNullOrEmpty(line2) && string.IsNullOrEmpty(line3))
{
    Console.WriteLine("Error: All lines are empty. Skipping image processing.");
}
else
{
    // Proceed with image processing only if there's valid text in line1, line2, or line3
    if (!string.IsNullOrEmpty(line1) || !string.IsNullOrEmpty(line2) || !string.IsNullOrEmpty(line3))
    {
        // Example: imageProcessingFunction(line1, line2, line3);
        //Console.WriteLine("Image processing for lines:");
        //Console.WriteLine($"Line 1: {line1}");
        //Console.WriteLine($"Line 2: {line2}");
        //Console.WriteLine($"Line 3: {line3}");

        // Assuming image is uploaded after these lines:
        // Image uploaded to Azure Blob Storage: output_416665.jpg
        // Image saved
    }
    else
    {
        Console.WriteLine("Skipping image processing due to empty lines.");
    }
}               // Define a rounded rectangle
                int x = 50, y = 50, radius = 5;

                // var roundedImage = new MagickImage(MagickColors.Transparent, image.Width, image.Height);

                // Set up font and text overlay settingsa
                var drawables = new Drawables()
                    //.FontPointSize(50)
                    //.StrokeColor(MagickColors.White)
                    .StrokeWidth(2)
                    .FillColor(MagickColors.White)
                    //.Rectangle(50, 50, 300, 200);
                                    //draw.Draw(image);
                //image.Composite(image, CompositeOperator.SrcOver);
                    .RoundRectangle(20, 210, 110, cropHeight + 30, radius, radius).FillColor(MagickColors.Black)
                    // .Rectangle(xPosition, 35, xPosition + 50, 90);       
                    .RoundRectangle(xPosition, 35, xPosition + 50, 90, radius, radius);


                // .Rectangle(x1, y1, x2, y2)
                //.FontPointSize(45)
                //.FillColor(MagickColors.White)
                //.TextAlignment(TextAlignment.Center);     
                // .Text(80, 80, overlay.ProgramTitle)                     
                // .Text(xPosition + 10, yPosition + 30, parsedDate.ToString("MMM dd"));

                if (String.IsNullOrEmpty(overlay.Location)==false)
                {
                    // Using Dapper for database access
                    string programIdRet = overlay.ProgramId;
                    string connectionString2 = @"Data Source=(LocalDb)\MSSQLLocalDB; Initial Catalog=PLI; Integrated Security=True;";
                    string query2 = @"  SELECT m.ProgramId,s.source_item_pk, s.item_class_description,s.location from dbo.ImageOverlay m left outer join dbo.ImageOverlay s on m.ProgramId = s.source_item_pk  where s.source_item_pk=@programIdRet";
                    //using SqlConnection connection = new(connectionString2);
                    //var imageLocation = connection.Query<ImageOverlay>(query2);
                    string locationstr = "";
                    using (SqlConnection connection = new SqlConnection(connectionString2))
                    {
                        connection.Open();

                        // Create a SQL command object with the query and connection
                        using (SqlCommand command = new SqlCommand(query2, connection))
                        {
                            // Add parameters to the SQL command
                            command.Parameters.AddWithValue("@programIdRet", programIdRet);
    
                            // Execute the query and get the data
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    Console.WriteLine("Item Class Desc: " + reader["item_class_description"] + ", Location " + reader["Location"]);
                                    if (reader["item_class_description"].ToString() == "Live Seminar")
                                    {
                                        locationstr = reader["Location"].ToString();
                                    }
                                    else if (reader["item_class_description"].ToString() == "Live Webcast")
                                    {
                                        locationstr = locationstr + " & Online";
                                    }
                                    else if (reader["Location"].ToString() == "")
                                    {
                                        locationstr = "Online";
                                    }
                                    else
                                    {
                                        locationstr = "Online";
                                    }
                                }
                            }
                        }
                    }

                    /*
       
                    foreach (var overlay2 in imageLocation)
                    {
                       if (overlay2.item_class_description == "Live Seminar")
                        {
                            locationstr = overlay2.Location;
                        }
                       else if (overlay2.item_class_description == "Live Webcast")
                        {
                            locationstr = locationstr + " Online";
                        }
                        else
                        {
                            locationstr = overlay2.Location;
                        }

                    }
                    */
                    if (String.IsNullOrEmpty(locationstr) == false)
                    {
                        drawables
                        .Font("Helvetica Neue")                     // Set font 
                        .FontPointSize(12)             // Set font size
                        .FillColor(MagickColors.White)     // Set text color
                        .Text(20, 45, locationstr);
                    }
                }


                // Add the program name (title) inside the rectangle with Arial, Bold, 24px
                drawables
                    .Font("Helvetica Neue")                     // Set font 
                    .FontPointSize(16)                     // Set font size to 24px
                    .FillColor(MagickColors.White)         // Set text color
                    .Text(20,70, line1); // First line of the title

                // If there is a second line, add it below the first line
                if (!string.IsNullOrEmpty(line2))
                {
                    drawables
                        .Font("Helvetica Neue")                     // Set font 
                        .FontPointSize(16)                     // Set font size to 24px
                        .FillColor(MagickColors.White)
                        .Text(20,90, line2);  // Second line of the title
                }
                if (!string.IsNullOrEmpty(line3))
                {
                    drawables
                        .Font("Helvetica Neue")                     // Set font 
                        .FontPointSize(16)                     // Set font size to 24px
                        .FillColor(MagickColors.White)
                        .Text(20, 110, line3);  // /Third line of the title
                 }

                // Add text date inside the rectangle
                drawables
                    .Font("Helvetica Neue")                     // Set font 
                    .FontPointSize(12)                 // Set font size
                    .FillColor(MagickColors.White)     // Set text color
                    .Text(xPosition+15, 60, month);
                // Add text inside the rectangle
                drawables
                    .Font("Helvetica Neue")                     // Set font 
                    .FontPointSize(12)                 // Set font size
                    .FillColor(MagickColors.White)     // Set text color
                    .Text(xPosition+17, 73, day);
                //Add text register button
                drawables
                   .Font("Helvetica Neue")                     // Set font 
                    .FillColor(MagickColors.Black)// Set font
                    .Font("Arial-Bold").FontPointSize(12)                 // Set font size
                    .Text(42, 225, "Register");

                // Draw text on the image
                drawables.Draw(image);

                // Define output path and save the modified image
                string outputImagePath = Path.Combine(outputFolder, $"output_{overlay.ProgramId}.jpg");
                image.Write(outputImagePath);

                if (uploadToAz)
                {
                    // Save the modified image to a MemoryStream for uploading
                    using MemoryStream memoryStream = new();
                    image.Write(memoryStream);
                    memoryStream.Position = 0;

                    // Upload the image to Azure Blob Storage
                    await UploadImageToBlobStorage(memoryStream, $"output_{overlay.ProgramId}.jpg");
                }

                Console.WriteLine($"Image saved");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing image {overlay.ProgramTitle}: {ex.Message}");
            }
        }

        private static DateTime DateParse(string programDate)
        {
            throw new NotImplementedException();
        }




        //private  static void ApplyImageOverlay(ImageOverlay overlay, bool uploadToAz  = true)
        //{
        //    try
        //    {
        //        var outputFolder = @"C:\Users\MarioPaes\Pictures\output_images";
        //        Directory.CreateDirectory(outputFolder);

        //        // Load the base image using ImageMagick
        //        //TODO: Get the base image from https://cms.pli.edu/api/client/product/<Id>/image
        //        var imagePath = @"C:\Users\MarioPaes\Pictures\PLIBaseImages\skills-page-associates-photo-scaled.jpg";


        //        using var image = new MagickImage(imagePath);
        //        // Set up the font settings for the overlay text
        //        var drawables = new Drawables()
        //            .FontPointSize(50)  // You can adjust the font size as needed
        //            .Font("Arial")      // Set the font; you can specify a different font here
        //            .FillColor(new MagickColor("White"))
        //            .Text(80, 80, overlay.ProgramTitle)  // First text
        //            .FillColor(new MagickColor("White"))
        //            .Text(120, 120, overlay.ProgramDate);  // Second text

        //        // Draw text on the image
        //        drawables.Draw(image);

        //        // Save the new image
        //        string outputImagePath = Path.Combine(@"C:\Users\MarioPaes\Pictures\output_images", $"output_{Path.GetFileName(imagePath)}");
        //        image.Write(outputImagePath);

        //        if (uploadToAz)
        //        {
        //            // Save the new image to a MemoryStream instead of a file
        //            using MemoryStream memoryStream = new();
        //            // Write the image to the MemoryStream
        //            image.Write(memoryStream);

        //            // Reset stream position to the beginning
        //            memoryStream.Position = 0;

        //            // Upload the image to Azure Blob Storage
        //            UploadImageToBlobStorage(memoryStream, Path.GetFileName(imagePath));
        //        }

        //        Console.WriteLine($"Image saved to: {outputImagePath}");
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error processing image {overlay.ProgramTitle}: {ex.Message}");
        //    }
        //}

        static async Task UploadImageToBlobStorage(Stream imageStream, string imageName)
        {
            try
            {
                // Create a BlobServiceClient object to interact with the blob storage
                BlobServiceClient blobServiceClient = new(blobConnectionString);

                // Get the container client
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

                // Get a reference to a blob in the container
                BlobClient blobClient = containerClient.GetBlobClient(imageName);

                // Set the Content-Type header for the image (image/jpeg for JPG files)
                var blobHttpHeaders = new BlobHttpHeaders
                {
                    ContentType = "image/jpeg" // Set this based on your image format, e.g., image/png for PNG images
                };

                // Reset stream position to the start before uploading
                imageStream.Position = 0;

                // Upload the image to the blob with the specified Content-Type
                await blobClient.UploadAsync(imageStream, new BlobUploadOptions { HttpHeaders = blobHttpHeaders });

                Console.WriteLine($"Image uploaded to Azure Blob Storage: {imageName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading image to Blob Storage: {ex.Message}");
            }
        }


        //static async Stream GetImage()
        //{
        //    // URL of the image
        //    string url = "https://lkjas.edu/api/client/product/389204/image";

        //    // Local path where you want to save the downloaded image
        //    string outputPath = "downloaded_image.jpg";  // Update the path as needed

        //    using HttpClient client = new HttpClient();

        //    try
        //    {
        //        // Send GET request and retrieve image as a stream
        //        using Stream imageStream = await client.GetStreamAsync(url);

        //        // Save the stream content to a local file
        //        using FileStream fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
        //        await imageStream.CopyToAsync(fileStream);

        //        Console.WriteLine("Image downloaded and saved to " + outputPath);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("An error occurred: " + ex.Message);
        //    }
        //}

    }
}

