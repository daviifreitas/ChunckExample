namespace ChunckExample.Dtos.ChunkUpload
{
    public class ChunkUploadDto
    {
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public int ChunkIndex { get; set; }
        public int TotalChunks { get; set; }
        public string ChunkId { get; set; }
        public IFormFile ChunkData { get; set; }
        public System.Guid ReferenceId { get; set; }
        public System.Guid FolderId { get; set; }
    }
}