FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY . .

RUN dotnet restore backend/SkillSwap.API/SkillSwap.API.csproj

RUN dotnet publish backend/SkillSwap.API/SkillSwap.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:$PORT

ENTRYPOINT ["dotnet", "SkillSwap.API.dll"]