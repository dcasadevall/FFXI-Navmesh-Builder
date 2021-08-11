using System.Threading;
using System.Threading.Tasks;

namespace FFXI_Navmesh_Builder_Forms.Generators {
  public interface IObjFileGenerator {
    Task GenerateObjFiles(CancellationToken cancellationToken);
  }
}