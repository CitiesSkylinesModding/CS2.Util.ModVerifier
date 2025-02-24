using CS2.Util.ModVerifier.Models;
using Spectre.Console;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using nietras.SeparatedValues;

namespace CS2.Util.ModVerifier
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            AnsiConsole.Write(new FigletText("Mod Verifier").Color(Color.LightCyan1));
            const string modsPath =
                @"%AppData%\..\LocalLow\Colossal Order\Cities Skylines II\.cache\Mods\mods_subscribed";
            var actualPath = Path.GetFullPath(Environment.ExpandEnvironmentVariables(modsPath));
            var displayPath = new TextPath(actualPath)
                .RootColor(Color.Red)
                .SeparatorColor(Color.Green)
                .StemColor(Color.Blue)
                .LeafColor(Color.Yellow);
            AnsiConsole.MarkupLine("[green]Current detected mods path[/]:");
            AnsiConsole.Write(displayPath);
            AnsiConsole.WriteLine();
            var scanMods = AnsiConsole.Prompt(
                new TextPrompt<bool>("Scan mods?")
                    .AddChoice(true)
                    .AddChoice(false)
                    .DefaultValue(true)
                    .WithConverter(choice => choice ? "y" : "n"));
            if (!scanMods)
            {
                return;
            }

            var table = new Table().Centered();
            var modInfos = new List<ModInfo>();
            AnsiConsole.Live(table)
                .AutoClear(false)
                .Start(ctx =>
                {
                    table.Border = TableBorder.MinimalHeavyHead;
                    table.AddColumn("[green]Name[/]");
                    table.AddColumn("[yellow]Id[/]");
                    table.AddColumn("[cyan]Author[/]");
                    table.Expand();
                    ctx.Refresh();
                    var mods = Directory.GetDirectories(actualPath)
                        .Where(x => File.Exists(Path.Combine(x, ".metadata", "metadata.json")));
                    foreach (var mod in mods)
                    {
                        var metadata = JsonSerializer.Deserialize(
                            File.ReadAllText(Path.Combine(mod, ".metadata", "metadata.json")),
                            ModInfoContext.Default.ModInfo);
                        if (metadata is null)
                        {
                            ctx.Refresh();
                            Thread.Sleep(70);
                            continue;
                        }

                        modInfos.Add(metadata);
                        table.AddRow([
                            $"[green]{metadata.DisplayName.EscapeMarkup()}[/]", $"[yellow]{metadata.Id.ToString()}[/]",
                            $"[cyan]{metadata.Author.EscapeMarkup()}[/]"
                        ]);
                        ctx.Refresh();
                        Thread.Sleep(20);
                    }
                });
            var verifyMods = AnsiConsole.Prompt(
                new TextPrompt<bool>("Verify mods?")
                    .AddChoice(true)
                    .AddChoice(false)
                    .DefaultValue(true)
                    .WithConverter(choice => choice ? "y" : "n"));
            if (!verifyMods)
            {
                return;
            }

            var o = new object();
            var verified = new List<(ModInfo, bool)>();
            await AnsiConsole.Status()
                .StartAsync("Verifying...", async ctx =>
                {
                    await Parallel.ForEachAsync(modInfos, async (mod, token) =>
                        {
                            var path = mod.LocalData.FolderAbsolutePath;
                            var name = mod.Name;
                            var manifestVersion =
                                await File.ReadAllTextAsync(Path.Combine(path, ".cpatch", name, "version"), token);
                            var manifests = new List<string>();
                            await using var fs = File.OpenRead(Path.Combine(Path.Combine(path, ".cpatch", name,
                                manifestVersion,
                                "complete", "manifest")));
                            using var sr = new StreamReader(fs);
                            // Name
                            _ = await sr.ReadLineAsync(token);
                            // Hash method, SHA256
                            _ = await sr.ReadLineAsync(token);
                            // Empty
                            _ = await sr.ReadLineAsync(token);
                            var csvReader = await Sep.New(',').Reader(opt => opt with
                            {
                                HasHeader = false,
                                DisableColCountCheck = true,
                                Unescape = true
                            }).FromAsync(sr, cancellationToken: token);
                            await foreach (var results in csvReader)
                            {
                                var fullPath = Path.Combine(path, results[0].ToString());
                                var file = new FileInfo(fullPath);
                                if (!file.Exists)
                                {
                                    lock (o)
                                    {
                                        AnsiConsole.MarkupLine(
                                            $"[red][[ERR]][/] {mod.DisplayName.EscapeMarkup()} is broken!");
                                        verified.Add((mod, false));
                                    }

                                    return;
                                }

                                if (file.Length.ToString() != results[1].ToString())
                                {
                                    lock (o)
                                    {
                                        AnsiConsole.MarkupLine(
                                            $"[red][[ERR]][/] {mod.DisplayName.EscapeMarkup()} is broken!");
                                        verified.Add((mod, false));
                                    }

                                    return;
                                }

                                var hash = SHA256.HashData(file.OpenRead());
                                if (Convert.ToBase64String(hash).Replace("/", "_").Replace("+", "-") ==
                                    results[2].ToString())
                                    continue;
                                lock (o)
                                {
                                    AnsiConsole.MarkupLine(
                                        $"[red][[ERR]][/] {mod.DisplayName.EscapeMarkup()} is broken!");
                                    verified.Add((mod, false));
                                }

                                return;
                            }

                            lock (o)
                            {
                                verified.Add((mod, true));
                            }
                        }
                    );
                });

            var verifiedTable = new Table().Centered();
            AnsiConsole.Live(verifiedTable)
                .AutoClear(false)
                .Start(ctx =>
                {
                    verifiedTable.Border = TableBorder.MinimalHeavyHead;
                    verifiedTable.AddColumn("Name");
                    verifiedTable.AddColumn("Id");
                    verifiedTable.AddColumn("Author");
                    verifiedTable.AddColumn("Verified");
                    verifiedTable.Expand();
                    ctx.Refresh();

                    foreach (var (mod, status) in verified)
                    {
                        var color = status ? "green" : "red";
                        verifiedTable.AddRow([
                            $"[{color}]{mod.DisplayName.EscapeMarkup()}[/]",
                            $"[{color}]{mod.Id.ToString().EscapeMarkup()}[/]",
                            $"[{color}]{mod.Author.EscapeMarkup()}[/]",
                            $"[{color}]{status.ToString().EscapeMarkup()}[/]"
                        ]);
                        ctx.Refresh();
                        Thread.Sleep(20);
                    }
                });

            AnsiConsole.Write(new BreakdownChart()
                .FullSize()
                .AddItem("Correct", verified.Count(x => x.Item2), Color.Green)
                .AddItem("Broken", verified.Count(x => !x.Item2), Color.Red));
            AnsiConsole.MarkupLine("[cyan]press any key to exit...[/]");
            Console.ReadKey();
        }
    }
}