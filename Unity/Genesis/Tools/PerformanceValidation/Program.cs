using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Genesis;

var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../"));
var game = new GenesisGame(GenesisCatalog.FromJson(File.ReadAllText(Path.Combine(root,
    "Assets/Genesis/Resources/Genesis/Data/catalog.json"))));
game.AddQuanta(100);
game.Buy(game.Catalog.generators[0].id);
var localization = new GenesisLocalization(new Dictionary<string, string[]> {
    ["Hope / {#}"] = new[] { "Esperanza / {#}", "Надежда / {#}", "希望 / {#}" }
});
localization.Select("es");
double result = 0;
Measure("Production queries", 20000, () => result = game.TotalQps);
Measure("Tick without production", 20000, () => game.Tick(0.016, false));
Measure("Repeated numeric localization", 20000, () => localization.Text("Hope / 123"));
Console.WriteLine("Production result: " + result);

static void Measure(string name, int iterations, Action action)
{
    for (int i = 0; i < 2000; i++) action();
    GC.Collect();
    long bytes = GC.GetAllocatedBytesForCurrentThread();
    var watch = Stopwatch.StartNew();
    for (int i = 0; i < iterations; i++) action();
    watch.Stop();
    Console.WriteLine($"{name}: {watch.Elapsed.TotalMilliseconds:F2} ms; {GC.GetAllocatedBytesForCurrentThread() - bytes} bytes / {iterations} calls");
}