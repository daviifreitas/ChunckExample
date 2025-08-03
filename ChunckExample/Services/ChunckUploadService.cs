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

    public async Task<ChunkUploadResponseDto> ReceberChunk(ChunkUploadDto chunkUploadDto)
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
                ReferenciaId = chunkUploadDto.ReferenciaId,
                PastaId = chunkUploadDto.PastaId
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
                    $"Chunk {chunkUploadDto.ChunkIndex} recebido para arquivo {chunkUploadDto.FileName}. {chunksReceived}/{session.TotalChunks}");

            if (isComplete) await CompletarChunck(session, session.ChunkId);

            return new ChunkUploadResponseDto
            {
                Success = true,
                Message = $"Chunk {chunkUploadDto.ChunkIndex} recebido com sucesso",
                ChunkId = chunkUploadDto.ChunkId,
                ChunksReceived = chunksReceived,
                TotalChunks = session.TotalChunks,
                IsComplete = isComplete
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                $"Erro ao receber chunk {chunkUploadDto.ChunkIndex} do arquivo {chunkUploadDto.FileName}");
            return new ChunkUploadResponseDto
            {
                Success = true,
                Message = $"Chunk {chunkUploadDto.ChunkIndex} recebido com sucesso",
                ChunkId = chunkUploadDto.ChunkId,
                ChunksReceived = chunkUploadDto.ChunkIndex,
                TotalChunks = chunkUploadDto.TotalChunks,
                IsComplete = false
            };
        }
    }

    public async Task<ChunkUploadResponseDto> CancelarUpload(string chunkId)
    {
        string mensagemRetorno = string.Empty;
        bool operacaoOk = false;
        try
        {
            if (UploadSessions.TryRemove(chunkId, out _))
            {
                await LimparChunksTemporarios(chunkId);
                mensagemRetorno = $"Upload cancelado para chunk ID {chunkId}";
                operacaoOk = true;
                _logger.LogInformation(mensagemRetorno);
            }
        }
        catch (Exception ex)
        {
            mensagemRetorno = $"Erro ao cancelar upload do chunk ID {chunkId}";
            operacaoOk = false;
            _logger.LogError(ex, mensagemRetorno);
        }

        return new ChunkUploadResponseDto
        {
            Success = operacaoOk,
            Message = mensagemRetorno
        };
    }

    private async Task CombinarChunks(ChunkUploadSession session, Guid anexoId)
    {
        string diretorioFinal = ObterPathFinalArquivo(session.PastaId, session.ReferenciaId);

        if (!Directory.Exists(diretorioFinal)) Directory.CreateDirectory(diretorioFinal);

        var finalFilePath = Path.Combine(diretorioFinal, $"Anexo_{anexoId}{Path.GetExtension(session.FileName)}");

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

    private async Task LimparChunksTemporarios(string chunkId)
    {
        var chunkDir = Path.Combine(_tempPath, chunkId);

        if (Directory.Exists(chunkDir))
        {
            await Task.Run(() => Directory.Delete(chunkDir, true));
        }
    }

    private async Task CompletarChunck(ChunkUploadSession session, string chunckId)
    {
        try
        {
            //AnexoId pode ser utilizado para fazer alguma inserção no banco para referência do arquivo em questão!
            Guid anexoId = Guid.NewGuid();

            await CombinarChunks(session, anexoId);

            await LimparChunksTemporarios(chunckId);

            UploadSessions.TryRemove(chunckId, out _);

            _logger.LogInformation($"Upload do arquivo {session.FileName} completado com sucesso");

            return;
        }
        catch
        {
            return;
        }
    }

    private string ObterPathFinalArquivo(Guid pastaId, Guid referenciaId)
        => Path.Combine(Path.Combine(_hostingEnvironment.WebRootPath, "ArquivosRepositorio", pastaId.ToString()),
            referenciaId.ToString());
}