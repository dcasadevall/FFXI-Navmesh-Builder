using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FFXI_Navmesh_Builder_Forms.lib.dat;
using FFXI_Navmesh_Builder_Forms.Logging;
using FFXI_Navmesh_Builder.Common;
using Ffxi_Navmesh_Builder.Common.dat;

namespace FFXI_Navmesh_Builder_Forms.Generators {
  public class LandSandBoatObjFileGenerator : IObjFileGenerator {
    private readonly dat dat;
    private readonly string ffxiPath;
    private readonly ParseZoneModelDatFactory zoneDatFactory;
    private readonly ILogger logger;
    private readonly bool saveEntityInfo;
    private readonly bool saveSubRegionInfo;
    private readonly bool useTopazZoneNames;
    private readonly bool useIdNames;

    public LandSandBoatObjFileGenerator(dat dat,
                                        string ffxiPath,
                                        ParseZoneModelDatFactory zoneDatFactory,
                                        ILogger logger,
                                        bool saveEntityInfo = true,
                                        bool saveSubRegionInfo = true,
                                        bool useTopazZoneNames = true,
                                        bool useIdNames = false) {
      this.dat = dat;
      this.ffxiPath = ffxiPath;
      this.zoneDatFactory = zoneDatFactory;
      this.logger = logger;
      this.saveEntityInfo = saveEntityInfo;
      this.saveSubRegionInfo = saveSubRegionInfo;
      this.useTopazZoneNames = useTopazZoneNames;
      this.useIdNames = useIdNames;
    }

    public async Task GenerateObjFiles(CancellationToken cancellationToken) {
      var cancellationTokenSource = new CancellationTokenSource();
      if (dat.Dms._zones.Count == 0) {
        dat.ParseDat(55465);
      }

      if (dat.Dms._zones.Count == 0) {
        logger.Log("Failed to load zones from dat. Is FFXI Path correct?");
        return;
      }

      var stopWatch = new Stopwatch();
      stopWatch.Start();
      foreach (var zone in dat.Dms._zones) {
        var zoneDat =
          await DumpZoneDat(zone.Id, zone.Name, zone.Path, cancellationTokenSource.Token);
        if (zoneDat == null) {
          logger.Log($"Error parsing: {zone.Name}");
          continue;
        }

        if (useTopazZoneNames) {
          var zoneName = TopazNames.zoneNames.FirstOrDefault(x => x.Key == zone.Id).Value;
          if (zoneName != null) {
            zoneDat.Mzb.WriteObj(zoneName);
          } else {
            logger.Log($"Topaz Name not found for zone: {zone.Name}");
          }
        } else {
          switch (useIdNames) {
            case true when zoneDat.Mzb.WriteObj(zone.Id.ToString()):
            case false when zoneDat.Mzb.WriteObj(zone.Name):
              continue;
          }
        }
      }

      stopWatch.Stop();
      var ts = stopWatch.Elapsed;
      var elapsedTime =
        $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}";
      logger.Log($@"Time taken to dump all collision obj files {elapsedTime}");
    }
    
    /// <summary>
    /// Dumps the zone dat.
    /// </summary>
    /// <param name="zoneId">The zone identifier.</param>
    /// <param name="zoneName">Name of the zone.</param>
    /// <param name="datPath">The dat path.</param>
    private async Task<ParseZoneModelDat> DumpZoneDat(int zoneId,
                                                      string zoneName,
                                                      string datPath,
                                                      CancellationToken cancellationToken) {
      async Task<ParseZoneModelDat> Function() {
        try {
          if (saveEntityInfo) {
            if (dat != null) {
              if (zoneId < 1000 || zoneId > 1299) {
                var fileId = zoneId < 256 ? zoneId + 6720 : zoneId + 86235;
                dat.ParseDat(fileId);
              } else {
                var fileId = zoneId + 66911;
                dat.ParseDat(fileId);
              }
            }

            dat?.Entity.DumpToXml(zoneId);
          }

          var stopWatch = new Stopwatch();
          stopWatch.Start();
          var zoneDatPath = $@"{ffxiPath}{datPath}";

          logger.Log($@"Building an OBJ file using collision data for:  {zoneName} ID= {zoneId.ToString()}");
          var zoneDat = zoneDatFactory.Create(zoneId, zoneName);
          if (zoneDat.LoadDat(zoneDatPath)) {
            foreach (var subRegion in zoneDat.Rid.SubRegions.Where(sr => sr.RomPath != ffxiPath &&
              sr.FileId != 0 &&
              sr.RomPath != zoneDatPath &&
              sr.RomPath != string.Empty)) {
              zoneDat.LoadDat(subRegion.RomPath);
            }
          }

          await Task.Delay(100, cancellationToken);
          if (saveSubRegionInfo) {
            zoneDat.Rid.DumpToXml(zoneId);
          }

          stopWatch.Stop();
          var ts = stopWatch.Elapsed;
          var elapsedTime =
            $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}";
          logger.Log($@"Finished dumping {zoneName} collision data to {zoneName}.obj, Time taken {elapsedTime}");

          return zoneDat;
        } catch (Exception ex) {
          logger.Log($"{ex}");
          return null;
        }
      }

      return await Task.Run(Function, cancellationToken);
    }
  }
}