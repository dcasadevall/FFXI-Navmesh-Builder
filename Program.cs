using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using FFXI_Navmesh_Builder_Forms.Generators;
using FFXI_Navmesh_Builder_Forms.Logging;
using FFXI_Navmesh_Builder_Forms.Settings;
using Ffxi_Navmesh_Builder.Common;

namespace FFXI_Navmesh_Builder_Forms {
  static class Program {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main() {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      
      // This is essentially our root of composition
      var ffxiNav = new Ffxinav();
      var logger = new SystemConsoleLogger();
      var navMeshSettings = new NavMeshGenerationSettings();
      var navMeshGenerator = new LandSandBoatNavMeshGenerator(logger, ffxiNav, navMeshSettings);
      var objFileGenerator = new LandSandBoatObjFileGenerator();
      Application.Run(new MainForm(navMeshGenerator, objFileGenerator, logger));
    }
  }
}
