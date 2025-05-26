# CineHub

**CineHub** é uma aplicação web desenvolvida como parte de um teste técnico, utilizando a stack ASP.NET Core MVC com foco em autenticação, consumo de API externa e persistência de dados com Entity Framework Core.

## 🧰 Tecnologias e Ferramentas Utilizadas

- **Framework**: ASP.NET Core MVC (.NET 6+)
- **Frontend**: Razor Pages, HTML5, CSS3, JavaScript
- **Backend**: C#
- **ORM**: Entity Framework Core
- **Banco de Dados**: PostgreSQL
- **API Externa**: [TMDb (The Movie Database)](https://www.themoviedb.org/documentation/api)
- **Gerenciamento de Dependências**: NuGet
- **IDE**: Visual Studio 2022
- **Versionamento**: Git + GitHub

## 📌 Funcionalidades Implementadas

- Autenticação e registro de usuários
- Listagem e detalhamento de filmes via API do TMDb
- Avaliação e favoritação de filmes por usuários autenticados
- Perfil do usuário com histórico de interações
- Organização da aplicação em camadas (Controller → Service → ViewModel)
- Layout principal reutilizável e Views organizadas

## 📁 Estrutura de Pastas 
CineHub/
├── Configuration/ # Configurações globais (ex: ImageSettings)
├── Controllers/ # Controladores MVC (Account, Movie, Rating, etc.)
├── Data/ # Contexto do EF Core (ApplicationDbContext)
├── Models/
│ ├── DTOs/ # Objetos de Transferência de Dados
│ ├── ViewModels/ # ViewModels para as Views
│ └── Entidades # Modelos principais: User, Movie, etc.
├── Services/ # Camada de serviços (Auth, Movie, Rating, TMDb)
├── Views/ # Razor Views organizadas por área
├── wwwroot/ # Arquivos estáticos (CSS, JS, imagens)
├── appsettings.json # Configurações (conexão com BD, chave API)
└── Program.cs # Configuração e execução da aplicação
## ⚙️ Como Executar

1. Clone o repositório:
   ```bash
   git clone https://github.com/marcusfreiremartins/CineHub.git

2. Configure a string de conexão no arquivo appsettings.json:
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=SEU_DATABASE;Username=SEU_USERNAME;Password=SUA_SENHA"
  },
"TMDb": {
  "ApiKey": "SUA_CHAVE_AQUI"
}
