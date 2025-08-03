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

### POST `/enviar-arquivo-chunck`
Envia um chunk de arquivo para o servidor.

**Parâmetros:**
- `FileName`: Nome do arquivo
- `FileSize`: Tamanho total do arquivo
- `ChunkIndex`: Índice do chunk atual
- `TotalChunks`: Total de chunks do arquivo
- `ChunkId`: Identificador único da sessão de upload
- `ChunkData`: Dados binários do chunk
- `ReferenciaId`: ID de referência para organização
- `PastaId`: ID da pasta de destino

### POST `/cancelar-arquivo-chunck/{chuckId}`
Cancela um upload em progresso e limpa os arquivos temporários.

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
- **Final**: Arquivos completos são salvos em `wwwroot/ArquivosRepositorio/{PastaId}/{ReferenciaId}/`