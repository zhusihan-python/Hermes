# Hermes 2.0

## Add migration

```
dotnet ef migrations add 001 --context HermesRemoteContext --output-dir AppData/Migrations/Remote
```

```
dotnet ef migrations add 001 --context HermesLocalContext --output-dir AppData/Migrations/Local
```