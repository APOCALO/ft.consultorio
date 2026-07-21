FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Ft.Consultorio.Gateway/Ft.Consultorio.Gateway.csproj", "Ft.Consultorio.Gateway/"]
RUN dotnet restore "Ft.Consultorio.Gateway/Ft.Consultorio.Gateway.csproj"
COPY . .
WORKDIR "/src/Ft.Consultorio.Gateway"
RUN dotnet publish "Ft.Consultorio.Gateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Ft.Consultorio.Gateway.dll"]
