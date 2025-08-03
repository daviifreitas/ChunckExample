using System.Collections.Concurrent;

namespace ChunckExample.Dtos.ChunkUpload;

public class ChunkUploadSession
{
    public string ChunkId { get; set; }
    public string FileName { get; set; }
    public long FileSize { get; set; }
    public int TotalChunks { get; set; }
    public DateTime CreatedAt { get; set; }
    public ConcurrentDictionary<int, bool> ReceivedChunks { get; set; }
    public Guid ReferenceId { get; set; }
    public Guid FolderId { get; set; }
}
