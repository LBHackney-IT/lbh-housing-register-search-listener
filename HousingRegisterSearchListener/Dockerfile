FROM mcr.microsoft.com/dotnet/sdk:6.0

ARG LBHPACKAGESTOKEN
ENV LBHPACKAGESTOKEN=$LBHPACKAGESTOKEN
WORKDIR /app

# Copy csproj and NUGet.Config and restore as distinct layers
COPY ./HousingRegisterSearchListener/HousingRegisterSearchListener.csproj ./
COPY /NuGet.Config /root/.nuget/NuGet/NuGet.Config
RUN dotnet restore ./HousingRegisterSearchListener.csproj


# Copy everything else and build
COPY ./HousingRegisterSearchListener ./
RUN dotnet build -c Release -o out ./HousingRegisterSearchListener.csproj
RUN dotnet publish -c Release -o out ./HousingRegisterSearchListener.csproj

EXPOSE ${PORT:-3000}
CMD ASPNETCORE_URLS=http://+:${PORT:-3000} dotnet ./out/HousingRegisterSearchListener.dll
