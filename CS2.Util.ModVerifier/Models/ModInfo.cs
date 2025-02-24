using System.Text.Json.Serialization;

namespace CS2.Util.ModVerifier.Models;

public partial class ModInfo
{
    [JsonPropertyName("Id")] public long Id { get; set; }

    [JsonPropertyName("Name")] public string Name { get; set; }

    [JsonPropertyName("DisplayName")] public string DisplayName { get; set; }

    [JsonPropertyName("Author")] public string Author { get; set; }

    [JsonPropertyName("ShortDescription")] public string ShortDescription { get; set; }

    [JsonPropertyName("LongDescription")] public string LongDescription { get; set; }

    [JsonPropertyName("RequiredGameVersion")]
    public string RequiredGameVersion { get; set; }

    [JsonPropertyName("LatestVersion")] public string LatestVersion { get; set; }

    [JsonPropertyName("Version")] public string Version { get; set; }

    [JsonPropertyName("ThumbnailPath")] public Uri ThumbnailPath { get; set; }

    [JsonPropertyName("Size")] public long Size { get; set; }

    [JsonPropertyName("Tags")] public Tag[] Tags { get; set; }

    [JsonPropertyName("Rating")] public long Rating { get; set; }

    [JsonPropertyName("RatingsTotal")] public long RatingsTotal { get; set; }

    [JsonPropertyName("State")] public string State { get; set; }

    [JsonPropertyName("LocalData")] public LocalData LocalData { get; set; }

    [JsonPropertyName("LatestUpdate")] public string LatestUpdate { get; set; }

    [JsonPropertyName("InstalledDate")] public string InstalledDate { get; set; }

    [JsonPropertyName("Playsets")] public Playset[] Playsets { get; set; }

    [JsonPropertyName("HasLiked")] public bool HasLiked { get; set; }
}

public partial class LocalData
{
    [JsonPropertyName("LocalType")] public string LocalType { get; set; }

    [JsonPropertyName("FolderAbsolutePath")]
    public string FolderAbsolutePath { get; set; }

    [JsonPropertyName("ContentFileOrFolder")]
    public string ContentFileOrFolder { get; set; }

    [JsonPropertyName("ThumbnailFilename")]
    public string ThumbnailFilename { get; set; }

    [JsonPropertyName("ScreenshotsFilenames")]
    public string[] ScreenshotsFilenames { get; set; }
}

public partial class Playset
{
    [JsonPropertyName("PlaysetId")] public long PlaysetId { get; set; }

    [JsonPropertyName("SubscribedDate")] public string SubscribedDate { get; set; }

    [JsonPropertyName("ModIsEnabled")] public bool ModIsEnabled { get; set; }

    [JsonPropertyName("Version")] public string Version { get; set; }

    [JsonPropertyName("LoadOrder")] public long LoadOrder { get; set; }
}

public partial class Tag
{
    [JsonPropertyName("Id")] public string Id { get; set; }

    [JsonPropertyName("DisplayName")] public string DisplayName { get; set; }
}

[JsonSerializable(typeof(ModInfo))]
public partial class ModInfoContext : JsonSerializerContext;