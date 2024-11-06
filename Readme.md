# Hermes 2.0

## Build new version

If you want to build a new version, you can use the following script,
Be sure to replace the assembly version with the new version you want to build.

```
Scripts/build_hermes.ps1
```

Probably you will need to install the vpk tool, you can find it here: https://docs.velopack.io/

```
dotnet tool update -g vpk
```

## Deploy new version

If you want to deploy a new version, you need to copy the files generated to the folder /Releases
into the http server. We are currently using Apache-Xampp to do so.

```
How to Setup a Local Static File Server in Windows by XAMPP

1. create a folder for server root path in location [ D:\zfileserver ]

2. Go to the path [D:\xampp\apache\conf\extra]
Open the file [ httpd-vhosts.conf ]

3. Add a code block like

<VirtualHost 10.12.204.48:80>
    DocumentRoot "D:/xampp/zfileserver"
    ServerName 10.12.204.48
    ErrorLog "logs/10.12.204.48-error.log"
    CustomLog "logs/10.12.204.48-access.log" combined
    <Directory "D:/xampp/zfileserver">
        # AllowOverride All      # Deprecated
        # Order Allow,Deny       # Deprecated
        # Allow from all         # Deprecated

        # --New way of doing it
        Require all granted    
    </Directory>
</VirtualHost>


4. Now go to any browser and type http://10.12.204.48/hermes/Hermes-win-Setup.exe

Based on: https://gist.github.com/javagrails/9acb42d95b0d49dcb78fe454da59bff6
```

## Add migration

```
dotnet ef migrations add 001 --context HermesRemoteContext --output-dir AppData/Migrations/Remote
```

```
dotnet ef migrations add 001 --context HermesLocalContext --output-dir AppData/Migrations/Local
```