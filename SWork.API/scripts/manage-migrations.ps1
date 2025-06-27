# SWork Migration Management Script
# Usage: .\manage-migrations.ps1 [command] [parameters]

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("add", "update", "remove", "list", "script")]
    [string]$Command,
    
    [Parameter(Mandatory=$false)]
    [string]$MigrationName,
    
    [Parameter(Mandatory=$false)]
    [string]$FromMigration,
    
    [Parameter(Mandatory=$false)]
    [string]$ToMigration
)

$ProjectPath = "SWork.API"
$StartupProject = "SWork.API"
$Context = "SWorkDbContext"

Write-Host "SWork Migration Manager" -ForegroundColor Green
Write-Host "=======================" -ForegroundColor Green

switch ($Command.ToLower()) {
    "add" {
        if ([string]::IsNullOrEmpty($MigrationName)) {
            Write-Host "Error: Migration name is required for 'add' command" -ForegroundColor Red
            Write-Host "Usage: .\manage-migrations.ps1 add 'MigrationName'" -ForegroundColor Yellow
            exit 1
        }
        
        Write-Host "Adding migration: $MigrationName" -ForegroundColor Yellow
        dotnet ef migrations add $MigrationName --project $ProjectPath --startup-project $StartupProject --context $Context
    }
    
    "update" {
        Write-Host "Updating database with latest migrations..." -ForegroundColor Yellow
        dotnet ef database update --project $ProjectPath --startup-project $StartupProject --context $Context
    }
    
    "remove" {
        Write-Host "Removing last migration..." -ForegroundColor Yellow
        dotnet ef migrations remove --project $ProjectPath --startup-project $StartupProject --context $Context
    }
    
    "list" {
        Write-Host "Listing all migrations:" -ForegroundColor Yellow
        dotnet ef migrations list --project $ProjectPath --startup-project $StartupProject --context $Context
    }
    
    "script" {
        $scriptParams = ""
        if (![string]::IsNullOrEmpty($FromMigration)) {
            $scriptParams += " --from $FromMigration"
        }
        if (![string]::IsNullOrEmpty($ToMigration)) {
            $scriptParams += " --to $ToMigration"
        }
        
        Write-Host "Generating migration script..." -ForegroundColor Yellow
        dotnet ef migrations script $scriptParams --project $ProjectPath --startup-project $StartupProject --context $Context --output "migration-script.sql"
        Write-Host "Migration script generated: migration-script.sql" -ForegroundColor Green
    }
}

Write-Host "Migration command completed!" -ForegroundColor Green 