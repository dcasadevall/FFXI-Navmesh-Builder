using FFXI_Navmesh_Builder_Forms.Logging;
using Ffxi_Navmesh_Builder.Common.dat;
using Ffxi_Navmesh_Builder.Common.dat.Types;

namespace FFXI_Navmesh_Builder_Forms.lib.dat {
  public class ParseZoneModelDatFactory {
    private readonly ILogger logger;
    private readonly string ffxiPath;
    private readonly RomPath romPath;
    private readonly Mmb mmb;
    private readonly bool dumpSubregionInfo;

    public ParseZoneModelDatFactory(ILogger logger,
                                    string ffxiPath,
                                    RomPath romPath,
                                    Mmb mmb,
                                    bool dumpSubregionInfo) {
      this.logger = logger;
      this.ffxiPath = ffxiPath;
      this.romPath = romPath;
      this.mmb = mmb;
      this.dumpSubregionInfo = dumpSubregionInfo;
    }

    public ParseZoneModelDat Create(int zoneId, string zoneName) {
      return new ParseZoneModelDat(logger,
                                   zoneId,
                                   zoneName,
                                   ffxiPath,
                                   romPath,
                                   new Mzb(logger),
                                   mmb,
                                   new Rid(ffxiPath, romPath, logger),
                                   dumpSubregionInfo);
    }
  }
}