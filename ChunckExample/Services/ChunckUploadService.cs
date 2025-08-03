using System.Collections.Concurrent;
using ChunckExample.Dtos.ChunkUpload;

namespace ChunckExample.Services;

public class ChunkUploadService
{
    private readonly ILogger<ChunkUploadService> _logger;
    private readonly string _tempPath;
    private static readonly ConcurrentDictionary<string, ChunkUploadSession> UploadSessions = new();
    private readonly IWebHostEnvironment _hostingEnvironment;


    public ChunkUploadService(ILogger<ChunkUploadService> logger,
        IWebHostEnvironment hostingEnvironment)
    {
        _logger = logger;
        _tempPath = Path.Combine(Path.GetTempPath(), "ChunkUploads");
        if (!Directory.Exists(_tempPath)) Directory.CreateDirectory(_tempPath);
        _hostingEnvironment = hostingEnvironment;
    }

    public async Task<ChunkUploadResponseDto> ReceiveChunk(ChunkUploadDto chunkUploadDto)
    {
        try
        {
            ChunkUploadSession session = UploadSessions.GetOrAdd(chunkUploadDto.ChunkId, _ => new ChunkUploadSession
            {
                ChunkId = chunkUploadDto.ChunkId,
                FileName = chunkUploadDto.FileName,
                FileSize = chunkUploadDto.FileSize,
                TotalChunks = chunkUploadDto.TotalChunks,
                CreatedAt = DateTime.UtcNow,
                ReceivedChunks = new ConcurrentDictionary<int, bool>(),
                ReferenceId = chunkUploadDto.ReferenceId,
                FolderId = chunkUploadDto.FolderId
            });

            var chunkPath = Path.Combine(_tempPath, chunkUploadDto.ChunkId, $"chunk_{chunkUploadDto.ChunkIndex}");
            var chunkDir = Path.GetDirectoryName(chunkPath);

            if (!Directory.Exists(chunkDir)) Directory.CreateDirectory(chunkDir);

            using (var fileStream = new FileStream(chunkPath, FileMode.Create))
            {
                await chunkUploadDto.ChunkData.CopyToAsync(fileStream);
            }

            session.ReceivedChunks[chunkUploadDto.ChunkIndex] = true;

            var chunksReceived = session.ReceivedChunks.Count;
            var isComplete = chunksReceived == session.TotalChunks;

            _logger
                .LogInformation(
                    $"Chunk {chunkUploadDto.ChunkIndex} received for file {chunkUploadDto.FileName}. {chunksReceived}/{session.TotalChunks}");

            if (isComplete) await CompleteChunk(session, session.ChunkId);

            return new ChunkUploadResponseDto
            {
                Success = true,
                Message = $"Chunk {chunkUploadDto.ChunkIndex} received successfully",
                ChunkId = chunkUploadDto.ChunkId,
                ChunksReceived = chunksReceived,
                TotalChunks = session.TotalChunks,
                IsComplete = isComplete
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                $"Error receiving chunk {chunkUploadDto.ChunkIndex} from file {chunkUploadDto.FileName}");
            return new ChunkUploadResponseDto
            {
                Success = true,
                Message = $"Chunk {chunkUploadDto.ChunkIndex} received successfully",
                ChunkId = chunkUploadDto.ChunkId,
                ChunksReceived = chunkUploadDto.ChunkIndex,
                TotalChunks = chunkUploadDto.TotalChunks,
                IsComplete = false
            };
        }
    }

    public async Task<ChunkUploadResponseDto> CancelUpload(string chunkId)
    {
        string returnMessage = string.Empty;
        bool operationOk = false;
        try
        {
            if (UploadSessions.TryRemove(chunkId, out _))
            {
                await CleanupTemporaryChunks(chunkId);
                returnMessage = $"Upload cancelled for chunk ID {chunkId}";
                operationOk = true;
                _logger.LogInformation(returnMessage);
            }
        }
        catch (Exception ex)
        {
            returnMessage = $"Error cancelling upload for chunk ID {chunkId}";
            operationOk = false;
            _logger.LogError(ex, returnMessage);
        }

        return new ChunkUploadResponseDto
        {
            Success = operationOk,
            Message = returnMessage
        };
    }

    private async Task CombineChunks(ChunkUploadSession session, Guid attachmentId)
    {
        string finalDirectory = GetFinalFilePath(session.FolderId, session.ReferenceId);

        if (!Directory.Exists(finalDirectory)) Directory.CreateDirectory(finalDirectory);

        var finalFilePath = Path.Combine(finalDirectory, $"Attachment_{attachmentId}{Path.GetExtension(session.FileName)}");

        using (var finalStream = new FileStream(finalFilePath, FileMode.Create))
        {
            for (int i = 0; i < session.TotalChunks; i++)
            {
                var chunkPath = Path.Combine(_tempPath, session.ChunkId, $"chunk_{i}");

                if (File.Exists(chunkPath))
                {
                    using (var chunkStream = new FileStream(chunkPath, FileMode.Open))
                    {
                        await chunkStream.CopyToAsync(finalStream);
                    }
                }
            }
        }
    }

    private async Task CleanupTemporaryChunks(string chunkId)
    {
        var chunkDir = Path.Combine(_tempPath, chunkId);

        if (Directory.Exists(chunkDir))
        {
            await Task.Run(() => Directory.Delete(chunkDir, true));
        }
    }

    private async Task CompleteChunk(ChunkUploadSession session, string chunkId)
    {
        try
        {
            //AttachmentId can be used to insert a database record for file reference!
            Guid attachmentId = Guid.NewGuid();

            await CombineChunks(session, attachmentId);

            await CleanupTemporaryChunks(chunkId);

            UploadSessions.TryRemove(chunkId, out _);

            _logger.LogInformation($"File upload {session.FileName} completed successfully");

            return;
        }
        catch
        {
            return;
        }
    }

    private string GetFinalFilePath(Guid folderId, Guid referenceId)
        => Path.Combine(Path.Combine(_hostingEnvironment.WebRootPath, "FileRepository", folderId.ToString()),
            referenceId.ToString());
}