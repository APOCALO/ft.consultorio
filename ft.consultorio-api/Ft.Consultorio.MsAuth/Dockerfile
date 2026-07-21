FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled-extra AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Ft.Consultorio.MsAuth/Ft.Consultorio.MsAuth.csproj", "Ft.Consultorio.MsAuth/"]
RUN dotnet restore "Ft.Consultorio.MsAuth/Ft.Consultorio.MsAuth.csproj"
COPY . .
WORKDIR "/src/Ft.Consultorio.MsAuth"
RUN dotnet publish "Ft.Consultorio.MsAuth.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Ft.Consultorio.MsAuth.dll"]
