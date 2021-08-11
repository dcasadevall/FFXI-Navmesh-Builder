// ***********************************************************************
// Assembly         : FFXI NAVMESH BUILDER
// Author           : Xenonsmurf
// Created          : 04-29-2021
//
// Last Modified By : dcasadevall
// Last Modified On : 07-10-2021
// Original: https://github.com/LandSandBoat/FFXI-NavMesh-Builder-/blob/master/src/FFXI%20Navmesh%20Builder/Common/dat/dat.cs
// ***********************************************************************
// <copyright file="dat.cs" company="Xenonsmurf">
//     Copyright © Xenonsmurf 2021
// </copyright>
// <summary></summary>
// ***********************************************************************

using Ffxi_Navmesh_Builder.Common.dat.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using FFXI_Navmesh_Builder_Forms.Logging;

namespace Ffxi_Navmesh_Builder.Common.dat {
  /// <summary>
  /// Class dat.
  /// </summary>
  public class dat {
    private readonly ILogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="dat"/> class.
    /// </summary>
    /// <param name="ffxIpath">The FFX ipath.</param>
    /// <param name="dms"></param>
    /// <param name="logger"></param>
    public dat(string ffxIpath, d_ms dms, RomPath romPath, ILogger logger) {
      this.logger = logger;
      try {
        InstallPath = ffxIpath;
        RomPath = romPath;
        Dms = dms;
        Entity = new Entity();
        Zones = new List<Zones>();
        _DatTypes = new List<Zones>();
      } catch (Exception ex) {
        logger.Log($"{ex}");
      }
    }

    /// <summary>
    /// Gets or sets the DMS.
    /// </summary>
    /// <value>The DMS.</value>
    public d_ms Dms { get; set; }

    /// <summary>
    /// Gets or sets the entity.
    /// </summary>
    /// <value>The entity.</value>
    public Entity Entity { get; set; }

    /// <summary>
    /// Gets or sets the zones.
    /// </summary>
    /// <value>The zones.</value>
    public List<Zones> Zones { get; set; }

    /// <summary>
    /// Gets or sets the dat types.
    /// </summary>
    /// <value>The dat types.</value>
    private List<Zones> _DatTypes { get; set; }

    /// <summary>
    /// Gets or sets the install path.
    /// </summary>
    /// <value>The install path.</value>
    private string InstallPath { get; set; }

    /// <summary>
    /// Gets or sets the rom path.
    /// </summary>
    /// <value>The rom path.</value>
    private RomPath RomPath { get; set; }

    /// <summary>
    /// Dumps to XML.
    /// </summary>
    public void DumpToXml() {
      try {
        var path = ($@"{AppDomain.CurrentDomain.BaseDirectory}");
        if (!Directory.Exists(path))
          Directory.CreateDirectory(path);
        if (!Directory.Exists(path))
          return;
        var outFile = File.Create($@"{path}\\datTypes.xml");
        var formatter = new XmlSerializer(_DatTypes.GetType());
        formatter.Serialize(outFile, _DatTypes);
        outFile.Close();
      } catch (Exception ex) {
        logger.Log($"{ex}");
      }
    }

    /// <summary>
    /// Parses the dat.
    /// </summary>
    /// <param name="fileId">The file identifier.</param>
    public void ParseDat(int fileId) {
      var datPath = RomPath.GetRomPath(fileId, RomPath.TableDirectory);
      var data = File.ReadAllBytes($@"{InstallPath}{datPath}");
      if (data.Length <= 0)
        return;
      var value = BitConverter.ToInt32(data, 4);
      var identifier = Encoding.ASCII.GetString(data, 0, 4).TrimStart('\0').TrimEnd('\0');
      switch (identifier) {
        case "d_ms":
          //I have only set this up for the English ZoneList.dat.
          Dms.ParseD_MSG(data);
          break;

        case "none":
          Entity.ParseNpcDat(data);
          break;

        default:
          break;
      }
    }
  }
}