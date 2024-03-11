using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Xml.Linq;
using UAssetAPI.IO;
using UAssetAPI.Unversioned;

namespace UAssetAPI.UnrealTypes
{
    public enum EPackageObjectIndexType
    {
        Export,
        ScriptImport,
        PackageImport,
        Null,
        TypeCount = Null
    }

    public struct FPackageObjectIndex
    {
        internal const int IndexBits = 62;
        internal const ulong IndexMask = (1UL << IndexBits) - 1UL;
        internal const ulong ExportMask = (1UL << sizeof(uint)) - 1UL;
        internal const int TypeShift = IndexBits;
        public const ulong Invalid = ~0UL;

        public EPackageObjectIndexType Type;

        // Export
        public uint Export
        {
            get
            {
                return (uint)Hash;
            }
            set
            {
                Hash = (Hash & ~ExportMask) | value;
            }
        }

        // ScriptImport
        // This is usually the CityHash64 of the lowercase UTF-16 path
        // The first 2 bits will be discarded if set
        public ulong Hash;

        // PackageImport
        public uint ImportedPackageIndex
        {
            get
            {
                return (uint)((Hash & IndexMask) >> sizeof(uint));
            }
            set
            {
                Hash = ((ulong)value << sizeof(uint)) | (uint)Hash;
            }
        }

        public uint ImportedPublicExportHashIndex
        {
            get
            {
                return (uint)Hash;
            }
            set
            {
                Hash = (Hash & ~ExportMask) | value;
            }
        }

        public bool IsNull => Type == EPackageObjectIndexType.Null;
        public bool IsExport => Type == EPackageObjectIndexType.Export;
        public bool IsImport => IsScriptImport || IsPackageImport;
        public bool IsScriptImport => Type == EPackageObjectIndexType.ScriptImport;
        public bool IsPackageImport => Type == EPackageObjectIndexType.PackageImport;

        public FPackageIndex ToFPackageIndex(ZenAsset asset)
        {
            if (asset == null) return null;

            int idx = 0;
            switch (Type)
            {
                case EPackageObjectIndexType.Null:
                    idx = 0;
                    break;
                case EPackageObjectIndexType.Export:
                    idx = (int)Export;
                    break;
                case EPackageObjectIndexType.ScriptImport:
                case EPackageObjectIndexType.PackageImport:
                    for (int i = 0; i < asset.Imports.Count; i++)
                    {
                        if (asset.Imports[i].Equals(this))
                        {
                            idx = FPackageIndex.FromImport(i).Index;
                            break;
                        }
                    }
                    break;
            }
            return new FPackageIndex(idx);
        }

        private static Dictionary<FPackageObjectIndex, Import> _importCache = new Dictionary<FPackageObjectIndex, Import>();
        private static Dictionary<string, UsmapSchema> _schemaCache = new Dictionary<string, UsmapSchema>();

        private object GetImportOrExport(ZenAsset asset, FPackageObjectIndex index)
        {
            if (index.IsExport)
            {
                return asset.Exports[(int)index.Export];
            }
            else
            {
                return index.ToImport(asset);
            }
        }

        public Import ToImport(ZenAsset asset)
        {
            //throw new NotImplementedException("ZenAsset ToImport is currently unimplemented");

            var res = new Import();
            switch (Type)
            {
                case EPackageObjectIndexType.Null:
                    return null;
                case EPackageObjectIndexType.Export:
                    throw new InvalidOperationException("Attempt to call ToImport on an FPackageObjectIndex with type " + Type);
                case EPackageObjectIndexType.ScriptImport:
                    //var test = asset.GlobalData.ScriptObjectEntriesMap[this];

                    var scriptObject = asset.GlobalData.ScriptObjectEntriesMap[this];
                    var outer = GetImportOrExport(asset, scriptObject.OuterIndex);
                    var cdoClass = GetImportOrExport(asset, scriptObject.CDOClassIndex);

                    res.ObjectName = scriptObject.ObjectName;
                    res.ClassPackage = (outer as Import)?.ObjectName; // TODO
                    res.ClassName = (cdoClass as Import)?.ObjectName ??
                        (scriptObject.ObjectName.ToString().StartsWith("/") ?
                            FName.DefineDummy(asset, "Package") :
                            FName.DefineDummy(asset, "Object"));
                    res.OuterIndex = scriptObject.OuterIndex.ToFPackageIndex(asset);
                    break;
                case EPackageObjectIndexType.PackageImport:
                    // Import global object
                    // TODO record all exports (SerializeCommand) into a global list and reference that here 
                    //throw new NotImplementedException("ToImport on an FPackageObjectIndex with type " + Type + " is currently unimplemented");
                    return new Import()
                    {
                        bImportOptional = false,
                        ClassName = FName.DefineDummy(asset, "Package"),
                        ClassPackage = FName.DefineDummy(asset, ""),
                        ObjectName = FName.DefineDummy(asset, $"PackageImport_{Hash.ToString()}"),
                        OuterIndex = new FPackageIndex(0),
                    };
            }
            return res;
        }

        public override int GetHashCode()
        {
            int hashCode = -921245338;
            hashCode = hashCode * -1521134295 + Type.GetHashCode();
            hashCode = hashCode * -1521134295 + Hash.GetHashCode();
            return hashCode;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is FPackageObjectIndex poi2)) return false;
            return this.GetHashCode() == poi2.GetHashCode();
        }

        public static FPackageObjectIndex Unpack(ulong packed)
        {
            // TODO: is this interpretation actually right? results don't always make sense
            var res = new FPackageObjectIndex();
            res.Type = (EPackageObjectIndexType)(packed >> TypeShift);
            res.Hash = packed;
            return res;
        }

        public static ulong Pack(EPackageObjectIndexType typ, ulong hash)
        {
            return typ == EPackageObjectIndexType.Null ? ulong.MaxValue : (((ulong)typ << TypeShift) | hash);
        }

        public static ulong Pack(FPackageObjectIndex unpacked)
        {
            return FPackageObjectIndex.Pack(unpacked.Type, unpacked.Hash);
        }

        public static FPackageObjectIndex Read(UnrealBinaryReader reader)
        {
            return Unpack(reader.ReadUInt64());
        }

        public static int Write(UnrealBinaryWriter writer, EPackageObjectIndexType typ, ulong hash)
        {
            writer.Write(FPackageObjectIndex.Pack(typ, hash));
            return sizeof(ulong);
        }

        public int Write(UnrealBinaryWriter writer)
        {
            return FPackageObjectIndex.Write(writer, Type, Hash);
        }
    }
}
