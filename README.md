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
    <PackageReference Include="Codestellation.Appulse" Version="0.2.0" PrivateAssets="All" />
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

## Automatic Update To Reference `.editorconfig` 

Starting from version `0.2` Codestellation.Appulse can update local `.editorconfig` automatically and it's on by default. 
If you want to disable it add the following property to the project file:

```xml
<PropertyGroup>
    <EditorConfigAutoUpdate>false</EditorConfigAutoUpdate>
</PropertyGroup>
```