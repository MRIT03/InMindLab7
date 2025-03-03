using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using InMindLab7.Data;

[Route("api/[controller]")]
[ApiController]
public class ProfilePictureController : ControllerBase
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly UniversityContext _context;

    public ProfilePictureController(BlobServiceClient blobServiceClient, UniversityContext context)
    {
        _blobServiceClient = blobServiceClient;
        _context = context;
    }

    [HttpPost("{studentId}/upload")]
    public async Task<IActionResult> Upload(int studentId, IFormFile file)
    {
        // 1. Validate the file
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file was uploaded.");
        }

        // 2. validate student
        var student = await _context.Students.FindAsync(studentId);
        if (student == null)
        {
            return NotFound($"Student with ID {studentId} not found.");
        }

        // 3. Get a reference to the container
        //    Create the container if it doesn't exist
        var containerName = "profile-pictures";
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

        // 4. Generate a unique blob name (GUid looks ugly but is actually so cool)
        var extension = System.IO.Path.GetExtension(file.FileName);
        var blobName = $"{studentId}_{Guid.NewGuid()}{extension}";

        // 5. Upload to Blob Storage
        var blobClient = containerClient.GetBlobClient(blobName);
        using (var stream = file.OpenReadStream())
        {
            await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
        }

        // 6. Update the student's ProfilePictureBlobName in the DB
        student.ProfilePictureBlobName = blobName;
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Upload successful!", BlobName = blobName });
    }
    [HttpGet("{studentId}/download")]
    public async Task<IActionResult> Download(int studentId)
    {
        // 1. We validate the student
        var student = await _context.Students.FindAsync(studentId);
        if (student == null || string.IsNullOrEmpty(student.ProfilePictureBlobName))
        {
            return NotFound("Student or profile picture not found.");
        }

        // 2. Then we connect to the correct blob reference
        var containerName = "profile-pictures";
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(student.ProfilePictureBlobName);

        // 3. Check if the blob exists
        if (!await blobClient.ExistsAsync())
        {
            return NotFound("Blob not found in storage.");
        }

        // 4. Download the blob to a stream
        var download = await blobClient.DownloadAsync();
        return File(download.Value.Content, download.Value.Details.ContentType ?? "application/octet-stream"); // some random binary default (cringe but good practice ??)
    }

    
}
