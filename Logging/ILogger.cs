using System;

namespace FFXI_Navmesh_Builder_Forms.Logging {
  public interface ILogger {
    event Action<string> LineLogged; 
    
    void Log(string message);
  }
}