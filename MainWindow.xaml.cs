using Ffxi_Navmesh_Builder.Common;
using Ffxi_Navmesh_Builder.Common.dat;
using Ffxi_Navmesh_Builder.Common.dat.Types;
using FFXI_Navmesh_Builder_Forms.Generators;
using FFXI_Navmesh_Builder_Forms.lib.dat;
using FFXI_Navmesh_Builder_Forms.Logging;
using FFXI_Navmesh_Builder_Forms.Settings;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace FFXI_NavMesh_Builder {
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window {
    private XenonsmurfNavMeshGenerator navMeshGenerator;
    private XenonsmurfObjFileGenerator objFileGenerator;
    private readonly ILogger logger;

    public MainWindow(XenonsmurfNavMeshGenerator navMeshGenerator, XenonsmurfObjFileGenerator objFileGenerator, ILogger logger) {
      this.navMeshGenerator = navMeshGenerator;
      this.objFileGenerator = objFileGenerator;
      this.logger = logger;
      logger.LineLogged += (line) => {
        Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate {
          LogLabel.Content = line;
        }));
      };

      InitializeComponent();
    }

    private async void CreateObjsButton_Click(object sender, RoutedEventArgs e) {
      CreateObjsButton.IsEnabled = false;
      CreateMeshesButton.IsEnabled = false;
      await objFileGenerator.GenerateObjFiles(CancellationToken.None);
      CreateObjsButton.IsEnabled = true;
      CreateObjsButton.IsEnabled = true;
    }

    private async void CreateMeshesButton_Click(object sender, RoutedEventArgs e) {
      CreateObjsButton.IsEnabled = false;
      CreateMeshesButton.IsEnabled = false;
      await navMeshGenerator.GenerateNavMeshes();
      CreateMeshesButton.IsEnabled = true;
      CreateObjsButton.IsEnabled = true;
    }
  }
}
