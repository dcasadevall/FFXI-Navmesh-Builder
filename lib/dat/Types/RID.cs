// ***********************************************************************
// Assembly         : FFXI NAVMESH BUILDER
// Author           : Xenonsmurf
// Created          : 04-29-2021
//
// Last Modified By : dcasadevall
// Last Modified On : 07-10-2021
// Original: https://github.com/LandSandBoat/FFXI-NavMesh-Builder-/blob/master/src/FFXI%20Navmesh%20Builder/Common/dat/Types/RID.cs
// ***********************************************************************
// <copyright file="RID.cs" company="Xenonsmurf">
//     Copyright © Xenonsmurf 2021
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using FFXI_Navmesh_Builder_Forms.Logging;

namespace Ffxi_Navmesh_Builder.Common.dat.Types {
  /// <summary>
  /// Class Rid.
  /// </summary>
  public class Rid {
    private readonly ILogger logger;

    /// <summary>
    /// Gets or sets the sub region model.
    /// </summary>
    /// <value>The sub region model.</value>
    public int SubRegionModel { get; set; }

    /// <summary>
    /// Gets or sets the sub regions.
    /// </summary>
    /// <value>The sub regions.</value>
    public BindingList<SubRegion> SubRegions { get; }

    /// <summary>
    /// Gets or sets the install path.
    /// </summary>
    /// <value>The install path.</value>
    private string InstallPath { get; }

    /// <summary>
    /// Gets or sets the rom path.
    /// </summary>
    /// <value>The rom path.</value>
    private RomPath RomPath { get; }

    /// <summary>
    /// Gets or sets the sr sub regions.
    /// </summary>
    /// <value>The sr sub regions.</value>
    private List<SubRegion> SrSubRegions { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Rid"/> class.
    /// </summary>
    /// <param name="ffxipath">The ffxipath.</param>
    /// <param name="rm">The rm.</param>
    /// <param name="logger"></param>
    public Rid(string ffxipath, RomPath rm, ILogger logger) {
      this.logger = logger;
      InstallPath = ffxipath;
      RomPath = rm;
      SubRegions = new BindingList<SubRegion>();
      SrSubRegions = new List<SubRegion>();
    }

    /// <summary>
    /// Dumps to XML.
    /// </summary>
    /// <param name="zone">The zone.</param>
    public void DumpToXml(int zone) {
      try {
        var path = ($@"{AppDomain.CurrentDomain.BaseDirectory}Sub Region Info");
        if (!Directory.Exists(path))
          Directory.CreateDirectory(path);
        if (!Directory.Exists(path))
          return;
        var outFile = File.Create($@"{path}\\ZoneID_{zone}_SubRegions.xml");
        var formatter = new XmlSerializer(SubRegions.GetType());
        formatter.Serialize(outFile, SubRegions);
        outFile.Close();
      } catch (Exception ex) {
        logger.Log($"{ex}");
      }
    }

    /// <summary>
    /// Parses the rid.
    /// </summary>
    /// <param name="block">The block.</param>
    /// <param name="id">The identifier.</param>
    public void ParseRid(byte[] block, int id) {
      try {
        var count = BitConverter.ToInt32(block, 0x30);
        for (var i = 0; i < count; i++) {
          var temp = new SubRegion {
            X = BitConverter.ToSingle(block, 0x40 + i * 0x40 + 0x00),
            Y = BitConverter.ToSingle(block, 0x40 + i * 0x40 + 0x04),
            Z = BitConverter.ToSingle(block, 0x40 + i * 0x40 + 0x08),
            RotationX = BitConverter.ToSingle(block, 0x40 + i * 0x40 + 0x0c),
            RotationY = BitConverter.ToSingle(block, 0x40 + i * 0x40 + 0x10),
            RotationZ = BitConverter.ToSingle(block, 0x40 + i * 0x40 + 0x14),
            ScaleX = BitConverter.ToSingle(block, 0x40 + i * 0x40 + 0x18),
            ScaleY = BitConverter.ToSingle(block, 0x40 + i * 0x40 + 0x1c),
            ScaleZ = BitConverter.ToSingle(block, 0x40 + i * 0x40 + 0x20)
          };
          var namebytes = new byte[8];
          Buffer.BlockCopy(block, 0x40 + i * 0x40 + 0x24, namebytes, 0, 8);
          var len = 0;
          while (len < namebytes.Length && namebytes[len] != 0)
            len++;
          len = 8;
          temp.Identifier = Encoding.ASCII.GetString(namebytes, 0, len).Trim('\0');
          if (temp.Identifier.Length > 0) {
            var sRtype = temp.Identifier.Substring(0, 1);
            switch (sRtype) {
              case "Z":
              case "z":
                temp.Type = "ZoneLine";
                break;

              case "_":
                temp.Type = "Door or Object";
                break;

              case "F":
              case "f":
                temp.Type = "Fishing area";
                break;

              case "@":
                temp.Type = "Elevators";
                break;

              case "E":
              case "e":
                temp.Type = "Event";
                break;

              case "M":
              case "m":

                temp.Type = "Model";
                temp.FileId = BitConverter.ToInt32(block, 0x40 + i * 0x40 + 0x2c);
                var fileId = temp.FileId + 100;
                temp.RomPath =
                  ($@"{InstallPath}{RomPath.GetRomPath(fileId, RomPath.TableDirectory)}");
                SubRegionModel++;
                break;

              default:
                temp.Type = sRtype;
                break;
            }
          }

          temp.Unknown = BitConverter.ToInt32(block, 0x40 + i * 0x40 + 0x30);
          {
            var vx = temp.ScaleX / 2;
            var vy = temp.ScaleY / 2;
            var vz = temp.ScaleZ / 2;
            var ry = -temp.RotationY;
          }
          SubRegions.Add(temp);
        }
      } catch (Exception ex) {
        logger.Log($"{ex}");
      }
    }
  }
}