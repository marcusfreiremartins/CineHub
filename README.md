# CineHub

**CineHub** é uma aplicação web desenvolvida como parte de um teste técnico, utilizando a stack ASP.NET Core MVC com foco em autenticação, consumo de API externa e persistência de dados com Entity Framework Core.

## 🧰 Tecnologias e Ferramentas Utilizadas

- **Framework**: ASP.NET Core MVC (.NET 7+)
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

3. Execute os scripts que estão na pasta "Migration" no PostgreSQL.
