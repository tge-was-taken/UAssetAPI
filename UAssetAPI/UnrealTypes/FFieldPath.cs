using Newtonsoft.Json;
using UAssetAPI.UnrealTypes;
using UAssetAPI.ExportTypes;
using System;

namespace UAssetAPI.UnrealTypes
{
    [JsonObject(MemberSerialization.OptIn)]
    public class FFieldPath
    {
        public static readonly FFieldPath Null = new FFieldPath()
        {
            Path = Array.Empty<FName>(),
            ResolvedOwner = FPackageIndex.Null,
        };

        /// <summary>
        /// Path to the FField object from the innermost FField to the outermost UObject (UPackage)
        /// </summary>
        [JsonProperty]
        public FName[] Path;

        /// <summary>
        /// The cached owner of this field.
        /// </summary>
        [JsonProperty]
        public FPackageIndex ResolvedOwner;

        public FFieldPath(FName[] path, FPackageIndex resolvedOwner)
        {
            Path = path;
            ResolvedOwner = resolvedOwner;
        }

        public FFieldPath()
        {

        }
    }
}
