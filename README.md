# Appulse
MS build take to ensure latest `.editorconfig` usage by comparing one that placed in you solution folder and another one which is considered reference.

# Usage

## Installation
```powershell
PM> Install-Package Codestellation.Appulse
```

After completing install make sure your project contains such lines:

```xml
<ItemGroup>
    <PackageReference Include="Codestellation.Appulse" Version="0.1.6" PrivateAssets="All" />
</ItemGroup>
```

## Setting Location Of Reference `.editorconfig` location

Location is set using a msbuild propery `ReferenceEditorConfig`
It can point to a file
```xml
<ItemGroup>
    <PropertyGroup>file://somewhere/in/filesystem/.editorconfig</PropertyGroup>
</ItemGroup>
```

or to to a file available throught http/https

```xml
<ItemGroup>
    <PropertyGroup>https://raw.githubusercontent.com/Codestellation/Standards/master/.editorconfig</PropertyGroup>
</ItemGroup>
```

If files have differences build will fail and error message will contains instruction how to fix it.