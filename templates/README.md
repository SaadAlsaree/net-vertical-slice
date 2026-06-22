# NetVerticalSlice Templates

This package contains .NET project templates maintained by net-vertical-slice.

## Included templates

- `vsa-webapi` - Wolverine VSA Web API
- `vsa-feature` - Wolverine VSA Feature (item template)

## Install

```bash
dotnet new install NetVerticalSlice.Templates
```

## Use

```bash
dotnet new vsa-webapi -n MyApi
```

## Build package

```bash
dotnet pack NetVerticalSlice.Templates.csproj -c Release
```

The package is created at:

`bin/Release/NetVerticalSlice.Templates.1.0.2.nupkg`

## Publish to NuGet.org

Requires the `NUGET_API_KEY` environment variable. **Never commit API keys to source control.**

```bash
dotnet nuget push .\bin\Release\NetVerticalSlice.Templates.<version>.nupkg \
  --source https://api.nuget.org/v3/index.json \
  --api-key $NUGET_API_KEY
```
