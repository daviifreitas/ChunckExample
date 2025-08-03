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

app.MapPost("/enviar-arquivo-chunck",
        async (ChunkUploadDto request, ChunkUploadService service) =>
            Results.Ok(await service.ReceberChunk(request)))
    .WithName("Enviar chunck")
    .WithDescription("Método para enviar arquivo em blocos")
    .Produces<ChunkUploadResponseDto>()
    .ProducesProblem(StatusCodes.Status400BadRequest);


app.MapPost("/cancelar-arquivo-chunck/{chuckId}",
        async (string chuckId, ChunkUploadService service) =>
            Results.Ok(await service.CancelarUpload(chuckId)))
    .WithName("Cancelar envio chunck")
    .WithDescription("Método para cancelar o envio de arquivo em blocos")
    .Produces<ChunkUploadResponseDto>()
    .ProducesProblem(StatusCodes.Status400BadRequest);


app.Run();