FROM mcr.microsoft.com/dotnet/sdk:6.0

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

ENV DynamoDb_LocalMode='true'
ARG LBHPACKAGESTOKEN
ENV LBHPACKAGESTOKEN=$LBHPACKAGESTOKEN
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./HousingRegisterSearchListener.sln ./
COPY ./HousingRegisterSearchListener/HousingRegisterSearchListener.csproj ./HousingRegisterSearchListener/
COPY ./HousingRegisterSearchListener.Tests/HousingRegisterSearchListener.Tests.csproj ./HousingRegisterSearchListener.Tests/
COPY /NuGet.Config /root/.nuget/NuGet/NuGet.Config

RUN dotnet restore ./HousingRegisterSearchListener/HousingRegisterSearchListener.csproj
RUN dotnet restore ./HousingRegisterSearchListener.Tests/HousingRegisterSearchListener.Tests.csproj

# Copy everything else and build
COPY . .

RUN dotnet build -c debug -o out HousingRegisterSearchListener.Tests/HousingRegisterSearchListener.Tests.csproj

CMD dotnet test
