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
├── Configuration/
│   └── ImageSettings.cs                   # Configurações para exibição de imagens
│
├── Controllers/                           # Lida com requisições e navegação
│   ├── AccountController.cs
│   ├── BaseController.cs
│   ├── HomeController.cs
│   ├── MovieController.cs
│   └── RatingController.cs
│
├── Data/
│   └── ApplicationDbContext.cs            # DbContext para Entity Framework
│
├── Models/
│   ├── DTOs/
│   │   └── MovieDTO.cs                    # Objeto de transferência de dados de filmes
│   ├── ViewModels/
│   │   ├── AuthenticationViewModels.cs
│   │   ├── UserViewModels.cs
│   │   └── ViewModel.cs
│   ├── ErrorViewModel.cs
│   ├── Movie.cs                           # Modelo de entidade para filmes
│   └── User.cs                            # Modelo de entidade para usuários
│
├── Services/                              # Lógica de negócio
│   ├── AuthService.cs
│   ├── MovieService.cs
│   ├── RatingService.cs
│   └── TMDbService.cs                     # Comunicação com API do TMDb
│
├── Views/                                 # Interface (Razor Views)
│   ├── Home/
│   │   ├── Index.cshtml
│   │   └── Privacy.cshtml
│   ├── Movies/
│   │   ├── Details.cshtml
│   │   └── Index.cshtml
│   ├── Shared/
│   │   ├── _Layout.cshtml                 # Layout principal do site
│   │   ├── _ValidationScriptsPartial.cshtml
│   │   └── Error.cshtml
│   ├── User/
│   │   ├── Favorites.cshtml
│   │   ├── Login.cshtml
│   │   ├── MyRatings.cshtml
│   │   ├── RateMovie.cshtml
│   │   ├── Register.cshtml
│   │   └── UserProfile.cshtml
│   ├── _ViewImports.cshtml
│   └── _ViewStart.cshtml
│
├── wwwroot/                               # Arquivos estáticos (imagens, CSS, JS)
├── appsettings.json                       # Configurações da aplicação
├── Program.cs                             # Ponto de entrada do app
└── CineHub.sln                            # Solução do Visual Studio

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
