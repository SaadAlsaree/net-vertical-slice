# NetVerticalSlice Templates

This package contains .NET project templates maintained by net-vertical-slice.

## Included templates

- `vsa-webapi` - Wolverine VSA Web API

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

`bin/Release/NetVerticalSlice.Templates.1.0.0.nupkg`

## Publish to NuGet.org

```bash
dotnet nuget push .\bin\Release\NetVerticalSlice.Templates.1.0.0.nupkg --source https://api.nuget.org/v3/index.json --api-key oy2jdh45dowd7wiytoyaswmqcxfcrk4ujbowzjrm34mtwq
```
