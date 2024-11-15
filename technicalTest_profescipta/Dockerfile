# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
RUN apt update && apt install tzdata -y
ENV TZ="Asia/Jakarta"
WORKDIR /app
EXPOSE 80
EXPOSE 443

RUN apt-get update && \
    apt-get install -y libgdiplus && \
    apt-get install -y libc6-dev && \
    ln -s /usr/lib/libgdiplus.so /usr/lib/gdiplus.dll

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["technicalTest_profescipta/technicalTest_profescipta.csproj", "technicalTest_profescipta/"]
COPY ["TechnicalTest_profescipta.Common/TechnicalTest_profescipta.Common.csproj", "TechnicalTest_profescipta.Common/"]
COPY ["TechnicalTest_profescipta.Services/TechnicalTest_profescipta.Services.csproj", "TechnicalTest_profescipta.Services/"]
COPY ["TechnicalTest_profescipta.DAL/TechnicalTest_profescipta.DAL.csproj", "TechnicalTest_profescipta.DAL/"]
RUN dotnet restore "./technicalTest_profescipta/technicalTest_profescipta.csproj"
COPY . .
WORKDIR "/src/technicalTest_profescipta"
RUN dotnet build "./technicalTest_profescipta.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./technicalTest_profescipta.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "technicalTest_profescipta.dll"]