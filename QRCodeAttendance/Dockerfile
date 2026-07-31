# ----------------------
# Base runtime image
# ----------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# ----------------------
# Build stage
# ----------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["QRCodeAttendance/QRCodeAttendance.csproj", "QRCodeAttendance/"]
RUN dotnet restore "QRCodeAttendance/QRCodeAttendance.csproj"

# Copy the rest of the project
COPY . .
WORKDIR "/src/QRCodeAttendance"

# Build the project
RUN dotnet build "QRCodeAttendance.csproj" -c Release -o /app/build

# ----------------------
# Publish stage
# ----------------------
FROM build AS publish
RUN dotnet publish "QRCodeAttendance.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ----------------------
# Final image
# ----------------------
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "QRCodeAttendance.dll"]
