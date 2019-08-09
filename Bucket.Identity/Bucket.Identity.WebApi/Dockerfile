FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["Bucket.Identity.WebApi/Bucket.Identity.WebApi.csproj", "Bucket.Identity.WebApi/"]
COPY ["Bucket.Identity.IServices/Bucket.Identity.IServices.csproj", "Bucket.Identity.IServices/"]
COPY ["Bucket.Identity.Dto/Bucket.Identity.Dto.csproj", "Bucket.Identity.Dto/"]
COPY ["Bucket.Identity.Model/Bucket.Identity.Model.csproj", "Bucket.Identity.Model/"]
COPY ["Bucket.Identity.Services/Bucket.Identity.Services.csproj", "Bucket.Identity.Services/"]
RUN dotnet restore "Bucket.Identity.WebApi/Bucket.Identity.WebApi.csproj"
COPY . .
WORKDIR "/src/Bucket.Identity.WebApi"
RUN dotnet build "Bucket.Identity.WebApi.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Bucket.Identity.WebApi.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Bucket.Identity.WebApi.dll"]
