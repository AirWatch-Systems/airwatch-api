# AirWatch API (Backend) – .NET 7 (sem Docker) LU30mar03*

API RESTful do Sistema de Monitoramento da Qualidade do Ar. Este backend fornece autenticação com 2FA, endpoints de poluição do ar (OpenWeatherMap), pesquisas e geocodificação (Google), CRUD de feedbacks, histórico do usuário, documentação via Swagger e persistência no SQL Server, seguindo as especificações do projeto.

Sumário
- Visão geral e arquitetura
- Requisitos
- Estrutura de pastas
- Configuração (variáveis de ambiente e appsettings)
- Primeira execução (migrations + run)
- Uso (Swagger e exemplos de chamadas)
- Fluxo de autenticação com 2FA
- Integrações externas
- Logs e observabilidade
- Deploy sem Docker (Kestrel e IIS)
- Segurança e boas práticas
- Solução de problemas
- Bibliotecas e ferramentas necessárias

Visão geral e arquitetura
- Framework: .NET 7 (C#)
- ORM: Entity Framework Core 7 (SQL Server)
- Autenticação: JWT + 2FA (via Firebase Admin ou modo demo com código em log)
- Documentação: Swagger/OpenAPI
- Logs: Serilog (console + arquivo)
- Integrações:
  - OpenWeatherMap Air Pollution API (dados de poluentes)
  - Google Maps Geocoding API (geocodificação/pesquisa)
  - Firebase (Auth/FCM para 2FA e notificações push futuras)
- Padrões:
  - Controllers (endpoints REST)
  - Services/Repositories (lógica e dados)
  - DTOs (separação de contratos)
  - Middleware (tratamento de erros global simples)

Requisitos
- .NET 7 SDK instalado
- SQL Server (Developer/Express/LocalDB) ou SQL Server 2022+
- Node/Expo não são necessários para o backend, apenas para o app mobile
- Acesso à internet para chamadas às APIs externas
- Chaves/API configuradas (OpenWeatherMap, Google, Firebase)

Estrutura de pastas
- airwatch-api/
  - .gitignore
  - README.md (este arquivo)
  - AirWatch.Api/
    - AirWatch.Api.csproj
    - Program.cs
    - Controllers/
      - AuthController.cs
      - FeedbacksController.cs
      - LocationsController.cs
      - PollutionController.cs
      - UserController.cs
    - DTOs/
      - Contracts.cs
    - Models/
      - Entities.cs
      - AirWatchDbContext.cs
    - Repositories/
      - Repositories.cs
    - Middleware/
    - External/
    - Migrations/ (gerado após Add-Migration)
    - Config/

Configuração
1) Variáveis de ambiente (recomendado)
Defina as seguintes variáveis antes de executar em desenvolvimento/produção:

- DATABASE_CONNECTION_STRING: string de conexão do SQL Server.
  Exemplo (Windows com Trusted_Connection):
  Server=localhost;Database=AirWatch;Trusted_Connection=True;TrustServerCertificate=True;
  Exemplo (SQL Auth):
  Server=localhost;Database=AirWatch;User Id=sa;Password=SuaSenhaSegura!;TrustServerCertificate=True;

- JWT_SECRET: chave secreta de no mínimo 32 caracteres (para assinar JWT).
  Exemplo: 8b9f3da2e2a64f8bb4f8a0e9e8b2f6e1-CHANGE-ME

- OPENWEATHERMAP_API_KEY: chave da API OpenWeatherMap.

- GOOGLE_MAPS_API_KEY: chave da Google Maps Platform (para Geocoding).

- FIREBASE_CREDENTIALS: conteúdo JSON do Service Account do Firebase (string completa do JSON).
  Alternativa: deixe em branco e o sistema tentará Application Default Credentials.

- ALLOWED_ORIGINS: lista separada por vírgula com origens permitidas no CORS.
  Exemplo: http://localhost:19006,http://localhost:8081

- LOG_PATH (opcional): caminho de logs do Serilog (padrão: Logs/log-.txt).

Em PowerShell (apenas para a sessão atual):
$env:DATABASE_CONNECTION_STRING = "Server=localhost;Database=AirWatch;Trusted_Connection=True;TrustServerCertificate=True;"
$env:JWT_SECRET = "MINHA_SUPER_CHAVE_DE_32+_CARACTERES_123456"
$env:OPENWEATHERMAP_API_KEY = "SUA_CHAVE_OWM"
$env:GOOGLE_MAPS_API_KEY = "SUA_CHAVE_GOOGLE"
$env:FIREBASE_CREDENTIALS = "{ ...json_do_service_account... }"
$env:ALLOWED_ORIGINS = "http://localhost:19006"

Para persistir no usuário (reabra o terminal depois):
setx DATABASE_CONNECTION_STRING "Server=localhost;Database=AirWatch;Trusted_Connection=True;TrustServerCertificate=True;"
setx JWT_SECRET "MINHA_SUPER_CHAVE_DE_32+_CARACTERES_123456"
setx OPENWEATHERMAP_API_KEY "SUA_CHAVE_OWM"
setx GOOGLE_MAPS_API_KEY "SUA_CHAVE_GOOGLE"
setx ALLOWED_ORIGINS "http://localhost:19006"

2) appsettings.json (opcional)
Você pode manter defaults em appsettings.json, porém NUNCA coloque segredos. Variáveis de ambiente têm precedência.

Exemplo minimalista de AirWatch.Api/appsettings.json:
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=AirWatch;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Secret": "NÃO_USE_EM_PRODUÇÃO_TROQUE_POR_ENV_VAR"
  },
  "OpenWeatherMap": {
    "ApiKey": "COLOQUE_POR_ENV"
  },
  "Google": {
    "MapsApiKey": "COLOQUE_POR_ENV"
  },
  "Cors": {
    "AllowedOrigins": "http://localhost:19006"
  },
  "Firebase": {
    "Credentials": "" // vazio => usa ADC ou variável de ambiente
  }
}

Primeira execução
1) Restaurar pacotes
cd AirWatch-Systems/airwatch-api/AirWatch.Api
dotnet restore

2) Certificado HTTPS de desenvolvimento (Windows)
dotnet dev-certs https --trust

3) Instalar ferramenta EF (uma vez)
dotnet tool install --global dotnet-ef
ou atualizar:
dotnet tool update --global dotnet-ef

4) Criar o banco de dados (migrations)
- Gerar migration inicial (caso ainda não exista):
dotnet ef migrations add InitialCreate
- Aplicar migrations:
dotnet ef database update

5) Executar
dotnet run

Por padrão, o ASP.NET inicia em:
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001

6) Swagger
Abra:
https://localhost:5001/swagger
ou
http://localhost:5000/swagger

Uso (exemplos de chamadas)
Atenção: substitua valores de lat/lon por coordenadas reais.

1) Cadastro (RF01)
POST /api/auth/register
Body (JSON):
{
  "name": "João",
  "email": "joao@example.com",
  "password": "minhasenha123",
  "confirmPassword": "minhasenha123"
}

Resposta 200:
{
  "userId": "GUID",
  "message": "User registered successfully"
}

2) Login + 2FA (RF02)
2.1) Login (inicia sessão e envia código 2FA para log)
POST /api/auth/login
{
  "email": "joao@example.com",
  "password": "minhasenha123"
}
Resposta:
{
  "requires2FA": true,
  "sessionId": "sess_..."
}
O código 2FA é exibido no log do servidor (modo demo). Em produção, configure o Firebase para enviar o código por push/SMS/e-mail.

2.2) Verificar 2FA
POST /api/auth/verify-2fa
{
  "sessionId": "sess_...",
  "token": "123456"
}
Resposta:
{
  "token": "JWT_AQUI",
  "expiresIn": 3600
}

3) Poluição atual (RF04)
GET /api/pollution/current?lat=-23.5505&lon=-46.6333
Header: Authorization: Bearer {JWT}
Resposta 200:
{
  "aqi": 75,
  "pollutants": {
    "pm25": 12.34,
    "pm10": 45.67,
    "co": 1.23,
    "no2": 10.5,
    "so2": 3.1,
    "o3": 22.2
  },
  "timestamp": "2025-01-01T12:00:00Z"
}

4) Histórico de poluição (24h padrão) (RF04)
GET /api/pollution/history?lat=-23.55&lon=-46.63&hours=24
Resposta:
{ "data": [ { "timestamp": "...", "aqi": 70, "pollutants": { ... } } ] }

5) Listar feedbacks por localização/tempo (RF05)
GET /api/feedbacks?lat=-23.55&lon=-46.63&radius=5&hours=12
Resposta:
{
  "feedbacks": [
    { "id":"...", "user": { "id":"...", "name":"João", "avatarUrl":null }, "rating":4, "comment":"Ar ok", "createdAt":"..." }
  ]
}

6) Criar feedback (RF06)
POST /api/feedbacks
Header: Authorization: Bearer {JWT}
{
  "lat": -23.55,
  "lon": -46.63,
  "rating": 3,
  "comment": "Pouca visibilidade"
}
Resposta:
{
  "feedbackId": "GUID",
  "message": "Feedback created successfully"
}

7) Pesquisa de localizações (RF07)
GET /api/locations/search?query=São Paulo
Resposta:
{ "results": [ { "name":"São Paulo, SP, Brasil", "lat": -23.5505, "lon": -46.6333, "placeId":"..." } ] }

8) Marcadores no mapa por bounds (RF08)
GET /api/locations/markers?bounds=-23.7,-46.8,-23.3,-46.4
Resposta:
{ "markers": [ { "lat": -23.6, "lon": -46.6, "avgAqi": 85.2, "feedbackCount": 12 } ] }

9) Histórico pessoal (RF10)
GET /api/user/history
Header: Authorization: Bearer {JWT}
Resposta:
{ "feedbacks": [ ... ], "searches": [ ... ] }

Fluxo de autenticação com 2FA (detalhe)
- Passo 1: /api/auth/login valida credenciais.
- Passo 2: um código 2FA (6 dígitos) é gerado e, no modo demo, LOGADO no servidor.
- Passo 3: o cliente chama /api/auth/verify-2fa com sessionId e token para obter o JWT.
- Produção: configure FIREBASE_CREDENTIALS e substitua o envio do código em log por push/SMS/e-mail via Firebase.

Integrações externas
- OpenWeatherMap Air Pollution API:
  - Necessário OPENWEATHERMAP_API_KEY.
  - Endpoint utilizado: data/2.5/air_pollution (retorna AQI e componentes).
  - Cache: resultados recentes são armazenados em PollutionCache.

- Google Maps Geocoding API:
  - Necessário GOOGLE_MAPS_API_KEY.
  - Endpoint: geocode/json (consulta textual → coordenadas).
  - Idioma: pt-BR.

- Firebase Admin:
  - FIREBASE_CREDENTIALS deve conter o JSON do Service Account (string).
  - Alternativa: ADC (Application Default Credentials).
  - Uso atual: habilitar envio de código 2FA/FCM no futuro; no momento, o código é logado para demo.

Logs e observabilidade
- Serilog:
  - Console + arquivo diário (Logs/log-.txt por padrão).
  - Personalize com a variável LOG_PATH.
- Erros não tratados:
  - Middleware simples captura exceções e retorna 500 com traceId.

Deploy sem Docker
1) Publicação (Release)
cd AirWatch-Systems/airwatch-api/AirWatch.Api
dotnet publish -c Release -o .\publish

2) Kestrel (Windows Service ou processo em background)
- Configure as variáveis de ambiente no servidor.
- Execute o binário da pasta publish:
.\publish\AirWatch.Api.exe
- Opcional: instale como serviço do Windows (sc create / NSSM / PowerShell).

3) IIS
- Instale o Hosting Bundle do .NET 7 no servidor IIS.
- Crie um App Pool (No Managed Code) e um Site apontando para a pasta .\publish.
- Habilite HTTPS (certificado instalado).
- Configure as variáveis de ambiente no nível do sistema ou do site (web.config/envs).
- Verifique o acesso a /swagger.

Segurança e boas práticas
- Não exponha chaves/segredos no repositório.
- Use HTTPS em produção.
- JWT_SECRET com entropia alta (32+ caracteres).
- Valide todos os inputs no backend (DataAnnotations já inclusas nos DTOs principais).
- Sanitização e logs sem dados sensíveis.
- CORS controlado via ALLOWED_ORIGINS.
- Rate limiting: recomendado configurar (não incluso por padrão).
- Atualize sempre dependências com patches de segurança.

Solução de problemas
- 2FA não chega:
  - Em modo demo, o código é logado no console/arquivo.
  - Configure FIREBASE_CREDENTIALS para produção/integração real.
- Erro ao acessar OWM/Google:
  - Verifique as variáveis OPENWEATHERMAP_API_KEY / GOOGLE_MAPS_API_KEY.
  - Confirme permissão de saída do servidor.
- HTTPS falhando em dev:
  - Rode: dotnet dev-certs https --trust
- Migrations:
  - dotnet ef migrations add NovaMudanca
  - dotnet ef database update
- SQL Server:
  - Teste a conexão com a sua string de conexão; use TrustServerCertificate=True em dev, se necessário.

Bibliotecas e ferramentas necessárias
- Pacotes (já referenciados no projeto):
  - Microsoft.EntityFrameworkCore 7.0.17
  - Microsoft.EntityFrameworkCore.SqlServer 7.0.17
  - Microsoft.EntityFrameworkCore.Tools 7.0.17
  - Microsoft.EntityFrameworkCore.Design 7.0.17
  - Swashbuckle.AspNetCore 6.5.0
  - Microsoft.AspNetCore.Authentication.JwtBearer 7.0.20
  - BCrypt.Net-Next 4.0.3
  - Serilog.AspNetCore 7.0.0
  - Serilog.Sinks.Console 5.0.1
  - Serilog.Sinks.File 5.0.0
  - FirebaseAdmin 2.4.0

- Ferramentas (instalar localmente, se ainda não tiver):
  - EF Core CLI:
    dotnet tool install --global dotnet-ef
    ou atualizar:
    dotnet tool update --global dotnet-ef

Notas finais
- Este backend foi projetado para funcionar sem Docker. Utilize Kestrel direto ou IIS.
- A documentação interativa está em /swagger.
- Para integrar com o app mobile, aponte EXPO_PUBLIC_API_URL no frontend para a URL do backend (ex.: https://seu-servidor:5001).

Bom desenvolvimento!