# ChunkExample

Uma API .NET 8 para upload de arquivos em chunks (blocos), permitindo o envio de arquivos grandes de forma eficiente e resiliente.

## Características

- **Upload em Chunks**: Permite o envio de arquivos grandes divididos em blocos menores
- **Gestão de Sessões**: Controla o estado de cada upload em progresso
- **Cancelamento**: Possibilidade de cancelar uploads em andamento
- **Recuperação**: Suporte para retomada de uploads interrompidos
- **API RESTful**: Endpoints simples e intuitivos
- **Swagger**: Documentação automática da API

## Tecnologias

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

## Como Executar

1. **Pré-requisitos:**
   - .NET 8.0 SDK

2. **Executar o projeto:**
   ```bash
   cd ChunckExample
   dotnet run
   ```

3. **Acessar a documentação:**
   - Swagger UI: `http://localhost:5119/swagger`

## Funcionamento

1. **Início do Upload**: O cliente divide o arquivo em chunks e envia cada um usando o endpoint `/enviar-arquivo-chunck`
2. **Armazenamento Temporário**: Cada chunk é armazenado temporariamente no servidor
3. **Controle de Progresso**: O servidor mantém o estado de cada upload e retorna o progresso
4. **Finalização**: Quando todos os chunks são recebidos, o arquivo é montado e movido para o destino final
5. **Limpeza**: Arquivos temporários são removidos automaticamente

## Estrutura do Projeto

```
ChunckExample/
├── Dtos/
│   └── ChunkUpload/
│       ├── ChunkUploadDto.cs           # DTO para receber chunks
│       ├── ChunkUploadResponseDto.cs   # DTO de resposta
│       └── ChunckUploadSession.cs      # Modelo de sessão de upload
├── Services/
│   └── ChunckUploadService.cs          # Lógica principal de upload
├── Program.cs                          # Configuração da aplicação
└── ChunckExample.csproj               # Arquivo de projeto
```

## Armazenamento

- **Temporário**: Chunks são armazenados em `%TEMP%/ChunkUploads/`
- **Final**: Complete files are saved to `wwwroot/FileRepository/{FolderId}/{ReferenceId}/`