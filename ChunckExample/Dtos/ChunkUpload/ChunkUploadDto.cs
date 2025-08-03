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
        public System.Guid ReferenciaId { get; set; }
        public System.Guid PastaId { get; set; }
    }
}