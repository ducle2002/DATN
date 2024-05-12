FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

WORKDIR /src
COPY ["src/Yootek.Web.Host/Yootek.Web.Host.csproj", "src/Yootek.Web.Host/"]
COPY ["src/Yootek.Web.Core/Yootek.Web.Core.csproj", "src/Yootek.Web.Core/"]
COPY ["src/Yootek.Application/Yootek.Application.csproj", "src/Yootek.Application/"]
COPY ["src/Yootek.Core/Yootek.Core.csproj", "src/Yootek.Core/"]
COPY ["src/Yootek.EntityFrameworkCore/Yootek.EntityFrameworkCore.csproj", "src/Yootek.EntityFrameworkCore/"]
WORKDIR "/src/src/Yootek.Web.Host"
RUN dotnet restore 

WORKDIR /src
COPY ["src/Yootek.Web.Host", "src/Yootek.Web.Host"]
COPY ["src/Yootek.Web.Core", "src/Yootek.Web.Core"]
COPY ["src/Yootek.Application", "src/Yootek.Application"]
COPY ["src/Yootek.Core", "src/Yootek.Core"]
COPY ["src/Yootek.EntityFrameworkCore", "src/Yootek.EntityFrameworkCore"]
WORKDIR "/src/src/Yootek.Web.Host"
RUN dotnet publish -c Release -o /publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:7.0
EXPOSE 80
WORKDIR /app
COPY --from=build /publish .
ENTRYPOINT ["dotnet", "Yootek.Web.Host.dll"]
