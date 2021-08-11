using System;
using System.Windows.Forms;
using FFXI_Navmesh_Builder_Forms.Generators;
using FFXI_Navmesh_Builder_Forms.Logging;

namespace FFXI_Navmesh_Builder_Forms {
  public partial class MainForm : Form {
    private readonly INavMeshGenerator navMeshGenerator;
    private readonly IObjFileGenerator objFileGenerator;

    public MainForm(INavMeshGenerator navMeshGenerator,
                    IObjFileGenerator objFileGenerator,
                    ILogger logger) {
      this.navMeshGenerator = navMeshGenerator;
      this.objFileGenerator = objFileGenerator;

      logger.LineLogged += (line) => {
        Invoke((MethodInvoker)delegate {
          LogLabel.Text = line;
        });
      };
      
      InitializeComponent();
    }

    private async void BuildAllButton_Click(object sender, EventArgs e) {
      BuildAllButton.Enabled = false;
      await navMeshGenerator.GenerateNavMeshes();
      BuildAllButton.Enabled = true;
    }

    private async void DumpObjButton_Click(object sender, EventArgs e) {
      BuildAllButton.Enabled = false;
      await objFileGenerator.GenerateObjFiles();
      BuildAllButton.Enabled = true;
    }
  }
}
