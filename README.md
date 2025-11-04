# AirWatch API - Sistema de Monitoramento da Qualidade do Ar

## 📋 Visão Geral

API RESTful desenvolvida em .NET 7 para monitoramento da qualidade do ar com autenticação 2FA, integração com APIs externas (OpenWeatherMap e Google Maps), sistema de feedbacks com validação temporal e documentação completa via Swagger.

### 🚀 Funcionalidades Principais
- ✅ Autenticação JWT com 2FA
- 🌬️ Dados de qualidade do ar em tempo real
- 📍 Busca e geocodificação de localizações
- 💬 Sistema de feedbacks com validação de 4 horas por região
- 📊 Histórico de dados e estatísticas
- 📚 Documentação interativa (Swagger)
- 🔒 Validação de entrada e segurança

## 📚 Sumário

- [🛠️ Requisitos e Instalação](#️-requisitos-e-instalação)
- [📱 Arquitetura e Tecnologias](#-arquitetura-e-tecnologias)
- [📁 Estrutura do Projeto](#-estrutura-do-projeto)
- [⚙️ Configuração](#️-configuração)
- [🚀 Primeira Execução](#-primeira-execução)
- [📝 Uso da API](#-uso-da-api)
- [🔐 Autenticação 2FA](#-autenticação-2fa)
- [🔗 Integrações Externas](#-integrações-externas)
- [📦 Deploy](#-deploy)
- [🔒 Segurança](#-segurança)
- [🔧 Solução de Problemas](#-solução-de-problemas)

## 🛠️ Requisitos e Instalação

### 📋 Pré-requisitos

| Ferramenta | Versão | Link de Download |
|------------|--------|-----------------|
| **.NET SDK** | 7.0+ | [Download .NET 7](https://dotnet.microsoft.com/download/dotnet/7.0) |
| **SQL Server** | 2019+ | [SQL Server Express](https://www.microsoft.com/sql-server/sql-server-downloads) |
| **Visual Studio** | 2022+ (opcional) | [Visual Studio Community](https://visualstudio.microsoft.com/downloads/) |
| **Git** | Qualquer | [Git SCM](https://git-scm.com/downloads) |

### 🔧 Instalação do .NET 7

1. **Windows:**
   - Baixe o instalador do [site oficial](https://dotnet.microsoft.com/download/dotnet/7.0)
   - Execute o instalador e siga as instruções
   - Verifique: `dotnet --version`

2. **macOS:**
   ```bash
   brew install dotnet
   ```

3. **Linux (Ubuntu):**
   ```bash
   wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
   sudo dpkg -i packages-microsoft-prod.deb
   sudo apt-get update
   sudo apt-get install -y dotnet-sdk-7.0
   ```

### 🗄️ Instalação do SQL Server

1. **SQL Server Express (Gratuito):**
   - Baixe: [SQL Server Express](https://www.microsoft.com/sql-server/sql-server-downloads)
   - Execute o instalador
   - Escolha "Instalação Básica"
   - Anote a string de conexão fornecida

2. **SQL Server LocalDB (Alternativa leve):**
   ```bash
   # Já incluído com Visual Studio
   sqllocaldb create MSSQLLocalDB
   sqllocaldb start MSSQLLocalDB
   ```

## 📱 Arquitetura e Tecnologias

### 🏗️ Stack Tecnológico

| Categoria | Tecnologia | Versão |
|-----------|------------|--------|
| **Framework** | ASP.NET Core | 7.0 |
| **Linguagem** | C# | 11.0 |
| **ORM** | Entity Framework Core | 7.0.17 |
| **Banco de Dados** | SQL Server | 2019+ |
| **Autenticação** | JWT + 2FA | - |
| **Documentação** | Swagger/OpenAPI | 6.5.0 |
| **Logs** | Serilog | 7.0.0 |
| **Criptografia** | BCrypt.Net | 4.0.3 |

### 🔗 Integrações Externas

- **OpenWeatherMap API** - Dados de qualidade do ar
- **Google Maps Geocoding** - Busca de localizações
- **Firebase Admin** - Autenticação 2FA (opcional)

### 🏛️ Padrões Arquiteturais

- **Repository Pattern** - Abstração de dados
- **Service Layer** - Lógica de negócio
- **DTO Pattern** - Transferência de dados
- **Dependency Injection** - Inversão de controle
- **Middleware Pipeline** - Tratamento de requisições

## 📁 Estrutura do Projeto

```
airwatch-api/
├── AirWatch.Api/                    # Projeto principal
│   ├── Controllers/                 # Endpoints da API
│   │   ├── AuthController.cs        # Autenticação e 2FA
│   │   ├── FeedbacksController.cs   # CRUD de feedbacks
│   │   ├── LocationsController.cs   # Busca de localizações
│   │   ├── PollutionController.cs   # Dados de qualidade do ar
│   │   └── UserController.cs        # Perfil do usuário
│   ├── DTOs/                        # Objetos de transferência
│   │   ├── Auth/                    # DTOs de autenticação
│   │   ├── Feedback/                # DTOs de feedback
│   │   ├── Location/                # DTOs de localização
│   │   ├── Pollution/               # DTOs de poluição
│   │   └── User/                    # DTOs de usuário
│   ├── Models/                      # Modelos de dados
│   │   ├── Entities/                # Entidades do banco
│   │   └── AirWatchDbContext.cs     # Contexto do EF Core
│   ├── Repositories/                # Camada de dados
│   │   ├── Interfaces/              # Contratos dos repositórios
│   │   └── [Entity]Repository.cs    # Implementações
│   ├── Services/                    # Lógica de negócio
│   │   ├── GoogleMapsGeocodingService.cs
│   │   └── OpenWeatherMapService.cs
│   ├── Migrations/                  # Migrações do banco
│   ├── Logs/                        # Arquivos de log
│   ├── Program.cs                   # Ponto de entrada
│   └── appsettings.json             # Configurações
├── test-register.http               # Testes da API
└── README.md                        # Este arquivo
```

## ⚙️ Configuração

### 🔑 Chaves de API Necessárias

| Serviço | Como Obter | Documentação |
|---------|------------|--------------|
| **OpenWeatherMap** | [Criar conta gratuita](https://openweathermap.org/api) | [API Docs](https://openweathermap.org/api/air-pollution) |
| **Google Maps** | [Google Cloud Console](https://console.cloud.google.com/) | [Geocoding API](https://developers.google.com/maps/documentation/geocoding) |
| **Firebase** | [Firebase Console](https://console.firebase.google.com/) | [Admin SDK](https://firebase.google.com/docs/admin/setup) |

### 📝 Variáveis de Ambiente

#### Windows (PowerShell)
```powershell
# Configuração temporária (sessão atual)
$env:DATABASE_CONNECTION_STRING = "Server=localhost;Database=AirWatch;Trusted_Connection=True;TrustServerCertificate=True;"
$env:JWT_SECRET = "SuaChaveSecretaDeNoMinimo32Caracteres123456789"
$env:OPENWEATHERMAP_API_KEY = "sua_chave_openweathermap"
$env:GOOGLE_MAPS_API_KEY = "sua_chave_google_maps"
$env:ALLOWED_ORIGINS = "http://localhost:19006,http://localhost:8081"

# Configuração permanente (requer reiniciar terminal)
setx DATABASE_CONNECTION_STRING "Server=localhost;Database=AirWatch;Trusted_Connection=True;TrustServerCertificate=True;"
setx JWT_SECRET "SuaChaveSecretaDeNoMinimo32Caracteres123456789"
setx OPENWEATHERMAP_API_KEY "sua_chave_openweathermap"
setx GOOGLE_MAPS_API_KEY "sua_chave_google_maps"
setx ALLOWED_ORIGINS "http://localhost:19006,http://localhost:8081"
```

#### macOS/Linux (Bash)
```bash
# Adicionar ao ~/.bashrc ou ~/.zshrc
export DATABASE_CONNECTION_STRING="Server=localhost;Database=AirWatch;User Id=sa;Password=SuaSenha123!;TrustServerCertificate=True;"
export JWT_SECRET="SuaChaveSecretaDeNoMinimo32Caracteres123456789"
export OPENWEATHERMAP_API_KEY="sua_chave_openweathermap"
export GOOGLE_MAPS_API_KEY="sua_chave_google_maps"
export ALLOWED_ORIGINS="http://localhost:19006,http://localhost:8081"

# Recarregar configurações
source ~/.bashrc
```

### 📄 appsettings.json (Opcional)

```json
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
    "Secret": "NUNCA_USE_EM_PRODUCAO_USE_ENV_VAR",
    "ExpirationHours": 24
  },
  "OpenWeatherMap": {
    "ApiKey": "USE_VARIAVEL_DE_AMBIENTE",
    "BaseUrl": "https://api.openweathermap.org/data/2.5/"
  },
  "Google": {
    "MapsApiKey": "USE_VARIAVEL_DE_AMBIENTE"
  },
  "Cors": {
    "AllowedOrigins": "http://localhost:19006"
  }
}
```

> ⚠️ **Importante:** Nunca coloque chaves secretas no appsettings.json em produção!

## 🚀 Primeira Execução

### 1️⃣ Clone do Repositório

```bash
git clone https://github.com/seu-usuario/airwatch-systems.git
cd airwatch-systems/airwatch-api
```

### 2️⃣ Instalação de Ferramentas

```bash
# Instalar Entity Framework CLI
dotnet tool install --global dotnet-ef

# Ou atualizar se já estiver instalado
dotnet tool update --global dotnet-ef

# Verificar instalação
dotnet ef --version
```

### 3️⃣ Configuração do Certificado HTTPS

```bash
# Confiar no certificado de desenvolvimento
dotnet dev-certs https --trust
```

### 4️⃣ Restauração de Pacotes

```bash
cd AirWatch.Api
dotnet restore
```

### 5️⃣ Configuração do Banco de Dados

```bash
# Aplicar migrações ao banco
dotnet ef database update
```

### 6️⃣ Execução da Aplicação

```bash
# Executar em modo desenvolvimento
dotnet run

# Ou executar com hot reload
dotnet watch run
```

### 7️⃣ Verificação da Instalação

✅ **Aplicação rodando em:**
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`

✅ **Swagger disponível em:**
- `https://localhost:5001/swagger`
- `http://localhost:5000/swagger`

✅ **Health Check:**
- `GET https://localhost:5001/health`

## 📝 Uso da API

### 📚 Documentação Interativa (Swagger)

Acesse `https://localhost:5001/swagger` para:
- 📋 Ver todos os endpoints disponíveis
- 📝 Testar requisições diretamente no navegador
- 📄 Visualizar esquemas de dados
- 🔒 Configurar autenticação JWT

### 📎 Endpoints Principais

| Categoria | Endpoint | Método | Descrição |
|-----------|----------|---------|-----------|
| **Auth** | `/api/auth/register` | POST | Cadastro de usuário |
| **Auth** | `/api/auth/login` | POST | Login com 2FA |
| **Auth** | `/api/auth/verify-2fa` | POST | Verificação 2FA |
| **Pollution** | `/api/pollution/current` | GET | Dados atuais de qualidade do ar |
| **Pollution** | `/api/pollution/history` | GET | Histórico de poluição |
| **Feedbacks** | `/api/feedbacks` | POST | Criar feedback |
| **Feedbacks** | `/api/feedbacks/my` | GET | Meus feedbacks |
| **Feedbacks** | `/api/feedbacks/near` | GET | Feedbacks por localização |
| **Locations** | `/api/locations/search` | GET | Buscar localizações |
| **User** | `/api/user/profile` | GET | Perfil do usuário |

### 📝 Exemplos de Uso

#### 1. Cadastro de Usuário
```http
POST /api/auth/register
Content-Type: application/json

{
  "name": "João Silva",
  "email": "joao@exemplo.com",
  "password": "MinhaSenh@123",
  "confirmPassword": "MinhaSenh@123"
}
```

#### 2. Login e 2FA
```http
# Passo 1: Login
POST /api/auth/login
Content-Type: application/json

{
  "email": "joao@exemplo.com",
  "password": "MinhaSenh@123"
}

# Resposta:
{
  "requires2FA": true,
  "sessionId": "sess_abc123"
}

# Passo 2: Verificar 2FA (código no log do servidor)
POST /api/auth/verify-2fa
Content-Type: application/json

{
  "sessionId": "sess_abc123",
  "token": "123456"
}

# Resposta:
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "refresh_token_here",
  "expiresIn": 86400
}
```

#### 3. Consultar Qualidade do Ar
```http
GET /api/pollution/current?lat=-23.5505&lon=-46.6333
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...

# Resposta:
{
  "aqi": 3,
  "pollutants": {
    "pM25": 25.4,
    "pM10": 45.2,
    "co": 1.2,
    "nO2": 15.8,
    "sO2": 5.1,
    "o3": 85.3
  },
  "lastUpdated": "2024-01-15T10:30:00Z"
}
```

#### 4. Criar Feedback
```http
POST /api/feedbacks
Content-Type: application/json
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...

{
  "lat": -23.5505,
  "lon": -46.6333,
  "rating": 4,
  "comment": "Ar bem limpo hoje!"
}
```

> 📁 **Arquivo de Testes:** Use o arquivo `test-register.http` para testes completos

## 🔐 Autenticação 2FA

### 🔄 Fluxo de Autenticação

1. **Login:** `/api/auth/login` valida credenciais
2. **Código 2FA:** Sistema gera código de 6 dígitos
3. **Verificação:** `/api/auth/verify-2fa` valida código e retorna JWT
4. **Refresh:** `/api/auth/refresh` renova token expirado

### 🔧 Configuração 2FA

**Modo Desenvolvimento:**
- Código 2FA aparece no log do servidor
- Não requer configuração adicional

**Modo Produção:**
- Configure `FIREBASE_CREDENTIALS` para envio via push/SMS
- Integre com serviços de notificação

## 🔗 Integrações Externas

### 🌤️ OpenWeatherMap API

```bash
# Obter chave gratuita
1. Acesse: https://openweathermap.org/api
2. Crie uma conta
3. Gere uma API key
4. Configure: OPENWEATHERMAP_API_KEY
```

**Endpoints utilizados:**
- `/api/pollution/current` - Dados atuais

### 🗺️ Google Maps Geocoding

```bash
# Configurar Google Cloud
1. Acesse: https://console.cloud.google.com/
2. Crie um projeto
3. Ative a Geocoding API
4. Gere uma API key
5. Configure: GOOGLE_MAPS_API_KEY
```

**Funcionalidades:**
- Busca de endereços por texto
- Conversão coordenadas ↔ endereços
- Sugestões de localização

### 🔥 Firebase (Opcional)

```bash
# Configurar Firebase
1. Acesse: https://console.firebase.google.com/
2. Crie um projeto
3. Gere Service Account Key
4. Configure: FIREBASE_CREDENTIALS
```

## 📦 Deploy

### 🖥️ Deploy Local (Kestrel)

```bash
# Publicar aplicação
dotnet publish -c Release -o ./publish

# Executar
cd publish
./AirWatch.Api.exe
```

### 🌐 Deploy IIS

1. **Instalar .NET Hosting Bundle:**
   - [Download ASP.NET Core Runtime](https://dotnet.microsoft.com/download/dotnet/7.0)

2. **Configurar IIS:**
   ```bash
   # Criar App Pool
   New-WebAppPool -Name "AirWatchAPI" -ManagedRuntimeVersion ""
   
   # Criar Site
   New-Website -Name "AirWatch API" -ApplicationPool "AirWatchAPI" -PhysicalPath "C:\inetpub\wwwroot\airwatch-api"
   ```

3. **Configurar Variáveis:**
   - Painel de Controle → Sistema → Variáveis de Ambiente
   - Ou via web.config

### ☁️ Deploy Azure

```bash
# Azure CLI
az webapp create --resource-group myResourceGroup --plan myAppServicePlan --name myapp --runtime "DOTNET|7.0"
az webapp deployment source config --name myapp --resource-group myResourceGroup --repo-url https://github.com/user/repo --branch main
```

## 🔒 Segurança

### 🛡️ Boas Práticas Implementadas

- ✅ **Criptografia de senhas** com BCrypt
- ✅ **JWT com expiração** configurável
- ✅ **Validação de entrada** em todos os endpoints
- ✅ **CORS configurável** por ambiente
- ✅ **HTTPS obrigatório** em produção
- ✅ **Rate limiting** de feedbacks (4h por região)
- ✅ **Logs sem dados sensíveis**

### 🔐 Configurações de Segurança

```json
{
  "Jwt": {
    "Secret": "CHAVE_MINIMO_32_CARACTERES",
    "ExpirationHours": 24,
    "Issuer": "AirWatch.Api",
    "Audience": "AirWatch.Client"
  },
  "Cors": {
    "AllowedOrigins": "https://meuapp.com,https://app.exemplo.com"
  }
}
```

## 🔧 Solução de Problemas

### ❌ Problemas Comuns

#### 🔴 Erro de Conexão com Banco
```bash
# Verificar string de conexão
dotnet ef database update --verbose

# Testar conectividade
sqlcmd -S localhost -E -Q "SELECT @@VERSION"
```

#### 🔴 Certificado HTTPS
```bash
# Recriar certificado
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

#### 🔴 2FA não funciona
```bash
# Verificar logs
tail -f Logs/log-*.txt

# Código aparece como: "2FA Code: 123456"
```

#### 🔴 APIs externas falham
```bash
# Verificar chaves
echo $OPENWEATHERMAP_API_KEY
echo $GOOGLE_MAPS_API_KEY

# Testar conectividade
curl "https://api.openweathermap.org/data/2.5/air_pollution?lat=0&lon=0&appid=SUA_CHAVE"
```

### 📊 Logs e Monitoramento

```bash
# Localização dos logs
./Logs/log-YYYYMMDD.txt

# Níveis de log
- Information: Operações normais
- Warning: Situações inesperadas
- Error: Erros tratados
- Critical: Falhas graves
```

### 🆘 Suporte

Para problemas não resolvidos:

1. **Verifique os logs** em `./Logs/`
2. **Consulte o Swagger** em `/swagger`
3. **Teste com** `test-register.http`
4. **Verifique variáveis** de ambiente
5. **Consulte documentação** das APIs externas

---

## 📚 Bibliotecas e Dependências

### 📦 Pacotes NuGet Principais

| Pacote | Versão | Descrição |
|--------|--------|-----------|
| `Microsoft.EntityFrameworkCore` | 7.0.17 | ORM principal |
| `Microsoft.EntityFrameworkCore.SqlServer` | 7.0.17 | Provider SQL Server |
| `Microsoft.EntityFrameworkCore.Tools` | 7.0.17 | Ferramentas EF CLI |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 7.0.20 | Autenticação JWT |
| `Swashbuckle.AspNetCore` | 6.5.0 | Documentação Swagger |
| `BCrypt.Net-Next` | 4.0.3 | Criptografia de senhas |
| `Serilog.AspNetCore` | 7.0.0 | Sistema de logs |
| `FirebaseAdmin` | 2.4.0 | Integração Firebase |

### 🔧 Ferramentas de Desenvolvimento

```bash
# Entity Framework CLI
dotnet tool install --global dotnet-ef

# Verificar versão
dotnet ef --version
```