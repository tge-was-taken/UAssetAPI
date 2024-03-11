using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UAssetAPI.ExportTypes;
using UAssetAPI.UnrealTypes;
using UAssetAPI.Unversioned;

namespace UAssetAPI.IO
{
    public struct FInternalArc
    {
        public int FromExportBundleIndex;
        public int ToExportBundleIndex;

        public static FInternalArc Read(AssetBinaryReader reader)
        {
            var res = new FInternalArc();
            res.FromExportBundleIndex = reader.ReadInt32();
            res.ToExportBundleIndex = reader.ReadInt32();
            return res;
        }

        public static int Write(AssetBinaryWriter writer, int v2, int v3)
        {
            writer.Write(v2);
            writer.Write(v3);
            return sizeof(int) * 2;
        }

        public int Write(AssetBinaryWriter writer)
        {
            return FInternalArc.Write(writer, FromExportBundleIndex, ToExportBundleIndex);
        }
    }

    public struct FExternalArc
    {
        public int FromImportIndex;
        EExportCommandType FromCommandType;
        public int ToExportBundleIndex;

        public static FExternalArc Read(AssetBinaryReader reader)
        {
            var res = new FExternalArc();
            res.FromImportIndex = reader.ReadInt32();
            res.FromCommandType = (EExportCommandType)reader.ReadByte();
            res.ToExportBundleIndex = reader.ReadInt32();
            return res;
        }

        public static int Write(AssetBinaryWriter writer, int v1, EExportCommandType v2, int v3)
        {
            writer.Write(v1);
            writer.Write((byte)v2);
            writer.Write(v3);
            return sizeof(int) + sizeof(uint) + sizeof(int);
        }

        public int Write(AssetBinaryWriter writer)
        {
            return FExternalArc.Write(writer, FromImportIndex, FromCommandType, ToExportBundleIndex);
        }
    }

    public enum EExportCommandType : uint
    {
        ExportCommandType_Create,
        ExportCommandType_Serialize,
        ExportCommandType_Count
    }

    public struct FExportBundleHeader
    {
        public ulong SerialOffset;
        public uint FirstEntryIndex;
        public uint EntryCount;

        public static FExportBundleHeader Read(AssetBinaryReader reader)
        {
            var res = new FExportBundleHeader();
            res.SerialOffset = reader.Asset.ObjectVersionUE5 > ObjectVersionUE5.UNKNOWN ? reader.ReadUInt64() : ulong.MaxValue;
            res.FirstEntryIndex = reader.ReadUInt32();
            res.EntryCount = reader.ReadUInt32();
            return res;
        }

        public static int Write(AssetBinaryWriter writer, ulong v1, uint v2, uint v3)
        {
            if (writer.Asset.ObjectVersionUE5 > ObjectVersionUE5.UNKNOWN) writer.Write(v1);
            writer.Write(v2);
            writer.Write(v3);
            return (writer.Asset.ObjectVersionUE5 > ObjectVersionUE5.UNKNOWN ? sizeof(ulong) : 0) + sizeof(uint) * 2;
        }

        public int Write(AssetBinaryWriter writer)
        {
            return FExportBundleHeader.Write(writer, SerialOffset, FirstEntryIndex, EntryCount);
        }
    }    

    public struct FExportBundleEntry
    {
        public uint LocalExportIndex;
        public EExportCommandType CommandType;

        public static FExportBundleEntry Read(AssetBinaryReader reader)
        {
            var res = new FExportBundleEntry();
            res.LocalExportIndex = reader.ReadUInt32();
            res.CommandType = (EExportCommandType)reader.ReadUInt32();
            return res;
        }

        public static int Write(AssetBinaryWriter writer, uint lei, EExportCommandType typ)
        {
            writer.Write((uint)lei);
            writer.Write((uint)typ);
            return sizeof(uint) * 2;
        }

        public int Write(AssetBinaryWriter writer)
        {
            return FExportBundleEntry.Write(writer, LocalExportIndex, CommandType);
        }
    }

    public enum EZenPackageVersion : uint
    {
        Initial,

        LatestPlusOne,
        Latest = LatestPlusOne - 1
    }

    public class FSerializedNameHeader
    {
        public bool bIsWide;
        public int Len;

        public static FSerializedNameHeader Read(BinaryReader reader)
        {
            var b1 = reader.ReadByte();
            var b2 = reader.ReadByte();

            var res = new FSerializedNameHeader();
            res.bIsWide = (b1 & (byte)0x80) > 0;
            res.Len = ((b1 & (byte)0x7F) << 8) + b2;
            return res;
        }

        public static void Write(BinaryWriter writer, bool bIsWideVal, int lenVal)
        {
            byte b1 = (byte)(((byte)(bIsWideVal ? 1 : 0)) << 7 | (byte)(lenVal >> 8));
            byte b2 = (byte)lenVal;
            writer.Write(b1); writer.Write(b2);
        }

        public void Write(BinaryWriter writer)
        {
            FSerializedNameHeader.Write(writer, bIsWide, Len);
        }
    }

    public class ImportedPackage
    {
        public ulong PackageId { get; set; }
        public List<FExternalArc> ExternalArcs { get; set; } = new List<FExternalArc>();
    }

    public class ZenAsset : UnrealPackage
    {
        /// <summary>
        /// The global data of the game that this asset is from.
        /// </summary>
        public IOGlobalData GlobalData;

        public EZenPackageVersion ZenVersion;
        public FName Name;
        public FName SourceName;
        /// <summary>
        /// Should serialized hashes be verified on read?
        /// </summary>
        public bool VerifyHashes = false;
        public ulong HashVersion = UnrealBinaryReader.CityHash64;
        public byte[] BulkDataMap;

        public ulong[] ImportedPublicExportHashes;

        /// <summary>
        /// Map of object imports. UAssetAPI used to call these "links."
        /// </summary>
        public List<FPackageObjectIndex> Imports = new List<FPackageObjectIndex>();

        private Dictionary<ulong, string> CityHash64Map = new Dictionary<ulong, string>();

        public List<FExportBundleHeader> ExportBundleHeaders { get; private set; } = new List<FExportBundleHeader>();
        public List<FExportBundleEntry> ExportBundleEntries { get; private set; } = new List<FExportBundleEntry>();
        public List<ImportedPackage> ImportedPackages { get; private set; } = new List<ImportedPackage>();

        private void AddCityHash64MapEntryRaw(string val)
        {
            ulong hsh = CRCGenerator.GenerateImportHashFromObjectPath(val);
            if (CityHash64Map.ContainsKey(hsh))
            {
                if (CRCGenerator.ToLower(CityHash64Map[hsh]) == CRCGenerator.ToLower(val)) return;
                throw new FormatException("CityHash64 hash collision between \"" + CityHash64Map[hsh] + "\" and \"" + val + "\"");
            }
            CityHash64Map.Add(hsh, val);
        }
        public string GetStringFromCityHash64(ulong val)
        {
            if (CityHash64Map.ContainsKey(val)) return CityHash64Map[val];
            if (Mappings.CityHash64Map.ContainsKey(val)) return Mappings.CityHash64Map[val];
            return null;
        }

        /// <summary>
        /// Finds the class path and export name of the SuperStruct of this asset, if it exists.
        /// </summary>
        /// <param name="parentClassPath">The class path of the SuperStruct of this asset, if it exists.</param>
        /// <param name="parentClassExportName">The export name of the SuperStruct of this asset, if it exists.</param>
        public override void GetParentClass(out FName parentClassPath, out FName parentClassExportName)
        {
            throw new NotImplementedException("Unimplemented method ZenAsset.GetParentClass");
        }

        internal override FName GetParentClassExportName()
        {
            throw new NotImplementedException("Unimplemented method ZenAsset.GetParentClassExportName");
        }

        /// <summary>
        /// Reads an asset into memory.
        /// </summary>
        /// <param name="reader">The input reader.</param>
        /// <param name="manualSkips">An array of export indexes to skip parsing. For most applications, this should be left blank.</param>
        /// <param name="forceReads">An array of export indexes that must be read, overriding entries in the manualSkips parameter. For most applications, this should be left blank.</param>
        /// <exception cref="UnknownEngineVersionException">Thrown when <see cref="ObjectVersion"/> is unspecified.</exception>
        /// <exception cref="FormatException">Throw when the asset cannot be parsed correctly.</exception>
        public override void Read(AssetBinaryReader reader, int[] manualSkips = null, int[] forceReads = null)
        {
            if (ObjectVersion == ObjectVersion.UNKNOWN) throw new UnknownEngineVersionException("Cannot begin serialization before an object version is specified");

            FInternalArc[] internalArcs = null;
            FExternalArc[][] externalArcs = null; // the index is the same as the index into the ImportedPackageIds map
            uint CookedHeaderSize;
            if (ObjectVersionUE5 >= ObjectVersionUE5.INITIAL_VERSION)
            {
                IsUnversioned = reader.ReadUInt32() == 0;
                uint HeaderSize = reader.ReadUInt32();
                Name = reader.ReadFName();
                PackageFlags = (EPackageFlags)reader.ReadUInt32();
                CookedHeaderSize = reader.ReadUInt32(); // where does this number come from?
                int ImportedPublicExportHashesOffset = reader.ReadInt32();
                int ImportMapOffset = reader.ReadInt32();
                int ExportMapOffset = reader.ReadInt32();
                int ExportBundleEntriesOffset = reader.ReadInt32();
                int GraphDataOffset = reader.ReadInt32();

                if (!IsUnversioned)
                {
                    ZenVersion = (EZenPackageVersion)reader.ReadUInt32();
                    ObjectVersion = (ObjectVersion)reader.ReadInt32();
                    ObjectVersionUE5 = (ObjectVersionUE5)reader.ReadInt32();
                    FileVersionLicenseeUE = reader.ReadInt32();
                    CustomVersionContainer = reader.ReadCustomVersionContainer(ECustomVersionSerializationFormat.Optimized, CustomVersionContainer, Mappings);
                }

                // name map batch
                reader.ReadNameBatch(VerifyHashes, out HashVersion, out List<FString> tempNameMap);
                ClearNameIndexList();
                foreach (var entry in tempNameMap)
                {
                    AddCityHash64MapEntryRaw(entry.Value);
                    AddNameReference(entry, true);
                }

                // bulk data map
                if (ObjectVersionUE5 >= ObjectVersionUE5.DATA_RESOURCES)
                {
                    ulong bulkDataMapSize = reader.ReadUInt64();
                    BulkDataMap = reader.ReadBytes((int)bulkDataMapSize); // i don't like this cast; if it becomes a problem, we can use a workaround instead
                }

                // imported public export hashes
                reader.BaseStream.Seek(ImportedPublicExportHashesOffset, SeekOrigin.Begin);
                ImportedPublicExportHashes = new ulong[(ImportMapOffset - ImportedPublicExportHashesOffset) / sizeof(ulong)];
                for (int i = 0; i < ImportedPublicExportHashes.Length; i++) ImportedPublicExportHashes[i] = reader.ReadUInt64();

                // import map
                reader.BaseStream.Seek(ImportMapOffset, SeekOrigin.Begin);
                Imports = new List<FPackageObjectIndex>();
                for (int i = 0; i < (ExportMapOffset - ImportMapOffset) / sizeof(ulong); i++)
                {
                    Imports.Add(FPackageObjectIndex.Read(reader));
                }

                // export map
                reader.BaseStream.Seek(ExportMapOffset, SeekOrigin.Begin);
                Exports = new List<Export>();
                int exportMapEntrySize = (int)Export.GetExportMapEntrySize(this);
                for (int i = 0; i < (ExportBundleEntriesOffset - ExportMapOffset) / exportMapEntrySize; i++)
                {
                    var newExport = new Export(this, new byte[0]);
                    newExport.ReadExportMapEntry(reader);
                    Exports.Add(newExport);
                }

                // export bundle entries; gives order that exports should be serialized
                reader.BaseStream.Seek(ExportBundleEntriesOffset, SeekOrigin.Begin);
                long startPos = reader.BaseStream.Position;
                var exportBundleCount = Exports.Count * (int)(uint)EExportCommandType.ExportCommandType_Count;
                ExportBundleEntries = new List<FExportBundleEntry>(exportBundleCount);
                for (int i = 0; i < exportBundleCount; i++) ExportBundleEntries.Add(FExportBundleEntry.Read(reader));
                long endPos = reader.BaseStream.Position;
                if (endPos != GraphDataOffset) throw new FormatException("extra padding is needed; please report if you see this error message");
                //reader.ReadBytes((int)UAPUtils.AlignPadding(endPos - startPos, 8));

                // graph data, once this is implemented fix FPackageIndex ToImport & GetParentClass
                reader.BaseStream.Seek(GraphDataOffset, SeekOrigin.Begin);
                ExportBundleHeaders = new List<FExportBundleHeader>();
                int numEntriesTotal = 0;
                while (numEntriesTotal < exportBundleCount)
                {
                    var nxt = FExportBundleHeader.Read(reader);
                    numEntriesTotal += (int)nxt.EntryCount;
                    ExportBundleHeaders.Add(nxt);
                }

                int numInternalArcs = reader.ReadInt32();
                internalArcs = new FInternalArc[numInternalArcs];
                for (int i = 0; i < numInternalArcs; i++) internalArcs[i] = FInternalArc.Read(reader);

                var externalArcsList = new List<FExternalArc[]>();
                while (reader.BaseStream.Position < HeaderSize)
                {
                    int numExternalArcs = reader.ReadInt32();
                    var externalArcsThese = new FExternalArc[numExternalArcs];
                    for (int i = 0; i < numExternalArcs; i++) externalArcsThese[i] = FExternalArc.Read(reader);
                    externalArcsList.Add(externalArcsThese);
                }
                externalArcs = externalArcsList.ToArray();
            }
            else
            {
                // Read PackageSummary
                Name = reader.ReadFName();
                SourceName = reader.ReadFName();
                PackageFlags = (EPackageFlags)reader.ReadUInt32();
                CookedHeaderSize = reader.ReadUInt32();
                int NameMapNamesOffset = reader.ReadInt32();
                int NameMapNamesSize = reader.ReadInt32();
                int NameMapHashesOffset = reader.ReadInt32();
                int NameMapHashesSize = reader.ReadInt32();
                int ImportMapOffset = reader.ReadInt32();
                int ExportMapOffset = reader.ReadInt32();
                int ExportBundlesOffset = reader.ReadInt32();
                int GraphDataOffset = reader.ReadInt32();
                int GraphDataSize = reader.ReadInt32();

                // Read name map
                var nameMapEntryCount = (NameMapHashesSize / 8) - 1;
                reader.BaseStream.Seek(NameMapHashesOffset, SeekOrigin.Begin);
                HashVersion = reader.ReadUInt64();
                var nameMapHashes = new ulong[nameMapEntryCount];
                for (int i = 0; i < nameMapHashes.Length; i++)
                    nameMapHashes[i] = reader.ReadUInt64();

                reader.BaseStream.Seek(NameMapNamesOffset, SeekOrigin.Begin);
                ClearNameIndexList();
                for (int i = 0; i < nameMapEntryCount; i++)
                {
                    var header = FSerializedNameHeader.Read(reader);
                    var str = reader.ReadNameMapString(header, out var hashes);
                    AddCityHash64MapEntryRaw(str.Value);
                    AddNameReference(str, true);
                }

                // import map
                reader.BaseStream.Seek(ImportMapOffset, SeekOrigin.Begin);
                Imports = new List<FPackageObjectIndex>();
                for (int i = 0; i < (ExportMapOffset - ImportMapOffset) / sizeof(ulong); i++)
                {
                    Imports.Add(FPackageObjectIndex.Read(reader));
                }

                // export map
                reader.BaseStream.Seek(ExportMapOffset, SeekOrigin.Begin);
                Exports = new List<Export>();
                int exportMapEntrySize = (int)Export.GetExportMapEntrySize(this);
                for (int i = 0; i < (ExportBundlesOffset - ExportMapOffset) / exportMapEntrySize; i++)
                {
                    var newExport = new Export(this, new byte[0]);
                    newExport.ReadExportMapEntry(reader);
                    Exports.Add(newExport);
                }

                reader.BaseStream.Seek(ExportBundlesOffset, SeekOrigin.Begin);
                var exportBundlesSize = GraphDataOffset - ExportBundlesOffset;
                var exportBundleCount = exportBundlesSize / 8;
                var remainingBundleEntryCount = exportBundleCount;
                var foundBundlesCount = 0u;
                ExportBundleHeaders = new List<FExportBundleHeader>();
                while (foundBundlesCount < remainingBundleEntryCount)
                {
                    remainingBundleEntryCount--;
                    var exportBundleHeader = FExportBundleHeader.Read(reader);
                    foundBundlesCount += exportBundleHeader.EntryCount;
                    ExportBundleHeaders.Add(exportBundleHeader);
                }

                ExportBundleEntries = new List<FExportBundleEntry>((int)foundBundlesCount);
                for (int i = 0; i < foundBundlesCount; i++)
                    ExportBundleEntries.Add(FExportBundleEntry.Read(reader));

                // TODO save this data soemwhere
                reader.BaseStream.Seek(GraphDataOffset, SeekOrigin.Begin);
                var importedPackageCount = reader.ReadInt32();
                for (int i = 0; i < importedPackageCount; i++)
                {
                    var importedPackageId = reader.ReadUInt64();
                    var externalArcCount = reader.ReadInt32();
                    var importedPackage = new ImportedPackage() { PackageId = importedPackageId };
                    for (int j = 0; j < externalArcCount; j++)
                    {
                        var fromExportBundleIndex = reader.ReadInt32();
                        var toExportBundleIndex = reader.ReadInt32();
                        importedPackage.ExternalArcs.Add(new FExternalArc() { FromImportIndex = fromExportBundleIndex, ToExportBundleIndex = toExportBundleIndex });
                    }
                    ImportedPackages.Add(importedPackage);
                }
            }

            // end summary
            var cookedSerialOffsetBase = (Exports[0]?.SerialOffset ?? 0) - reader.BaseStream.Position;
            
            foreach (FExportBundleHeader headr in ExportBundleHeaders)
            {
                for (uint i = 0u; i < headr.EntryCount; i++)
                {
                    FExportBundleEntry entry = ExportBundleEntries[(int)(headr.FirstEntryIndex + i)];
                    switch (entry.CommandType)
                    {
                        case EExportCommandType.ExportCommandType_Serialize:
                            var cookedSerialOffset = Exports[(int)entry.LocalExportIndex].SerialOffset - cookedSerialOffsetBase;
                            reader.BaseStream.Seek(cookedSerialOffset, SeekOrigin.Begin);
                            ConvertExportToChildExportAndRead(reader, (int)entry.LocalExportIndex);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Serializes an asset from memory.
        /// </summary>
        /// <returns>A stream that the asset has been serialized to.</returns>
        public override MemoryStream WriteData()
        {
            if (Exports == null || Exports.Count == 0) throw new InvalidOperationException("No exports to write.");
            if (ObjectVersion == ObjectVersion.UNKNOWN) throw new InvalidOperationException("Object version must be specified before serialization.");

            if (ObjectVersionUE5 >= ObjectVersionUE5.INITIAL_VERSION)
                throw new NotImplementedException();

            var stream = new MemoryStream();
            var writer = new AssetBinaryWriter(stream, this);
            var headerPos = writer.BaseStream.Position;
            writer.Seek(64, SeekOrigin.Current); // write later

            var nameMapNamesOffset = writer.BaseStream.Position;
            for (int i = 0; i < nameMapIndexList.Count; i++)
            {
                var name = nameMapIndexList[i];
                var isWide = name.Value.Any(x => x > 0x7F);
                FSerializedNameHeader.Write(writer, isWide, name.Value.Length);
                if (!isWide)
                {
                    writer.Write(Encoding.ASCII.GetBytes(name.Value));
                }
                else
                {
                    writer.Write(Encoding.Unicode.GetBytes(name.Value));
                }
            }
            var nameMapNamesSize = writer.BaseStream.Position - nameMapNamesOffset;
            writer.Align(8);

            var nameMapHashesOffset = writer.BaseStream.Position;
            writer.Write(HashVersion);
            for (int i = 0; i < nameMapIndexList.Count; i++)
            {
                var name = nameMapIndexList[i];
                writer.Write(CRCGenerator.CityHash64WithLower(name));
            }
            var nameMapHashesSize = writer.BaseStream.Position - nameMapHashesOffset;

            var importMapOffset = writer.BaseStream.Position;
            foreach (var import in Imports)
            {
                writer.Write(import.Hash);
            }

            var exportMapOffset = writer.BaseStream.Position;
            writer.Seek(72 * Exports.Count, SeekOrigin.Current); // write later

            var exportBundlesOffset = writer.BaseStream.Position;
            foreach (var item in ExportBundleHeaders)
            {
                writer.Write(item.FirstEntryIndex);
                writer.Write(item.EntryCount);
            }

            foreach (var item in ExportBundleEntries)
            {
                writer.Write(item.LocalExportIndex);
                writer.Write((int)item.CommandType);
            }

            var graphDataOffset = writer.BaseStream.Position;
            writer.Write(ImportedPackages.Count);
            foreach (var import in ImportedPackages)
            {
                writer.Write(import.PackageId);
                writer.Write(import.ExternalArcs.Count);
                foreach (var item in import.ExternalArcs)
                {
                    writer.Write(item.FromImportIndex);
                    writer.Write(item.ToExportBundleIndex);
                }
            }
            var graphDataSize = writer.BaseStream.Position - graphDataOffset;
            
            foreach (var item in Exports)
            {
                item.SerialOffset = writer.BaseStream.Position;
                item.Write(writer);
            }

            var endOffset = writer.BaseStream.Position;

            // write export map
            writer.BaseStream.Position = exportMapOffset;
            foreach (var item in Exports)
                item.WriteExportMapEntry(writer);

            // write header
            writer.BaseStream.Position = headerPos;
            writer.Write(Name);
            writer.Write(SourceName);
            writer.Write((int)PackageFlags);
            writer.Write((int)0x5775); // cooked header size??
            writer.Write((int)nameMapNamesOffset);
            writer.Write((int)nameMapNamesSize);
            writer.Write((int)nameMapHashesOffset);
            writer.Write((int)nameMapHashesSize);
            writer.Write((int)importMapOffset);
            writer.Write((int)exportMapOffset);
            writer.Write((int)exportBundlesOffset);
            writer.Write((int)graphDataOffset);
            writer.Write((int)graphDataSize);
            writer.Write((int)0); // pad

            // it is done
            writer.BaseStream.Position = 0;

            return stream;
        }

        /// <summary>
        /// Serializes and writes an asset to disk from memory.
        /// </summary>
        /// <param name="outputPath">The path on disk to write the asset to.</param>
        /// <exception cref="UnknownEngineVersionException">Thrown when <see cref="ObjectVersion"/> is unspecified.</exception>
        public override void Write(string outputPath)
        {
            if (ObjectVersion == ObjectVersion.UNKNOWN) throw new UnknownEngineVersionException("Cannot begin serialization before an object version is specified");

            MemoryStream newData = WriteData();
            using (FileStream f = File.Open(outputPath, FileMode.Create, FileAccess.Write))
            {
                newData.CopyTo(f);
            }
        }

        /// <summary>
        /// Reads an asset from disk and initializes a new instance of the <see cref="UAsset"/> class to store its data in memory.
        /// </summary>
        /// <param name="path">The path of the asset file on disk that this instance will read from.</param>
        /// <param name="engineVersion">The version of the Unreal Engine that will be used to parse this asset. If the asset is versioned, this can be left unspecified.</param>
        /// <param name="mappings">A valid set of mappings for the game that this asset is from. Not required unless unversioned properties are used.</param>
        /// <exception cref="UnknownEngineVersionException">Thrown when this is an unversioned asset and <see cref="ObjectVersion"/> is unspecified.</exception>
        /// <exception cref="FormatException">Throw when the asset cannot be parsed correctly.</exception>
        public ZenAsset(string path, EngineVersion engineVersion = EngineVersion.UNKNOWN, Usmap mappings = null)
        {
            this.FilePath = path;
            this.Mappings = mappings;
            SetEngineVersion(engineVersion);

            Read(PathToReader(path));
        }


        /// <summary>
        /// Reads an asset from disk and initializes a new instance of the <see cref="UAsset"/> class to store its data in memory.
        /// </summary>
        /// <param name="path">The path of the asset file on disk that this instance will read from.</param>
        /// <param name="engineVersion">The version of the Unreal Engine that will be used to parse this asset. If the asset is versioned, this can be left unspecified.</param>
        /// <param name="mappings">A valid set of mappings for the game that this asset is from. Not required unless unversioned properties are used.</param>
        /// <exception cref="UnknownEngineVersionException">Thrown when this is an unversioned asset and <see cref="ObjectVersion"/> is unspecified.</exception>
        /// <exception cref="FormatException">Throw when the asset cannot be parsed correctly.</exception>
        public ZenAsset(string path, EngineVersion engineVersion = EngineVersion.UNKNOWN, Usmap mappings = null, IOGlobalData globalData = null)
        {
            this.FilePath = path;
            this.Mappings = mappings;
            this.GlobalData = globalData;
            SetEngineVersion(engineVersion);

            Read(PathToReader(path));
        }


        /// <summary>
        /// Reads an asset from a BinaryReader and initializes a new instance of the <see cref="ZenAsset"/> class to store its data in memory.
        /// </summary>
        /// <param name="reader">The asset's BinaryReader that this instance will read from.</param>
        /// <param name="engineVersion">The version of the Unreal Engine that will be used to parse this asset. If the asset is versioned, this can be left unspecified.</param>
        /// <param name="mappings">A valid set of mappings for the game that this asset is from. Not required unless unversioned properties are used.</param>
        /// <param name="useSeparateBulkDataFiles">Does this asset uses separate bulk data files (.uexp, .ubulk)?</param>
        /// <exception cref="UnknownEngineVersionException">Thrown when this is an unversioned asset and <see cref="ObjectVersion"/> is unspecified.</exception>
        /// <exception cref="FormatException">Throw when the asset cannot be parsed correctly.</exception>
        public ZenAsset(AssetBinaryReader reader, EngineVersion engineVersion = EngineVersion.UNKNOWN, Usmap mappings = null, bool useSeparateBulkDataFiles = false)
        {
            this.Mappings = mappings;
            UseSeparateBulkDataFiles = useSeparateBulkDataFiles;
            SetEngineVersion(engineVersion);
            Read(reader);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenAsset"/> class. This instance will store no asset data and does not represent any asset in particular until the <see cref="Read"/> method is manually called.
        /// </summary>
        /// <param name="engineVersion">The version of the Unreal Engine that will be used to parse this asset. If the asset is versioned, this can be left unspecified.</param>
        /// <param name="mappings">A valid set of mappings for the game that this asset is from. Not required unless unversioned properties are used.</param>
        public ZenAsset(EngineVersion engineVersion = EngineVersion.UNKNOWN, Usmap mappings = null)
        {
            this.Mappings = mappings;
            SetEngineVersion(engineVersion);
        }

        /// <summary>
        /// Reads an asset from disk and initializes a new instance of the <see cref="ZenAsset"/> class to store its data in memory.
        /// </summary>
        /// <param name="path">The path of the asset file on disk that this instance will read from.</param>
        /// <param name="objectVersion">The object version of the Unreal Engine that will be used to parse this asset</param>
        /// <param name="customVersionContainer">A list of custom versions to parse this asset with.</param>
        /// <param name="mappings">A valid set of mappings for the game that this asset is from. Not required unless unversioned properties are used.</param>
        /// <exception cref="UnknownEngineVersionException">Thrown when this is an unversioned asset and <see cref="ObjectVersion"/> is unspecified.</exception>
        /// <exception cref="FormatException">Throw when the asset cannot be parsed correctly.</exception>
        public ZenAsset(string path, ObjectVersion objectVersion, List<CustomVersion> customVersionContainer, Usmap mappings = null)
        {
            this.FilePath = path;
            this.Mappings = mappings;
            ObjectVersion = objectVersion;
            CustomVersionContainer = customVersionContainer;

            Read(PathToReader(path));
        }

        /// <summary>
        /// Reads an asset from a BinaryReader and initializes a new instance of the <see cref="ZenAsset"/> class to store its data in memory.
        /// </summary>
        /// <param name="reader">The asset's BinaryReader that this instance will read from.</param>
        /// <param name="objectVersion">The object version of the Unreal Engine that will be used to parse this asset</param>
        /// <param name="customVersionContainer">A list of custom versions to parse this asset with.</param>
        /// <param name="mappings">A valid set of mappings for the game that this asset is from. Not required unless unversioned properties are used.</param>
        /// <param name="useSeparateBulkDataFiles">Does this asset uses separate bulk data files (.uexp, .ubulk)?</param>
        /// <exception cref="UnknownEngineVersionException">Thrown when this is an unversioned asset and <see cref="ObjectVersion"/> is unspecified.</exception>
        /// <exception cref="FormatException">Throw when the asset cannot be parsed correctly.</exception>
        public ZenAsset(AssetBinaryReader reader, ObjectVersion objectVersion, List<CustomVersion> customVersionContainer, Usmap mappings = null, bool useSeparateBulkDataFiles = false)
        {
            this.Mappings = mappings;
            UseSeparateBulkDataFiles = useSeparateBulkDataFiles;
            ObjectVersion = objectVersion;
            CustomVersionContainer = customVersionContainer;
            Read(reader);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenAsset"/> class. This instance will store no asset data and does not represent any asset in particular until the <see cref="Read"/> method is manually called.
        /// </summary>
        /// <param name="objectVersion">The object version of the Unreal Engine that will be used to parse this asset</param>
        /// <param name="customVersionContainer">A list of custom versions to parse this asset with.</param>
        /// <param name="mappings">A valid set of mappings for the game that this asset is from. Not required unless unversioned properties are used.</param>
        public ZenAsset(ObjectVersion objectVersion, List<CustomVersion> customVersionContainer, Usmap mappings = null)
        {
            this.Mappings = mappings;
            ObjectVersion = objectVersion;
            CustomVersionContainer = customVersionContainer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenAsset"/> class. This instance will store no asset data and does not represent any asset in particular until the <see cref="Read"/> method is manually called.
        /// </summary>
        public ZenAsset()
        {

        }
    }
}
