using Ffxi_Navmesh_Builder.Common;
using Ffxi_Navmesh_Builder.Common.dat;
using Ffxi_Navmesh_Builder.Common.dat.Types;
using FFXI_Navmesh_Builder_Forms.Generators;
using FFXI_Navmesh_Builder_Forms.lib.dat;
using FFXI_Navmesh_Builder_Forms.Logging;
using FFXI_Navmesh_Builder_Forms.Settings;
using System;

namespace FFXI_NavMesh_Builder {
  class AppBoot {
    [STAThread]
    public static void Main(string[] args) {
      // This is essentially our root of composition
      // Path should be customizable, along with Settings
      var ffxiPath = "C:/Program Files (x86)/PlayOnline/SquareEnix/FINAL FANTASY XI/";
      var ffxiNav = new Ffxinav();
      var logger = new SystemConsoleLogger();

      // LandSandBoat code's dependencies
      var romPath = new RomPath(ffxiPath, logger);
      var dms = new d_ms(romPath, logger);
      var dat = new dat(ffxiPath, dms, romPath, logger);
      var zoneDatFactory =
        new ParseZoneModelDatFactory(logger,
                                     ffxiPath,
                                     romPath,
                                     new Mmb(logger),
                                     dumpSubregionInfo: true);

      // Generators
      var navMeshSettings = new NavMeshGenerationSettings();
      var navMeshGenerator = new XenonsmurfNavMeshGenerator(logger, ffxiNav, navMeshSettings);
      var objFileGenerator = new XenonsmurfObjFileGenerator(dat, ffxiPath, zoneDatFactory, logger);
      MainWindow mainWindow = new MainWindow(navMeshGenerator, objFileGenerator, logger);
      mainWindow.ShowDialog();
    }
  }
}
