using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FFXI_Navmesh_Builder_Forms.Logging;
using FFXI_Navmesh_Builder_Forms.Settings;
using Ffxi_Navmesh_Builder.Common;

namespace FFXI_Navmesh_Builder_Forms.Generators {
  /// <summary>
  /// Implementation of <see cref="INavMeshGenerator"/> that uses
  /// https://github.com/xenonsmurf/Ffxi_Navmesh_Builder's proxy classes to generate
  /// navmeshes
  /// </summary>
  public class XenonsmurfNavMeshGenerator : INavMeshGenerator {
    private readonly ILogger logger;
    private Ffxinav ffxinav;
    private readonly NavMeshGenerationSettings settings;

    public XenonsmurfNavMeshGenerator(ILogger logger,
                                        Ffxinav ffxinav,
                                        NavMeshGenerationSettings settings) {
      this.logger = logger;
      this.ffxinav = ffxinav;
      this.settings = settings;

      ApplySettings();
    }

    private void ApplySettings() {
      try {
        ffxinav.ChangeNavMeshSettings(Convert.ToDouble(settings.cellSize),
                                      Convert.ToDouble(settings.cellHeight),
                                      Convert.ToDouble(settings.agentHeight),
                                      Convert.ToDouble(settings.agentRadius),
                                      Convert.ToDouble(settings.maxClimb),
                                      Convert.ToDouble(settings.maxSlope),
                                      Convert.ToDouble(settings.tileSize),
                                      Convert.ToDouble(settings.regionMinSize),
                                      Convert.ToDouble(settings.regionMergeSize),
                                      Convert.ToDouble(settings.edgeMaxLen),
                                      Convert.ToDouble(settings.edgeMaxError),
                                      Convert.ToDouble(settings.vertsPerPoly),
                                      Convert.ToDouble(settings.detailSampleDistance),
                                      Convert.ToDouble(settings.detailSampleMaxError),
                                      settings.dllDebugMode);
      } catch (Exception e) {
        logger.Log($"{e}");
      }
    }

    public async Task GenerateNavMeshes() {
      var path = $@"{ Directory.GetCurrentDirectory()}\Map Collision obj files";
      var dumpPath = $@"{ Directory.GetCurrentDirectory()}\Dumped NavMeshes";
      if (!Directory.Exists(dumpPath)) {
        Directory.CreateDirectory(dumpPath);
      }

      foreach (var file in Directory.EnumerateFiles(string.Format(path, "*.obj"))) {
        var fullPath = Path.GetFileName(file);
        // var name = fullPath.Substring(0, fullPath.LastIndexOf(".", StringComparison.Ordinal) + 1).Replace(" -", "-").Replace("- ", "-").Replace(" ", "_").Replace("'", "").Replace("#", "");
        var name = fullPath.Substring(0, fullPath.LastIndexOf(".", StringComparison.Ordinal));
        var fullDumpFilePath = dumpPath + "\\" + name + ".nav";
        if (File.Exists(fullDumpFilePath)) {
          continue;
        }

        logger.Log($@"Building NavMesh for {name} ...");
        var cancellationTokenSource = new CancellationTokenSource();
        await BuildNavMesh(file, cancellationTokenSource.Token);
      }

      logger.Log($@"Done");
    }

    /// <summary>
    /// Builds the nav mesh.
    /// </summary>
    /// <param name="file">The file.</param>
    private async Task BuildNavMesh(string file, CancellationToken token) {
      void Function() {
        try {
          var stopWatch = new Stopwatch();
          stopWatch.Start();

          ffxinav = new Ffxinav();
          ApplySettings();
          if (!ffxinav.Dump_NavMesh(file)) {
            logger.Log($@"Failed to dump navmesh {file}"); //: " + ffxinav.GetErrorMessage());
            return;
          }

          stopWatch.Stop();
          var elapsed = stopWatch.Elapsed;
          var elapsedTime = $"{elapsed.Hours:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}.{elapsed.Milliseconds / 10:00}";
          logger.Log($@"Time Taken to Build NavMesh = {elapsedTime}");
        } catch (Exception ex) {
          logger.Log($@"{ex}");
        }
      }

      await Task.Run(Function, token);
    }
  }
}