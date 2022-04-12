using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Xml;
using System.Xml.Serialization;
using System.Text.RegularExpressions;

using Spectre.Console;
using Spectre.Console.Cli;

var app = new CommandApp<QuickScope>();
return app.Run(args);

internal sealed class QuickScope : AsyncCommand<QuickScope.Settings>
{
    const int PollSpeed = 750;
    const string VersionNumber = "1.0";

    public enum Update {
        Minor = 0,
        Major = 1
    };

    int[] Default_Update_Lengths = new int[] {
        3550,
        5350
    };

    HttpClient client = new();

    public async override Task<int> ExecuteAsync([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        client.DefaultRequestHeaders.Add("User-Agent", $"QuickScope/{VersionNumber} User/{settings.MainNation} (By 20XX, Atagait@hotmail.com)");

        string target = CleanName(settings.Region!);

        

        return 0;
    }

    public static string CleanName(string name) => name.ToLower().Replace(' ', '_');

    /// <summary>
    /// Unzips nation.xml.gz and region.xml.gz files
    /// </summary>
    /// <param name="Filename">the .xml.gz file to unzip</param>
    /// <returns></returns>
    private static string UnzipDump(string Filename)
    {
        using (var fileStream = new FileStream(Filename, FileMode.Open))
        {
            using (var gzStream = new GZipStream(fileStream, CompressionMode.Decompress))
            {
                using (var outputStream = new MemoryStream())
                {
                    gzStream.CopyTo(outputStream);
                    byte[] outputBytes = outputStream.ToArray(); // No data. Sad panda. :'(
                    return Encoding.Default.GetString(outputBytes);
                }
            }
        }
    }

    /// <summary>
    /// This method makes deserializing XML less painful
    /// <param name="url">The URL to request from</param>
    /// <returns>The parsed return from the request.</returns>
    /// </summary>
    T BetterDeserialize<T>(string XML) =>
        (T)new XmlSerializer(typeof(T))!.Deserialize(new StringReader(XML))!;

    /// <summary>
    /// This method waits the delay set by the program, then makes a request
    /// <param name="url">The URL to request from</param>
    /// <returns>The return from the request.</returns>
    /// </summary>
    HttpResponseMessage MakeReq(string url) {
        System.Threading.Thread.Sleep(PollSpeed);
        return client.GetAsync(url).GetAwaiter().GetResult();
    }

    public sealed class Settings : CommandSettings
    {
        [Description("Your nation name for identifying the user to NSAdmin")]
        [CommandArgument(0, "[MainNation]")]
        public string? MainNation { get; init; }

        [Description("Region to Scan")]
        [CommandArgument(1, "[region]")]
        public string? Region { get; init; }

        [Description("Region Data Dump XML file to use")]
        [CommandArgument(2, "[dataDump]")]
        public string? DataDump { get; init; }
    }
}

[XmlRoot("REGIONS")]
public class RegionDataDump
{
    [XmlElement("REGION", typeof(RegionAPI))]
    public RegionAPI[] Regions { get; init; }
}

[Serializable(), XmlRoot("REGION")]
public class Officer
{
    [XmlElement("NATION")]
    public string Nation { get; init; }
    [XmlElement("OFFICE")]
    public string Office { get; init; }
    [XmlElement("AUTHORITY")]
    public string OfficerAuth { get; init; }
    [XmlElement("TIME")]
    public int AssingedTimestamp { get; init; }
    [XmlElement("BY")]
    public string AssignedBy { get; init; }
    [XmlElement("ORDER")]
    public int Order { get; init; }
}

[XmlRoot("WORLD")]
public class WorldAPI
{
    [XmlArray("HAPPENINGS")]
    [XmlArrayItem("EVENT", typeof(WorldEvent))]
    public WorldEvent[] Happenings { get; init; }

    [XmlElement("REGIONS")]
    public string Regions { get; init; }

    [XmlElement("FEATUREDREGION")]
    public string Featured { get; init; }

    [XmlElement("NATIONS")]
    public string Nations { get; init; }

    [XmlElement("NEWNATIONS")]
    public string NewNations { get; init; }

    [XmlElement("NUMNATIONS")]
    public int NumNations { get; init; }

    [XmlElement("NUMREGIONS")]
    public int NumRegions { get; init; }

    [XmlElement("DISPATCH")]
    public DispatchAPI Dispatch { get; init; }
}

[Serializable()]
public class WorldEvent
{
    [XmlElement("TIMESTAMP")]
    public long Timestamp { get; init; }
    [XmlElement("TEXT")]
    public string Text { get; init; }
}

[Serializable()]
public class DispatchAPI
{
    [XmlAttribute("id")]
    public int DispatchID { get; init; }
    [XmlElement("TITLE")]
    public string Title { get; init; }
    [XmlElement("AUTHOR")]
    public string Author { get; init; }
    [XmlElement("CATEGORY")]
    public string Category { get; init; }
    [XmlElement("CREATED")]
    public long Created { get; init; }
    [XmlElement("EDITED")]
    public long Edited { get; init; }
    [XmlElement("VIEWS")]
    public int views { get; init; }
    [XmlElement("SCORE")]
    public int score { get; init; }
    [XmlElement("TEXT")]
    public string Text { get; init; }
}

[Serializable(), XmlRoot("REGION")]
public class RegionAPI
{
    //These are values parsed from the data dump
    [XmlElement("NAME")]
    public string name { get; init; }
    public string Name
    {
        get
        {
            return QuickScope.CleanName(name);
        }
    }

    [XmlElement("NUMNATIONS")]
    public int NumNations { get; init; }
    [XmlElement("NATIONS")]
    public string nations { get; init; }
    [XmlElement("DELEGATE")]
    public string Delegate { get; init; }
    [XmlElement("DELEGATEVOTES")]
    public int DelegateVotes { get; init; }
    [XmlElement("DELEGATEAUTH")]
    public string DelegateAuth { get; init; }
    [XmlElement("FOUNDER")]
    public string Founder { get; init; }
    [XmlElement("FOUNDERAUTH")]
    public string FounderAuth { get; init; }
    [XmlElement("FACTBOOK")]
    public string Factbook { get; init; }
    [XmlArray("OFFICERS"), XmlArrayItem("OFFICER", typeof(Officer))]
    public Officer[] Officers { get; init; }
    [XmlArray("EMBASSIES"), XmlArrayItem("EMBASSY", typeof(string))]
    public string[] Embassies { get; init; }
    [XmlElement("LASTUPDATE")]
    public double lastUpdate { get; init; }
    public double LastUpdate
    {
            get
            {
                //Subtract 4 hours from LastUpdate
                //Seconds into the update is more useful than the UTC update time
                return lastUpdate - (4 *3600);
            }
    }

    //These are values added after the fact by ARCore
    public string[] Nations
    {
        get {
            return nations
                .Replace(' ','_')
                .ToLower()
                .Split(":", StringSplitOptions.RemoveEmptyEntries);
        }
    }

    public long Index;
    public bool hasPassword;
    public bool hasFounder;
    public string FirstNation;
}
