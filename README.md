# Quick Start Guide 🚀

Running Power Positions on Cmd Line or Windows Service

## PowerService.dll Location

1. Downloaded from: https://github.com/kkmoorthy/PetroineosCodingChallenge
2. Place `PowerService.dll` in the `ExternalLibs/` folder

## Default Configure Settings

Edit `appsettings.json`:

```json
{
  "PowerPositionSettings": {
    "CsvOutputPath": "PowerReports",
    "ExtractIntervalMinutes": 1
  }
}
```

## Run as a Console App

```bash
# Navigate to project folder
cd PowerPositions

# Restore packages
dotnet restore

# Run Release version
dotnet run -c Release
```

## Install as Windows Service
```bash
# Opend cmd line as Admin
# Navigate to project folder
cd PowerPositions

# Restore packages
dotnet restore

# Publish the service
dotnet publish --configuration Release --output ./publish/Release

# Create and Start the service
sc.exe create PowerPositions binPath= "C:\full\path\to\PowerPositions\publish\Release\PowerPositions.exe"

sc.exe start PowerPositions 

# Stop the service
sc.exe stop PowerPositions 

# Delete the service
sc.exe delete PowerPositions

```
