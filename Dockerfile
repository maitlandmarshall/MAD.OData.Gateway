FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["MAD.OData.Gateway/MAD.OData.Gateway.csproj", "MAD.OData.Gateway/"]
RUN dotnet restore "MAD.OData.Gateway/MAD.OData.Gateway.csproj"
COPY . .
WORKDIR "/src/MAD.OData.Gateway"
RUN dotnet build "MAD.OData.Gateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MAD.OData.Gateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MAD.OData.Gateway.dll"]