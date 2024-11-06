do
{
    $assemblyVersion = Read-Host "Enter the assembly version (e.g., 2.0.0)"

    if ($assemblyVersion -eq "exit")
    {
        Write-Host "Exiting..."
        break
    }

    if ($assemblyVersion -match '\d+\.\d+\.\d+')
    {
        Write-Host "Valid assembly version: $assemblyVersion"
        Write-Host "Building release version..."
        dotnet publish -c Release --self-contained -r win-x64 -o .\Publish /p:AssemblyVersion=$assemblyVersion /p:Version=$assemblyVersion-product-version -p:FileVersion=$assemblyVersion
        Write-Host "Packing release version..."
        vpk pack -u Hermes -v $assemblyVersion -p .\Publish -e Hermes.exe
        break
    }
    else
    {
        Write-Warning "Invalid assembly version format. Please enter a version in the format 'x.x.x.x' or type 'exit' to quit."
    }
} while ($true)