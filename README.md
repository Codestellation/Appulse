# Codestellation Appulse

MS build task to ensure latest `.editorconfig` usage by comparing one that placed in you solution folder and another one which is considered reference. Capable to update or download local `.editorconfig` automatically.

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

Location is set using a msbuild property `AppulseReferenceEditorConfig`
It can point to a file

```xml
<PropertyGroup>
    <AppulseReferenceEditorConfig>file://somewhere/in/filesystem/.editorconfig</AppulseReferenceEditorConfig>
</PropertyGroup>
```

or to a file available over the network

```xml
<PropertyGroup>
    <AppulseReferenceEditorConfig>https://raw.githubusercontent.com/Codestellation/Standards/master/.editorconfig</AppulseReferenceEditorConfig>
</PropertyGroup>
```

If files have differences build will fail and error message will contains instruction how to fix it.

## Local  `.editorconfig` location

Appulse tries to locate local `.editorconfig` by recursive searching from a project folder to root folder. It stops at the root folder or at a folder containing `.git` directory. Also you can set location of local `.editorconfig` by providing `AppulseLocalEditorConfig` property.

```xml
<PropertyGroup>
    <AppulseLocalEditorConfig>../.editorconfig</AppulseLocalEditorConfig>
</PropertyGroup>
```

## Breaking Changes from 0.2 to 0.3

All properties was renamed to have Appulse prefix

* `ReferenceEditorConfig` -> `AppulseReferenceEditorConfig`
* `EditorConfigAutoUpdate` -> `AppulseEditorConfigAutoUpdate`

## Automatic Update To Reference `.editorconfig`

Starting from version `0.2` Codestellation.Appulse can update local `.editorconfig` automatically and it's on by default. It downloads reference `.editorconfig` to a folder which contains a `.sln` file in case local `.editorconfig` does not exist.
If you want to disable it add the following property to the project file:

```xml
<PropertyGroup>
    <AppulseEditorConfigAutoUpdate>false</AppulseEditorConfigAutoUpdate>
</PropertyGroup>
```
