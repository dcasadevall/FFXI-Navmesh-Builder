
namespace FFXI_Navmesh_Builder_Forms {
  partial class MainForm {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
      if (disposing && (components != null)) {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
      this.BuildAllButton = new System.Windows.Forms.Button();
      this.CurrentFileLabel = new System.Windows.Forms.Label();
      this.LogLabel = new System.Windows.Forms.Label();
      this.DumpObjButton = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // BuildAllButton
      // 
      this.BuildAllButton.Location = new System.Drawing.Point(12, 132);
      this.BuildAllButton.Name = "BuildAllButton";
      this.BuildAllButton.Size = new System.Drawing.Size(75, 23);
      this.BuildAllButton.TabIndex = 0;
      this.BuildAllButton.Text = "Build All";
      this.BuildAllButton.UseVisualStyleBackColor = true;
      this.BuildAllButton.Click += new System.EventHandler(this.BuildAllButton_Click);
      // 
      // CurrentFileLabel
      // 
      this.CurrentFileLabel.AutoSize = true;
      this.CurrentFileLabel.Location = new System.Drawing.Point(94, 138);
      this.CurrentFileLabel.Name = "CurrentFileLabel";
      this.CurrentFileLabel.Size = new System.Drawing.Size(0, 13);
      this.CurrentFileLabel.TabIndex = 1;
      // 
      // LogLabel
      // 
      this.LogLabel.AutoSize = true;
      this.LogLabel.Location = new System.Drawing.Point(12, 9);
      this.LogLabel.Name = "LogLabel";
      this.LogLabel.Size = new System.Drawing.Size(0, 13);
      this.LogLabel.TabIndex = 2;
      // 
      // DumpObjButton
      // 
      this.DumpObjButton.Location = new System.Drawing.Point(12, 9);
      this.DumpObjButton.Name = "DumpObjButton";
      this.DumpObjButton.Size = new System.Drawing.Size(75, 23);
      this.DumpObjButton.TabIndex = 3;
      this.DumpObjButton.Text = "Create OBJs";
      this.DumpObjButton.UseVisualStyleBackColor = true;
      this.DumpObjButton.Click += new System.EventHandler(this.DumpObjButton_Click);
      // 
      // MainForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(800, 159);
      this.Controls.Add(this.DumpObjButton);
      this.Controls.Add(this.LogLabel);
      this.Controls.Add(this.CurrentFileLabel);
      this.Controls.Add(this.BuildAllButton);
      this.Name = "MainForm";
      this.Text = "Navmesh Builder";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button BuildAllButton;
    private System.Windows.Forms.Label CurrentFileLabel;
    private System.Windows.Forms.Label LogLabel;
    private System.Windows.Forms.Button DumpObjButton;
  }
}

