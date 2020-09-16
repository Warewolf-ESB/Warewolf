FROM microsoft/dotnet-framework:4.7.2-sdk AS build 
WORKDIR /app
COPY APITesting.sln .
COPY ApiTesting/*.csproj ./ApiTesting/
COPY ApiTesting/*.config ./ApiTesting/
RUN nuget restore
COPY ApiTesting/. ./ApiTesting/
WORKDIR /app/ApiTesting
RUN msbuild /p:Configuration=Release

FROM mcr.microsoft.com/dotnet/framework/aspnet:4.7.2 AS runtime
WORKDIR /inetpub/wwwroot
COPY --from=build /app/ApiTesting/. ./
EXPOSE 80