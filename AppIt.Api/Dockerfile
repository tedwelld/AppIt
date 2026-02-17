FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["AppIt.Api/AppIt.Api.csproj", "AppIt.Api/"]
COPY ["AppIt.Core/AppIt.Core.csproj", "AppIt.Core/"]
COPY ["AppIt.Data/AppIt.Data.csproj", "AppIt.Data/"]
RUN dotnet restore "AppIt.Api/AppIt.Api.csproj"

COPY . .
WORKDIR "/src/AppIt.Api"
RUN dotnet publish "AppIt.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false /p:PublishAot=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AppIt.Api.dll"]
