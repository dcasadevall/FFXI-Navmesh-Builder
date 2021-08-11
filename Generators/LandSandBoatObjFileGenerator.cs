using System.Threading.Tasks;

namespace FFXI_Navmesh_Builder_Forms.Generators {
  public class LandSandBoatObjFileGenerator : IObjFileGenerator {
    public Task GenerateObjFiles() {
      throw new System.NotImplementedException();
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