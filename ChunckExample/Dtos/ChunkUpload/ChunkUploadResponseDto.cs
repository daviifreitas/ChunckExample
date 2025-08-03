namespace ChunckExample.Dtos.ChunkUpload
{
    public class ChunkUploadResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string ChunkId { get; set; }
        public int ChunksReceived { get; set; }
        public int TotalChunks { get; set; }
        public bool IsComplete { get; set; }
        public string FileId { get; set; }
        public string FilePath { get; set; }
    }
}