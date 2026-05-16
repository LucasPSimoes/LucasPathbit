\# OrderApi



API REST para gerenciamento de clientes e pedidos, desenvolvida em .NET 9 com C#, seguindo os princípios DDD, SOLID e Clean Code.



\## Tecnologias



\- .NET 9 / C#

\- Entity Framework Core 9

\- PostgreSQL

\- JWT Authentication

\- Swagger

\- Serilog

\- xUnit + Moq + FluentAssertions

\- Docker



\## Estrutura do Projeto



./src

./Application

./Common

./Domain

./Infrastructure

./tests/UnitTests

./scripts/db



\## Execução Local



\### Pré-requisitos

\- .NET 9 SDK

\- PostgreSQL 16+



\### Passos



1\. Clone o repositório:

```bash

git clone https://github.com/seu-usuario/OrderApi.git

cd OrderApi

```



2\. Configure o `appsettings.json`:

```json

{

&#x20; "ConnectionStrings": {

&#x20;   "DefaultConnection": "Host=localhost;Port=5432;Database=orderapi;Username=postgres;Password=postgres123"

&#x20; },

&#x20; "Jwt": {

&#x20;   "Key": "chave-secreta-super-segura-minimo-32-chars!!",

&#x20;   "Issuer": "OrderApi",

&#x20;   "Audience": "OrderApi"

&#x20; }

}

```



3\. Execute as migrations:

```bash

dotnet ef database update --project src\\Infrastructure\\Infrastructure.csproj --startup-project OrderApi.csproj

```



4\. Rode o projeto:

```bash

dotnet run --project OrderApi.csproj

```



5\. Acesse o Swagger: `https://localhost:7298/swagger`



\## Execução com Docker



```bash

docker-compose up --build

```



A API estará disponível em `http://localhost:8080/swagger`



\## Testes



```bash

dotnet test

```



\### Cobertura de testes



```bash

dotnet test --collect:"XPlat Code Coverage"

dotnet tool install --global dotnet-reportgenerator-globaltool

reportgenerator -reports:"tests/UnitTests/TestResults/\*\*/coverage.cobertura.xml" -targetdir:"coverage-report" -reporttypes:Html

```



Abra `coverage-report/index.html` para visualizar a cobertura.



\## Endpoints



| Método | Rota | Descrição | Auth |

|--------|------|-----------|------|

| POST | /api/Auth/signup | Cadastro de usuário | Público |

| POST | /api/Auth/login | Login e geração de token | Público |

| GET | /api/Product | Lista produtos | JWT |

| POST | /api/Product | Cria produto | Admin |

| PUT | /api/Product/{id} | Atualiza produto | Admin |

| DELETE | /api/Product/{id} | Remove produto | Admin |

| GET | /api/Customer | Lista clientes | JWT |

| POST | /api/Customer | Cria cliente | Admin |

| PUT | /api/Customer/{id} | Atualiza cliente | Admin |

| DELETE | /api/Customer/{id} | Remove cliente | Admin |

| GET | /api/Order | Lista pedidos | JWT |

| POST | /api/Order | Cria pedido | Cliente |

