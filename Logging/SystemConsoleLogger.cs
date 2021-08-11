using System;

namespace FFXI_Navmesh_Builder_Forms.Logging {
  /// <summary>
  /// Implementation of <see cref="ILogger"/> that forwards logged lines to the System <see cref="Console"/>
  /// </summary>
  public class SystemConsoleLogger : ILogger {
    public event Action<string> LineLogged;

    public void Log(string message) {
      Console.WriteLine(message);
      
      LineLogged?.Invoke(message);
    }
  }
}