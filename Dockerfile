# Usa una imagen base oficial de .NET Core
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Etapa de compilación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Quokka.csproj", "./"]
RUN dotnet restore "./Quokka.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "./Quokka.csproj" -c Release -o /app/build

# Etapa de publicación
FROM build AS publish
RUN dotnet publish "./Quokka.csproj" -c Release -o /app/publish

# Etapa final: Construcción del contenedor
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Quokka.dll"] 
