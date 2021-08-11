using System.Threading.Tasks;

namespace FFXI_Navmesh_Builder_Forms.Generators {
  public interface INavMeshGenerator {
    Task GenerateNavMeshes();
  }
}