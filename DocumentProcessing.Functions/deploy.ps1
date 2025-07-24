# Azure Functions Deployment Script
# Prerequisites:
# - Azure CLI installed and logged in
# - Azure Functions Core Tools installed
# - .NET 8 SDK installed

param(
    [Parameter(Mandatory=$true)]
    [string]$ResourceGroupName,
    
    [Parameter(Mandatory=$true)]
    [string]$FunctionAppName,
    
    [Parameter(Mandatory=$true)]
    [string]$StorageAccountName,
    
    [Parameter(Mandatory=$false)]
    [string]$Location = "East US"
)

Write-Host "Starting deployment of Document Processing Functions..." -ForegroundColor Green

# Create resource group if it doesn't exist
Write-Host "Creating resource group: $ResourceGroupName" -ForegroundColor Yellow
az group create --name $ResourceGroupName --location $Location

# Create storage account
Write-Host "Creating storage account: $StorageAccountName" -ForegroundColor Yellow
az storage account create `
    --name $StorageAccountName `
    --resource-group $ResourceGroupName `
    --location $Location `
    --sku Standard_LRS

# Create function app
Write-Host "Creating function app: $FunctionAppName" -ForegroundColor Yellow
az functionapp create `
    --resource-group $ResourceGroupName `
    --consumption-plan-location $Location `
    --runtime dotnet-isolated `
    --runtime-version 8.0 `
    --functions-version 4 `
    --name $FunctionAppName `
    --storage-account $StorageAccountName

# Configure app settings
Write-Host "Configuring application settings..." -ForegroundColor Yellow
az functionapp config appsettings set `
    --name $FunctionAppName `
    --resource-group $ResourceGroupName `
    --settings "PrintingApiBaseUrl=https://api.printingservice.com" "PrintingApiKey=your-printing-api-key-here"

# Build and package the function
Write-Host "Building and packaging function..." -ForegroundColor Yellow
dotnet publish --configuration Release --output ./publish

# Deploy the function
Write-Host "Deploying function app..." -ForegroundColor Yellow
func azure functionapp publish $FunctionAppName

Write-Host "Deployment completed successfully!" -ForegroundColor Green
Write-Host "Function App URL: https://$FunctionAppName.azurewebsites.net" -ForegroundColor Cyan
Write-Host "You can test the API using the endpoints in test-samples.http" -ForegroundColor Cyan