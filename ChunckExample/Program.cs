using ChunckExample.Dtos.ChunkUpload;
using ChunckExample.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ChunkUploadService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/upload-file-chunk",
        async (ChunkUploadDto request, ChunkUploadService service) =>
            Results.Ok(await service.ReceiveChunk(request)))
    .WithName("Upload chunk")
    .WithDescription("Method to upload file in chunks")
    .Produces<ChunkUploadResponseDto>()
    .ProducesProblem(StatusCodes.Status400BadRequest);


app.MapPost("/cancel-file-chunk/{chunkId}",
        async (string chunkId, ChunkUploadService service) =>
            Results.Ok(await service.CancelUpload(chunkId)))
    .WithName("Cancel chunk upload")
    .WithDescription("Method to cancel file chunk upload")
    .Produces<ChunkUploadResponseDto>()
    .ProducesProblem(StatusCodes.Status400BadRequest);


app.Run();