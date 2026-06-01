FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/Lienzo.API/Lienzo.API.csproj", "Lienzo.API/"]
COPY ["src/Lienzo.Application/Lienzo.Application.csproj", "Lienzo.Application/"]
COPY ["src/Lienzo.Domain/Lienzo.Domain.csproj", "Lienzo.Domain/"]
COPY ["src/Lienzo.Infrastructure/Lienzo.Infrastructure.csproj", "Lienzo.Infrastructure/"]
RUN dotnet restore "Lienzo.API/Lienzo.API.csproj"

COPY src/ .
WORKDIR "/src/Lienzo.API"
RUN dotnet build "Lienzo.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Lienzo.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Lienzo.API.dll"]
