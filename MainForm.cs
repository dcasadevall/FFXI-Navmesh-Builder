using Ffxi_Navmesh_Builder.Common;
using FFXI_Navmesh_Builder_Forms.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FFXI_Navmesh_Builder_Forms {
  public partial class MainForm : Form {
    private readonly Ffxinav ffxinav = new Ffxinav();

    public MainForm() {
      InitializeComponent();
      InitSettings();
    }

    private void InitSettings() {
      try {
        var settings = new NavMeshGenerationSettings();
        ffxinav.ChangeNavMeshSettings(Convert.ToDouble(settings.cellSize), Convert.ToDouble(settings.cellHeight)
            , Convert.ToDouble(settings.agentHeight), Convert.ToDouble(settings.agentRadius), Convert.ToDouble(settings.maxClimb)
            , Convert.ToDouble(settings.maxSlope), Convert.ToDouble(settings.tileSize), Convert.ToDouble(settings.regionMinSize),
            Convert.ToDouble(settings.regionMergeSize), Convert.ToDouble(settings.edgeMaxLen), Convert.ToDouble(settings.edgeMaxError), Convert.ToDouble(settings.vertsPerPoly)
            , Convert.ToDouble(settings.detailSampleDistance), Convert.ToDouble(settings.detailSampleMaxError),
            settings.dllDebugMode);
      } catch (Exception ex) {
        LogLabel.Text = $@"{ex}";
      }
    }

    private async void BuildAllButton_Click(object sender, EventArgs e) {
      BuildAllButton.Enabled = false;

      var path = $@"{ Directory.GetCurrentDirectory()}\Map Collision obj files";
      var dumpPath = $@"{ Directory.GetCurrentDirectory()}\Dumped NavMeshes";
      // var fileCount = Directory.GetFiles(path, "*.obj", SearchOption.AllDirectories).Length;

      /*
      foreach (var file in Directory.EnumerateFiles(string.Format(dumpPath, "*.nav"))) {
        var fullPath = Path.GetFileName(file);
        var name = fullPath.Substring(0, fullPath.LastIndexOf(".", StringComparison.Ordinal) + 1);

        File.Move(Path.GetFullPath(file), dumpPath + "\\" + name.Replace("_-", "-").Replace("-_", "-").Replace(" ", "_").Replace("'", "").Replace("#","") + "nav");
      }
      return;
      */

      foreach (var file in Directory.EnumerateFiles(string.Format(path, "*.obj"))) {
        var fullPath = Path.GetFileName(file);
        var name = fullPath.Substring(0, fullPath.LastIndexOf(".", StringComparison.Ordinal) + 1).Replace(" -", "-").Replace("- ", "-").Replace(" ", "_").Replace("'", "").Replace("#", "");
        var fullDumpFilePath = dumpPath + "\\" + name + "nav";
        if (File.Exists(fullDumpFilePath)) {
          continue;
        }

        CurrentFileLabel.Text = $@"Building NavMesh for {name} ...";
        var cancellationTokenSource = new CancellationTokenSource();
        await BuildNavMesh(fullDumpFilePath, cancellationTokenSource.Token);
      }

      CurrentFileLabel.Text = $@"Done";
      BuildAllButton.Enabled = true;
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

          ffxinav.Dump_NavMesh(file);
          stopWatch.Stop();
          var elapsed = stopWatch.Elapsed;
          var elapsedTime = $"{elapsed.Hours:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}.{elapsed.Milliseconds / 10:00}";
          Invoke((MethodInvoker)delegate {
            LogLabel.Text = $@"Time Taken to Build NavMesh = {elapsedTime}";
          });

        } catch (Exception ex) {
          Invoke((MethodInvoker)delegate {
            LogLabel.Text = $@"{ex}";
          });
        }
      }

      await Task.Run(Function, token);
    }

    private void DumpObjButton_Click(object sender, EventArgs e) {

    }


    /// <summary>
    /// Dumps the zone dat.
    /// </summary>
    /// <param name="zoneId">The zone identifier.</param>
    /// <param name="zoneName">Name of the zone.</param>
    /// <param name="datPath">The dat path.</param>
    private async Task DumpZoneDat(int zoneId, string zoneName, string datPath) {
      async Task Function() {
        try {
          if (_saveEntityinfo) {
            if (Dat != null) {
              if (zoneId < 1000 || zoneId > 1299) {
                var fileId = zoneId < 256 ? zoneId + 6720 : zoneId + 86235;
                Dat.ParseDat(fileId);
              } else {
                var fileId = zoneId + 66911;
                Dat.ParseDat(fileId);
              }
            }

            if (Dat != null)
              Dat.Entity.DumpToXml(zoneId);
          }
          var stopWatch = new Stopwatch();
          stopWatch.Start();
          ZoneDat = new ParseZoneModelDat(Log, this, zoneId, datPath, FFxiInstallPath, _dumpSubregionInfoToXml);
          var zoneDatPath = $@"{FFxiInstallPath}{datPath}";
          Log.AddDebugText(RtbDebug, $@"Building an OBJ file using collision data for:  {zoneName} ID= {zoneId.ToString()}");
          if (ZoneDat.LoadDat(zoneDatPath)) {
            foreach (var sr in ZoneDat.Rid.SubRegions
                .Where(sr =>
                    sr.RomPath != FFxiInstallPath && sr.FileId != 0 && sr.RomPath != zoneDatPath &&
                    sr.RomPath != string.Empty).Where(sr => ZoneDat.LoadDat(sr.RomPath)))
              ;
          }

          await Task.Delay(100);
          if (_saveSubRegioninfo) {
            ZoneDat.Rid.DumpToXml(zoneId);
          }
          stopWatch.Stop();
          var ts = stopWatch.Elapsed;
          var elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}";
          Log.AddDebugText(RtbDebug, $@"Finished dumping {zoneName} collision data to {zoneName}.obj, Time taken {elapsedTime}");
        } catch (Exception ex) {
          Log.LogFile(ex.ToString(), nameof(HomeView));
          Log.AddDebugText(RtbDebug, $@"{ex} > {nameof(HomeView)}");
        }
      }

      await Task.Run(Function, _cancellationToken.Token);
    }


    /// <summary>
    /// dump all object files as an asynchronous operation.
    /// </summary>
    /// <param name="run">if set to <c>true</c> [run].</param>
    private async Task DumpAllObjFilesAsync(bool run) {
      _cancellationToken = new CancellationTokenSource();
      switch (run) {
        case true: {
            if (Dat.Dms._zones.Count > 0) {
              var stopWatch = new Stopwatch();
              SubTp.IsEnabled = false;
              EntTp.IsEnabled = false;
              stopWatch.Start();
              foreach (var zone in Dat.Dms._zones) {
                await DumpZoneDat(zone.Id, zone.Name, zone.Path);

                if ((bool)TPNamesCB.IsChecked) {
                  foreach (KeyValuePair<int, string> tz in Tnames.zoneNames) {
                    if (tz.Key == zone.Id) {
                      ZoneDat.Mzb.WriteObj(tz.Value);
                    }
                  }
                } else
                  switch (IDonlyCb.IsChecked) {
                    case true when ZoneDat.Mzb.WriteObj(zone.Id.ToString()):
                    case false when ZoneDat.Mzb.WriteObj(zone.Name):
                      continue;
                  }
              }
              stopWatch.Stop();
              var ts = stopWatch.Elapsed;
              var elapsedTime = $"{ts.Hours.ToString("00")}:{ts.Minutes.ToString("00")}:{ts.Seconds.ToString("00")}.{ts.Milliseconds / 10:00}";
              Log.AddDebugText(RtbDebug, $@"Time taken to dump all collision obj files {elapsedTime}");
              BuildAllObJbtn.Content = @"Build obj files for all zones.";
              _dumpingMapDats = false;
              SubTp.IsEnabled = true;
              EntTp.IsEnabled = true;
              SubRegion.IsEnabled = true;
              Entity.IsEnabled = true;
            } else
              Log.AddDebugText(RtbDebug, "Please click Load Zones, before you try and build obj files!.");
            BuildAllObJbtn.Content = @"Build obj files for all zones.";
            _dumpingMapDats = false;
            SubTp.IsEnabled = true;
            EntTp.IsEnabled = true;
            SubRegion.IsEnabled = true;
            Entity.IsEnabled = true;
            break;
          }
        case false: {
            _cancellationToken?.Cancel();
            BuildAllObJbtn.Content = @"Build obj files for all zones.";
            _dumpingMapDats = false;
            SubTp.IsEnabled = true;
            EntTp.IsEnabled = true;
            SubRegion.IsEnabled = true;
            Entity.IsEnabled = true;
            break;
          }
      }
    }
  }
}
