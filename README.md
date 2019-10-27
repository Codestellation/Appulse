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
<PropertyGroup>
    <ReferenceEditorConfig>file://somewhere/in/filesystem/.editorconfig</ReferenceEditorConfig>
</PropertyGroup>
```

or to to a file available throught http/https

```xml
<PropertyGroup>
    <ReferenceEditorConfig>https://raw.githubusercontent.com/Codestellation/Standards/master/.editorconfig</ReferenceEditorConfig>
</PropertyGroup>
```

If files have differences build will fail and error message will contains instruction how to fix it.
