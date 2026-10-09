FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY TenderEvidenceChecker.Api/TenderEvidenceChecker.Api.csproj TenderEvidenceChecker.Api/
RUN dotnet restore TenderEvidenceChecker.Api/TenderEvidenceChecker.Api.csproj
COPY TenderEvidenceChecker.Api/ TenderEvidenceChecker.Api/
RUN dotnet publish TenderEvidenceChecker.Api/TenderEvidenceChecker.Api.csproj -c Release -o /app -p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
# Railway: mount a volume at /data. One service runs the API and the job worker together.
ENV DATABASE_URL="Data Source=/data/tender.db" \
    PRIVATE_UPLOAD_DIR="/data/uploads" \
    EMBED_WORKER=1
ENTRYPOINT ["dotnet", "TenderEvidenceChecker.Api.dll"]
