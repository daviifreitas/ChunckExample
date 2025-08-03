# ChunkExample

A .NET 8 API for uploading files in chunks (blocks), enabling efficient and resilient transfer of large files.

## Features

- **Chunk Upload**: Allows sending large files divided into smaller blocks
- **Session Management**: Controls the state of each upload in progress
- **Cancellation**: Ability to cancel uploads in progress
- **Recovery**: Support for resuming interrupted uploads
- **RESTful API**: Simple and intuitive endpoints
- **Swagger**: Automatic API documentation

## Technologies

- .NET 8.0
- ASP.NET Core Web API
- Swagger/OpenAPI

## Endpoints

### POST `/upload-file-chunk`
Sends a file chunk to the server.

**Parameters:**
- `FileName`: File name
- `FileSize`: Total file size
- `ChunkIndex`: Current chunk index
- `TotalChunks`: Total number of chunks
- `ChunkId`: Unique upload session identifier
- `ChunkData`: Binary chunk data
- `ReferenceId`: Reference ID for organization
- `FolderId`: Destination folder ID

### POST `/cancel-file-chunk/{chunkId}`
Cancels an upload in progress and cleans up temporary files.

## How to Run

1. **Prerequisites:**
   - .NET 8.0 SDK

2. **Run the project:**
   ```bash
   cd ChunckExample
   dotnet run
   ```

3. **Access documentation:**
   - Swagger UI: `http://localhost:5119/swagger`

## How it Works

1. **Upload Start**: The client divides the file into chunks and sends each one using the `/upload-file-chunk` endpoint
2. **Temporary Storage**: Each chunk is temporarily stored on the server
3. **Progress Control**: The server maintains the state of each upload and returns progress
4. **Completion**: When all chunks are received, the file is assembled and moved to the final destination
5. **Cleanup**: Temporary files are automatically removed

## Project Structure

```
ChunckExample/
├── Dtos/
│   └── ChunkUpload/
│       ├── ChunkUploadDto.cs           # DTO for receiving chunks
│       ├── ChunkUploadResponseDto.cs   # Response DTO
│       └── ChunckUploadSession.cs      # Upload session model
├── Services/
│   └── ChunckUploadService.cs          # Main upload logic
├── Program.cs                          # Application configuration
└── ChunckExample.csproj               # Project file
```

## Storage

- **Temporary**: Chunks are stored in `%TEMP%/ChunkUploads/`
- **Final**: Complete files are saved to `wwwroot/FileRepository/{FolderId}/{ReferenceId}/`