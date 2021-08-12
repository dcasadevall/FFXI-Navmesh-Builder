using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXI_NavMesh_Builder.Settings {
  public class LandSandDefaultNavMeshGenerationSettings {
    // Cell and Tiles
    public int tileSize = 256;
    public float cellSize = 0.4f;
    public float cellHeight = 0.2f;
    // Agent
    public float agentHeight = 1.8f;
    public float agentRadius = 0.6f;
    public float maxClimb = 0.7f;
    public int maxSlope = 46;
    // Misc
    public int regionMinSize = 8;
    public int regionMergeSize = 20;
    public int edgeMaxLen = 12;
    public float edgeMaxError = 1.3f;
    public int vertsPerPoly = 6;
    public int detailSampleDistance = 6;
    public int detailSampleMaxError = 1;
    public bool dllDebugMode = true;
  }
}
