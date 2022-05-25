FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["./GnuPlotAdapter/GnuPlotAdapter.csproj", "./"]
RUN dotnet restore "GnuPlotAdapter.csproj"
COPY ./GnuPlotAdapter .
WORKDIR "/src/"
RUN dotnet build "GnuPlotAdapter.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GnuPlotAdapter.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GnuPlotAdapter.dll"]