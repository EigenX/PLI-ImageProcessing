using System;
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
using System.Timers;


namespace ImageOverlayFunctionApp
{
    internal class ImageOverlayService
    {
        private static readonly string blobConnectionString = Environment.GetEnvironmentVariable("BlobConnection");
        private static readonly string containerName = "outputimages";


        public static async Task ApplyOverlayToAllProgramsAsync()
        {
            string connectionString = Environment.GetEnvironmentVariable("SQLServerConnection");
            string query = @"select m2.item_pk as ItemPk, m2.item_long_description as ItemLongDescription, m2.start_date as StartDate, m1.location_description as LocationDescription, m2.item_class_description as ItemClassDescription, count(m1.source_item_pk) over(partition by m1.source_item_pk) as SrcItemPkCount, m2.source_item_pk as SourceItemPk from [dbo].[mktg_item_for_image] m1 right join [dbo].[mktg_item_for_image] m2 on m1.item_pk = m2.source_item_pk";
            try
            {
                // Using Dapper for database access
                using SqlConnection connection = new(connectionString);
                var imageOverlays = connection.Query<ImageOverlay>(query);

                foreach (var overlay in imageOverlays)
                {
                    await ApplyImageOverlayAsync(overlay, true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing image: {ex.Message}");
            }
        }
        private static async Task ApplyImageOverlayAsync(ImageOverlay overlay, bool uploadToAz = false)
        {
            try
            {

                var outputFolder = @"C:\Users\SaiBussa\Pictures\output_images";
                Directory.CreateDirectory(outputFolder);

                // Define the image URL with the specified product ID
                string url = Environment.GetEnvironmentVariable("ImageUrl") + $"{overlay.ItemPk}/image";

                using HttpClient client = new();
                using Stream imageStream = await client.GetStreamAsync(url);

                // Load the base image from the stream
                using var image = new MagickImage(imageStream);
                var draw = new Drawables();
                image.Resize(new MagickGeometry(520, 240) { IgnoreAspectRatio = true });

                int rectWidth = 80;  // Width of the rectangle
                int rectHeight = 80;  // Height of the rectangle
                int cropHeight = 200;
                int xPosition = (int)(image.Width - rectWidth - 10);  // Right corner with some margin
                int yPosition = cropHeight - rectHeight - 10; // Margin from the top

                // Specify the exact format of the date string
                string format = "MM/dd/yyyy HH:mm:ss";
                DateTime parsedDate;
                parsedDate = DateTime.ParseExact(overlay.StartDate, format, CultureInfo.InvariantCulture);
                string month = parsedDate.ToString("MMM");
                string day = parsedDate.Day.ToString();

                string programTitle = overlay.ItemLongDescription;
                string Location = overlay.LocationDescription;
                string item_class = overlay.ItemClassDescription;
                string source_item_pk = overlay.SourceItemPk;
                int src_item_pk_count = overlay.SrcItemPkCount;

                // Clean the string by trimming leading/trailing spaces and replacing multiple spaces
                programTitle = programTitle.Trim();
                programTitle = System.Text.RegularExpressions.Regex.Replace(programTitle, @"\s+", " ");

                // Debugging to check the cleaned programTitle
                Console.WriteLine($"Processed Title: {programTitle}");
                Console.WriteLine($"Length of Program Title: {programTitle.Length}");

                int maxLength = 26;
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

                        if (string.IsNullOrEmpty(remainingText)) { line3 = ""; }
                        else
                        {
                            // Handle third line, check for remaining text
                            if (remainingText.Length > maxLength) { line3 = remainingText.Substring(0, maxLength).Trim() + "..."; }
                            else { line3 = remainingText.Trim(); }
                        }
                    }
                }
                else
                {
                    line1 = programTitle;
                    line2 = "";
                    line3 = "";
                }

                // Ensure none of the lines are empty before image processing
                if (string.IsNullOrEmpty(line1) && string.IsNullOrEmpty(line2) && string.IsNullOrEmpty(line3))
                {
                    Console.WriteLine("Error: All lines are empty. Skipping image processing.");
                }
                // Define a rounded rectangle
                int x = 50, y = 50, radius = 2;

                // Set up font and text overlay settingsa
                var drawables = new Drawables()
                    .StrokeWidth(2)
                    .FillColor(MagickColors.White)
                    .RoundRectangle(20, 195, 110, 217, radius, radius).FillColor(MagickColors.Black)
                    .RoundRectangle(xPosition + 20, 35, xPosition + 75, 90, radius, radius);

                // Using Dapper for database access
                string programIdRet = overlay.ItemPk;
                string locationstr = "";

                if (src_item_pk_count == 2)
                {
                    locationstr = Location.ToString() + " & Online";
                }
                else
                {
                    if (String.IsNullOrEmpty(Location) == false)
                    { locationstr = Location.ToString(); }
                    else { locationstr = "Online"; }
                }


                if (String.IsNullOrEmpty(locationstr) == false)
                {
                    drawables
                    .Font("Helvetica Neue")                     // Set font 
                    .FontPointSize(12)             // Set font size
                    .FillColor(MagickColors.White)     // Set text color
                    .Text(22, 45, locationstr);
                }

                // Add the program name (title) inside the rectangle with Arial, Bold, 24px
                drawables
                    .Font("Helvetica Neue")                     // Set font 
                    .FontPointSize(22)                     // Set font size to 24px
                    .FillColor(MagickColors.White)         // Set text color
                    .Text(22, 70, line1); // First line of the title

                // If there is a second line, add it below the first line
                if (!string.IsNullOrEmpty(line2))
                {
                    drawables
                        .Font("Helvetica Neue")                     // Set font 
                        .FontPointSize(22)                     // Set font size to 24px
                        .FillColor(MagickColors.White)
                        .Text(22, 90, line2);  // Second line of the title
                }
                if (!string.IsNullOrEmpty(line3))
                {
                    drawables
                        .Font("Helvetica Neue")                     // Set font 
                        .FontPointSize(22)                     // Set font size to 24px
                        .FillColor(MagickColors.White)
                        .Text(22, 110, line3);  // /Third line of the title
                }

                // Add text date inside the rectangle
                drawables
                    .Font("Helvetica Neue")                     // Set font 
                    .FontPointSize(12)                 // Set font size
                    .FillColor(MagickColors.White)     // Set text color
                    .Text(xPosition + 38, 60, month);
                // Add text inside the rectangle
                drawables
                    .Font("Helvetica Neue")                     // Set font 
                    .FontPointSize(12)                 // Set font size
                    .FillColor(MagickColors.White)     // Set text color
                    .Text(xPosition + 40, 73, day);
                //Add text register button
                drawables
                   .Font("Helvetica Neue-Bold")                // Set font 
                   .FillColor(MagickColors.Black)// Set font
                                                 //.StrokeColor(MagickColors.Black)
                                                 //.StrokeWidth(0.01)
                   .FontPointSize(12.65)
                   .Text(42, 210, "Register");

                // Draw text on the image
                drawables.Draw(image);

                // Define output path and save the modified image
                string outputImagePath = Path.Combine(outputFolder, $"output_{overlay.ItemPk}.jpg");
                image.Write(outputImagePath);

                if (uploadToAz)
                {
                    // Save the modified image to a MemoryStream for uploading
                    using MemoryStream memoryStream = new();
                    image.Write(memoryStream);
                    memoryStream.Position = 0;

                    // Upload the image to Azure Blob Storage
                    await UploadImageToBlobStorage(memoryStream, $"output_{overlay.ItemPk}.jpg");
                }

                Console.WriteLine($"Image saved");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing image {overlay.ItemLongDescription}: {ex.Message}");
            }
        }

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
    }
}

