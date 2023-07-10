# UAsset

Namespace: UAssetAPI

Represents an Unreal Engine asset.

```csharp
public class UAsset : UnrealPackage, UAssetAPI.IO.INameMap
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [UnrealPackage](./uassetapi.unrealpackage.md) → [UAsset](./uassetapi.uasset.md)<br>
Implements [INameMap](./uassetapi.io.inamemap.md)

## Fields

### **LegacyFileVersion**

The package file version number when this package was saved.

```csharp
public int LegacyFileVersion;
```

**Remarks:**

The lower 16 bits stores the UE3 engine version, while the upper 16 bits stores the UE4/licensee version. For newer packages this is -7.
 VersionDescription-2indicates presence of enum-based custom versions-3indicates guid-based custom versions-4indicates removal of the UE3 version. Packages saved with this ID cannot be loaded in older engine versions-5indicates the replacement of writing out the "UE3 version" so older versions of engine can gracefully fail to open newer packages-6indicates optimizations to how custom versions are being serialized-7indicates the texture allocation info has been removed from the summary-8indicates that the UE5 version has been added to the summary

### **UsesEventDrivenLoader**

Whether or not this asset is loaded with the Event Driven Loader.

```csharp
public bool UsesEventDrivenLoader;
```

### **WillSerializeNameHashes**

Whether or not this asset serializes hashes in the name map.
 If null, this will be automatically determined based on the object version.

```csharp
public Nullable<bool> WillSerializeNameHashes;
```

### **Imports**

Map of object imports. UAssetAPI used to call these "links."

```csharp
public List<Import> Imports;
```

### **DependsMap**

List of dependency lists for each export.

```csharp
public List<Int32[]> DependsMap;
```

### **SoftPackageReferenceList**

List of packages that are soft referenced by this package.

```csharp
public List<FString> SoftPackageReferenceList;
```

### **AssetRegistryData**

Uncertain

```csharp
public Byte[] AssetRegistryData;
```

### **ValorantGarbageData**

Some garbage data that appears to be present in certain games (e.g. Valorant)

```csharp
public Byte[] ValorantGarbageData;
```

### **Generations**

Data about previous versions of this package.

```csharp
public List<FGenerationInfo> Generations;
```

### **PackageGuid**

Current ID for this package. Effectively unused.

```csharp
public Guid PackageGuid;
```

### **RecordedEngineVersion**

Engine version this package was saved with. This may differ from CompatibleWithEngineVersion for assets saved with a hotfix release.

```csharp
public FEngineVersion RecordedEngineVersion;
```

### **RecordedCompatibleWithEngineVersion**

Engine version this package is compatible with. Assets saved by Hotfix releases and engine versions that maintain binary compatibility will have
 a CompatibleWithEngineVersion.Patch that matches the original release (as opposed to SavedByEngineVersion which will have a patch version of the new release).

```csharp
public FEngineVersion RecordedCompatibleWithEngineVersion;
```

### **ChunkIDs**

Streaming install ChunkIDs

```csharp
public Int32[] ChunkIDs;
```

### **PackageSource**

Value that is used by the Unreal Engine to determine if the package was saved by Epic, a licensee, modder, etc.

```csharp
public uint PackageSource;
```

### **FolderName**

The Generic Browser folder name that this package lives in. Usually "None" in cooked assets.

```csharp
public FString FolderName;
```

### **OverrideNameMapHashes**

A map of name map entries to hashes to use when serializing instead of the default engine hash algorithm. Useful when external programs improperly specify name map hashes and binary equality must be maintained.

```csharp
public Dictionary<FString, uint> OverrideNameMapHashes;
```

### **UASSET_MAGIC**

Magic number for the .uasset format

```csharp
public static uint UASSET_MAGIC;
```

### **ACE7_MAGIC**

Magic number for Ace Combat 7 encrypted .uasset format

```csharp
public static uint ACE7_MAGIC;
```

### **Info**

Agent string to provide context in serialized JSON.

```csharp
public string Info;
```

### **FilePath**

The path of the file on disk that this asset represents. This does not need to be specified for regular parsing.

```csharp
public string FilePath;
```

### **Mappings**

The corresponding mapping data for the game that this asset is from. Optional unless unversioned properties are present.

```csharp
public Usmap Mappings;
```

### **UseSeparateBulkDataFiles**

Should the asset be split into separate .uasset, .uexp, and .ubulk files, as opposed to one single .uasset file?

```csharp
public bool UseSeparateBulkDataFiles;
```

### **IsUnversioned**

Should this asset not serialize its engine and custom versions?

```csharp
public bool IsUnversioned;
```

### **FileVersionLicenseeUE**

The licensee file version. Used by some games to add their own Engine-level versioning.

```csharp
public int FileVersionLicenseeUE;
```

### **ObjectVersion**

The object version of UE4 that will be used to parse this asset.

```csharp
public ObjectVersion ObjectVersion;
```

### **ObjectVersionUE5**

The object version of UE5 that will be used to parse this asset. Set to [ObjectVersionUE5.UNKNOWN](./uassetapi.unrealtypes.objectversionue5.md#unknown) for UE4 games.

```csharp
public ObjectVersionUE5 ObjectVersionUE5;
```

### **CustomVersionContainer**

All the custom versions stored in the archive.

```csharp
public List<CustomVersion> CustomVersionContainer;
```

### **Exports**

Map of object exports. UAssetAPI used to call these "categories."

```csharp
public List<Export> Exports;
```

### **WorldTileInfo**

Tile information used by WorldComposition.
 Defines properties necessary for tile positioning in the world.

```csharp
public FWorldTileInfo WorldTileInfo;
```

### **MapStructTypeOverride**

In MapProperties that have StructProperties as their keys or values, there is no universal, context-free way to determine the type of the struct.



To that end, this dictionary maps MapProperty names to the type of the structs within them (tuple of key struct type and value struct type) if they are not None-terminated property lists.

```csharp
public Dictionary<string, Tuple<FString, FString>> MapStructTypeOverride;
```

### **ArrayStructTypeOverride**

IN ENGINE VERSIONS BEFORE [ObjectVersion.VER_UE4_INNER_ARRAY_TAG_INFO](./uassetapi.unrealtypes.objectversion.md#ver_ue4_inner_array_tag_info):



In ArrayProperties that have StructProperties as their keys or values, there is no universal, context-free way to determine the type of the struct. To that end, this dictionary maps ArrayProperty names to the type of the structs within them.

```csharp
public Dictionary<string, FString> ArrayStructTypeOverride;
```

## Properties

### **PackageFlags**

The flags for this package.

```csharp
public EPackageFlags PackageFlags { get; set; }
```

#### Property Value

[EPackageFlags](./uassetapi.unrealtypes.epackageflags.md)<br>

### **HasUnversionedProperties**

Whether or not this asset uses unversioned properties.

```csharp
public bool HasUnversionedProperties { get; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

## Constructors

### **UAsset(String, EngineVersion, Usmap)**

Reads an asset from disk and initializes a new instance of the [UAsset](./uassetapi.uasset.md) class to store its data in memory.

```csharp
public UAsset(string path, EngineVersion engineVersion, Usmap mappings)
```

#### Parameters

`path` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
The path of the asset file on disk that this instance will read from.

`engineVersion` [EngineVersion](./uassetapi.unrealtypes.engineversion.md)<br>
The version of the Unreal Engine that will be used to parse this asset. If the asset is versioned, this can be left unspecified.

`mappings` [Usmap](./uassetapi.unversioned.usmap.md)<br>
A valid set of mappings for the game that this asset is from. Not required unless unversioned properties are used.

#### Exceptions

[UnknownEngineVersionException](./uassetapi.unknownengineversionexception.md)<br>
Thrown when this is an unversioned asset and  is unspecified.

[FormatException](https://docs.microsoft.com/en-us/dotnet/api/system.formatexception)<br>
Throw when the asset cannot be parsed correctly.

### **UAsset(AssetBinaryReader, EngineVersion, Usmap, Boolean)**

Reads an asset from a BinaryReader and initializes a new instance of the [UAsset](./uassetapi.uasset.md) class to store its data in memory.

```csharp
public UAsset(AssetBinaryReader reader, EngineVersion engineVersion, Usmap mappings, bool useSeparateBulkDataFiles)
```

#### Parameters

`reader` [AssetBinaryReader](./uassetapi.assetbinaryreader.md)<br>
The asset's BinaryReader that this instance will read from. If a .uexp file exists, the .uexp file's data should be appended to the end of the .uasset file's data.

`engineVersion` [EngineVersion](./uassetapi.unrealtypes.engineversion.md)<br>
The version of the Unreal Engine that will be used to parse this asset. If the asset is versioned, this can be left unspecified.

`mappings` [Usmap](./uassetapi.unversioned.usmap.md)<br>
A valid set of mappings for the game that this asset is from. Not required unless unversioned properties are used.

`useSeparateBulkDataFiles` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
Does this asset uses separate bulk data files (.uexp, .ubulk)?

#### Exceptions

[UnknownEngineVersionException](./uassetapi.unknownengineversionexception.md)<br>
Thrown when this is an unversioned asset and  is unspecified.

[FormatException](https://docs.microsoft.com/en-us/dotnet/api/system.formatexception)<br>
Throw when the asset cannot be parsed correctly.

### **UAsset(EngineVersion, Usmap)**

Initializes a new instance of the [UAsset](./uassetapi.uasset.md) class. This instance will store no asset data and does not represent any asset in particular until the [UAsset.Read(AssetBinaryReader, Int32[], Int32[])](./uassetapi.uasset.md#readassetbinaryreader-int32-int32) method is manually called.

```csharp
public UAsset(EngineVersion engineVersion, Usmap mappings)
```

#### Parameters

`engineVersion` [EngineVersion](./uassetapi.unrealtypes.engineversion.md)<br>
The version of the Unreal Engine that will be used to parse this asset. If the asset is versioned, this can be left unspecified.

`mappings` [Usmap](./uassetapi.unversioned.usmap.md)<br>
A valid set of mappings for the game that this asset is from. Not required unless unversioned properties are used.

### **UAsset(String, ObjectVersion, ObjectVersionUE5, List&lt;CustomVersion&gt;, Usmap)**

Reads an asset from disk and initializes a new instance of the [UAsset](./uassetapi.uasset.md) class to store its data in memory.

```csharp
public UAsset(string path, ObjectVersion objectVersion, ObjectVersionUE5 objectVersionUE5, List<CustomVersion> customVersionContainer, Usmap mappings)
```

#### Parameters

`path` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
The path of the asset file on disk that this instance will read from.

`objectVersion` [ObjectVersion](./uassetapi.unrealtypes.objectversion.md)<br>
The UE4 object version of the Unreal Engine that will be used to parse this asset.

`objectVersionUE5` [ObjectVersionUE5](./uassetapi.unrealtypes.objectversionue5.md)<br>
The UE5 object version of the Unreal Engine that will be used to parse this asset.

`customVersionContainer` [List&lt;CustomVersion&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1)<br>
A list of custom versions to parse this asset with.

`mappings` [Usmap](./uassetapi.unversioned.usmap.md)<br>
A valid set of mappings for the game that this asset is from. Not required unless unversioned properties are used.

#### Exceptions

[UnknownEngineVersionException](./uassetapi.unknownengineversionexception.md)<br>
Thrown when this is an unversioned asset and  is unspecified.

[FormatException](https://docs.microsoft.com/en-us/dotnet/api/system.formatexception)<br>
Throw when the asset cannot be parsed correctly.

### **UAsset(AssetBinaryReader, ObjectVersion, ObjectVersionUE5, List&lt;CustomVersion&gt;, Usmap, Boolean)**

Reads an asset from a BinaryReader and initializes a new instance of the [UAsset](./uassetapi.uasset.md) class to store its data in memory.

```csharp
public UAsset(AssetBinaryReader reader, ObjectVersion objectVersion, ObjectVersionUE5 objectVersionUE5, List<CustomVersion> customVersionContainer, Usmap mappings, bool useSeparateBulkDataFiles)
```

#### Parameters

`reader` [AssetBinaryReader](./uassetapi.assetbinaryreader.md)<br>
The asset's BinaryReader that this instance will read from.

`objectVersion` [ObjectVersion](./uassetapi.unrealtypes.objectversion.md)<br>
The UE4 object version of the Unreal Engine that will be used to parse this asset.

`objectVersionUE5` [ObjectVersionUE5](./uassetapi.unrealtypes.objectversionue5.md)<br>
The UE5 object version of the Unreal Engine that will be used to parse this asset.

`customVersionContainer` [List&lt;CustomVersion&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1)<br>
A list of custom versions to parse this asset with.

`mappings` [Usmap](./uassetapi.unversioned.usmap.md)<br>
A valid set of mappings for the game that this asset is from. Not required unless unversioned properties are used.

`useSeparateBulkDataFiles` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
Does this asset uses separate bulk data files (.uexp, .ubulk)?

#### Exceptions

[UnknownEngineVersionException](./uassetapi.unknownengineversionexception.md)<br>
Thrown when this is an unversioned asset and  is unspecified.

[FormatException](https://docs.microsoft.com/en-us/dotnet/api/system.formatexception)<br>
Throw when the asset cannot be parsed correctly.

### **UAsset(ObjectVersion, ObjectVersionUE5, List&lt;CustomVersion&gt;, Usmap)**

Initializes a new instance of the [UAsset](./uassetapi.uasset.md) class. This instance will store no asset data and does not represent any asset in particular until the [UAsset.Read(AssetBinaryReader, Int32[], Int32[])](./uassetapi.uasset.md#readassetbinaryreader-int32-int32) method is manually called.

```csharp
public UAsset(ObjectVersion objectVersion, ObjectVersionUE5 objectVersionUE5, List<CustomVersion> customVersionContainer, Usmap mappings)
```

#### Parameters

`objectVersion` [ObjectVersion](./uassetapi.unrealtypes.objectversion.md)<br>
The UE4 object version of the Unreal Engine that will be used to parse this asset.

`objectVersionUE5` [ObjectVersionUE5](./uassetapi.unrealtypes.objectversionue5.md)<br>
The UE5 object version of the Unreal Engine that will be used to parse this asset.

`customVersionContainer` [List&lt;CustomVersion&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1)<br>
A list of custom versions to parse this asset with.

`mappings` [Usmap](./uassetapi.unversioned.usmap.md)<br>
A valid set of mappings for the game that this asset is from. Not required unless unversioned properties are used.

### **UAsset()**

Initializes a new instance of the [UAsset](./uassetapi.uasset.md) class. This instance will store no asset data and does not represent any asset in particular until the [UAsset.Read(AssetBinaryReader, Int32[], Int32[])](./uassetapi.uasset.md#readassetbinaryreader-int32-int32) method is manually called.

```csharp
public UAsset()
```

## Methods

### **VerifyBinaryEquality()**

Checks whether or not this asset maintains binary equality when serialized.

```csharp
public bool VerifyBinaryEquality()
```

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
Whether or not the asset maintained binary equality.

### **GetParentClass(FName&, FName&)**

Finds the class path and export name of the SuperStruct of this asset, if it exists.

```csharp
public void GetParentClass(FName& parentClassPath, FName& parentClassExportName)
```

#### Parameters

`parentClassPath` [FName&](./uassetapi.unrealtypes.fname&.md)<br>
The class path of the SuperStruct of this asset, if it exists.

`parentClassExportName` [FName&](./uassetapi.unrealtypes.fname&.md)<br>
The export name of the SuperStruct of this asset, if it exists.

### **GetParentClassExportName()**

```csharp
internal FName GetParentClassExportName()
```

#### Returns

[FName](./uassetapi.unrealtypes.fname.md)<br>

### **AddImport(Import)**

Adds a new import to the import map. This is equivalent to adding directly to the [UAsset.Imports](./uassetapi.uasset.md#imports) list.

```csharp
public FPackageIndex AddImport(Import li)
```

#### Parameters

`li` [Import](./uassetapi.import.md)<br>
The new import to add to the import map.

#### Returns

[FPackageIndex](./uassetapi.unrealtypes.fpackageindex.md)<br>
The FPackageIndex corresponding to the newly-added import.

### **SearchForImport(FName, FName, FPackageIndex, FName)**

Searches for an import in the import map based off of certain parameters.

```csharp
public int SearchForImport(FName classPackage, FName className, FPackageIndex outerIndex, FName objectName)
```

#### Parameters

`classPackage` [FName](./uassetapi.unrealtypes.fname.md)<br>
The ClassPackage that the requested import will have.

`className` [FName](./uassetapi.unrealtypes.fname.md)<br>
The ClassName that the requested import will have.

`outerIndex` [FPackageIndex](./uassetapi.unrealtypes.fpackageindex.md)<br>
The CuterIndex that the requested import will have.

`objectName` [FName](./uassetapi.unrealtypes.fname.md)<br>
The ObjectName that the requested import will have.

#### Returns

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>
The index of the requested import in the name map, or zero if one could not be found.

### **SearchForImport(FName, FName, FName)**

Searches for an import in the import map based off of certain parameters.

```csharp
public int SearchForImport(FName classPackage, FName className, FName objectName)
```

#### Parameters

`classPackage` [FName](./uassetapi.unrealtypes.fname.md)<br>
The ClassPackage that the requested import will have.

`className` [FName](./uassetapi.unrealtypes.fname.md)<br>
The ClassName that the requested import will have.

`objectName` [FName](./uassetapi.unrealtypes.fname.md)<br>
The ObjectName that the requested import will have.

#### Returns

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>
The index of the requested import in the name map, or zero if one could not be found.

### **SearchForImport(FName)**

Searches for an import in the import map based off of certain parameters.

```csharp
public int SearchForImport(FName objectName)
```

#### Parameters

`objectName` [FName](./uassetapi.unrealtypes.fname.md)<br>
The ObjectName that the requested import will have.

#### Returns

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>
The index of the requested import in the name map, or zero if one could not be found.

### **CopySplitUp(Stream, Stream, Int32, Int32)**

Copies a portion of a stream to another stream.

```csharp
internal static void CopySplitUp(Stream input, Stream output, int start, int leng)
```

#### Parameters

`input` [Stream](https://docs.microsoft.com/en-us/dotnet/api/system.io.stream)<br>
The input stream.

`output` [Stream](https://docs.microsoft.com/en-us/dotnet/api/system.io.stream)<br>
The output stream.

`start` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>
The offset in the input stream to start copying from.

`leng` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>
The length in bytes of the data to be copied.

### **Read(AssetBinaryReader, Int32[], Int32[])**

Reads an asset into memory.

```csharp
public void Read(AssetBinaryReader reader, Int32[] manualSkips, Int32[] forceReads)
```

#### Parameters

`reader` [AssetBinaryReader](./uassetapi.assetbinaryreader.md)<br>
The input reader.

`manualSkips` [Int32[]](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>
An array of export indexes to skip parsing. For most applications, this should be left blank.

`forceReads` [Int32[]](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>
An array of export indexes that must be read, overriding entries in the manualSkips parameter. For most applications, this should be left blank.

#### Exceptions

[UnknownEngineVersionException](./uassetapi.unknownengineversionexception.md)<br>
Thrown when this is an unversioned asset and  is unspecified.

[FormatException](https://docs.microsoft.com/en-us/dotnet/api/system.formatexception)<br>
Throw when the asset cannot be parsed correctly.

### **WriteData()**

Serializes an asset from memory.

```csharp
public MemoryStream WriteData()
```

#### Returns

[MemoryStream](https://docs.microsoft.com/en-us/dotnet/api/system.io.memorystream)<br>
A new MemoryStream containing the full binary data of the serialized asset.

### **Write(String)**

Serializes and writes an asset to disk from memory.

```csharp
public void Write(string outputPath)
```

#### Parameters

`outputPath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
The path on disk to write the asset to.

#### Exceptions

[UnknownEngineVersionException](./uassetapi.unknownengineversionexception.md)<br>
Thrown when  is unspecified.

### **SerializeJson(Boolean)**

Serializes this asset as JSON.

```csharp
public string SerializeJson(bool isFormatted)
```

#### Parameters

`isFormatted` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
Whether or not the returned JSON string should be indented.

#### Returns

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
A serialized JSON string that represents the asset.

### **SerializeJson(Formatting)**

Serializes this asset as JSON.

```csharp
public string SerializeJson(Formatting jsonFormatting)
```

#### Parameters

`jsonFormatting` Formatting<br>
The formatting to use for the returned JSON string.

#### Returns

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
A serialized JSON string that represents the asset.

### **SerializeJsonObject(Object, Boolean)**

Serializes an object as JSON.

```csharp
public string SerializeJsonObject(object value, bool isFormatted)
```

#### Parameters

`value` [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)<br>
The object to serialize as JSON.

`isFormatted` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
Whether or not the returned JSON string should be indented.

#### Returns

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
A serialized JSON string that represents the object.

### **SerializeJsonObject(Object, Formatting)**

Serializes an object as JSON.

```csharp
public string SerializeJsonObject(object value, Formatting jsonFormatting)
```

#### Parameters

`value` [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)<br>
The object to serialize as JSON.

`jsonFormatting` Formatting<br>
The formatting to use for the returned JSON string.

#### Returns

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
A serialized JSON string that represents the object.

### **DeserializeJsonObject(String)**

Deserializes an object from JSON.

```csharp
public object DeserializeJsonObject(string json)
```

#### Parameters

`json` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
A serialized JSON string to parse.

#### Returns

[Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)<br>

### **DeserializeJson(String)**

Reads an asset from serialized JSON and initializes a new instance of the [UAsset](./uassetapi.uasset.md) class to store its data in memory.

```csharp
public static UAsset DeserializeJson(string json)
```

#### Parameters

`json` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
A serialized JSON string to parse.

#### Returns

[UAsset](./uassetapi.uasset.md)<br>

### **DeserializeJson(Stream)**

Reads an asset from serialized JSON and initializes a new instance of the [UAsset](./uassetapi.uasset.md) class to store its data in memory.

```csharp
public static UAsset DeserializeJson(Stream stream)
```

#### Parameters

`stream` [Stream](https://docs.microsoft.com/en-us/dotnet/api/system.io.stream)<br>
A stream containing serialized JSON string to parse.

#### Returns

[UAsset](./uassetapi.uasset.md)<br>
