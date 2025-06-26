# Use a imagem base do .NET 8 SDK para build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiar o arquivo de projeto e restaurar dependências
COPY *.csproj ./
RUN dotnet restore

# Copiar todo o código fonte
COPY . ./

# Fazer o build da aplicação
RUN dotnet publish -c Release -o out

# Use a imagem runtime do .NET 8 para execução
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copiar os arquivos publicados do estágio de build
COPY --from=build /app/out .

# Expor a porta que a aplicação vai usar
EXPOSE 8080
EXPOSE 8081

# Definir o ponto de entrada
ENTRYPOINT ["dotnet", "CineHub.dll"]