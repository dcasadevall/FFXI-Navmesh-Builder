// ***********************************************************************
// Assembly         : FFXI NAVMESH BUILDER
// Author           : Xenonsmurf
// Created          : 04-29-2021
//
// Last Modified By : dcasadevall
// Last Modified On : 07-10-2021
// Original: https://github.com/LandSandBoat/FFXI-NavMesh-Builder-/blob/master/src/FFXI%20Navmesh%20Builder/Common/dat/ParseZoneModelDat.cs
// ***********************************************************************
// <copyright file="ParseZoneModelDat.cs" company="Xenonsmurf">
//     Copyright © Xenonsmurf 2021
// </copyright>
// <summary></summary>
// ***********************************************************************

using Ffxi_Navmesh_Builder.Common.dat.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FFXI_Navmesh_Builder_Forms.Logging;

namespace Ffxi_Navmesh_Builder.Common.dat {
  /// <summary>
  /// Class ParseZoneModelDat.
  /// </summary>
  public class ParseZoneModelDat {
    private readonly ILogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ParseZoneModelDat"/> class.
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="zid">The zid.</param>
    /// <param name="zname">The zname.</param>
    /// <param name="ffxIpath">The FFX ipath.</param>
    /// <param name="dumpSRinfo">if set to <c>true</c> [dump s rinfo].</param>
    public ParseZoneModelDat(ILogger logger,
                             int zid,
                             string zname,
                             string ffxIpath,
                             RomPath romPath,
                             // Per model dat. Contains normals triangles vertizes
                             Mzb mzb,
                             // Static
                             Mmb mmb,
                             // Per zone. Contains subregions
                             Rid rid,
                             bool dumpSRinfo) {
      this.logger = logger;
      ZoneId = zid;
      FileName = zname;
      InstallPath = ffxIpath;
      DumpSRtoXml = dumpSRinfo;
      RomPath = romPath;
      Mzb = mzb;
      Mmb = mmb;
      Rid = rid;
      
      try {
        Chunks = new List<DatChunk>();
      } catch (Exception ex) {
        logger.Log($"{ex}");
      }
    }

    /// <summary>
    /// Gets or sets the chunks.
    /// </summary>
    /// <value>The chunks.</value>
    public List<DatChunk> Chunks { get; set; }

    /// <summary>
    /// Gets or sets the MMB.
    /// </summary>
    /// <value>The MMB.</value>
    public Mmb Mmb { get; set; }

    /// <summary>
    /// Gets or sets the MZB.
    /// </summary>
    /// <value>The MZB.</value>
    public Mzb Mzb { get; set; }

    /// <summary>
    /// Gets or sets the rid.
    /// </summary>
    /// <value>The rid.</value>
    public Rid Rid { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether [dump s rto XML].
    /// </summary>
    /// <value><c>true</c> if [dump s rto XML]; otherwise, <c>false</c>.</value>
    private bool DumpSRtoXml { get; }

    /// <summary>
    /// Gets or sets the name of the file.
    /// </summary>
    /// <value>The name of the file.</value>
    private string FileName { get; }

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
    /// Gets or sets the zone identifier.
    /// </summary>
    /// <value>The zone identifier.</value>
    private int ZoneId { get; }

    /// <summary>
    /// Loads the dat.
    /// </summary>
    /// <param name="datPath">The dat path.</param>
    /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
    public bool LoadDat(string datPath) {
      try {
        Chunks.Clear();
        // var data = File.ReadAllBytes(datPath).AsSpan();
        using (var reader = new BinaryReader(File.Open(datPath, FileMode.Open))) {
          while (reader.BaseStream.Position != reader.BaseStream.Length) {
            var name = Encoding.UTF8.GetString(reader.ReadBytes(4));
            var value = BitConverter.ToUInt32(reader.ReadBytes(12), 0);
            var type = (ResourceType)(value & 0x7F);
            var size = 16 * ((value >> 7) & 0x7FFFF) - 16;
            var block = reader.ReadBytes((int)size);
            
            switch (type) {
              //dont need mmb for collision mesh
              case ResourceType.Mmb:
                // Mmb.DecodeMmb(block);
                break;

              case ResourceType.Mzb:
                //testing  code from ida
                // Mzb.sub_46B7A80(block,1);
                //testing  code from ida
                Mzb.DecodeMzb(block);
                Mzb.ParseMzb(block);
                break;

              case ResourceType.Rid:
                Rid.ParseRid(block, ZoneId);
                break;
            }
            
            Chunks.Add(new DatChunk {
              name = name,
              data = block,
              type = type,
              size = size
            });
          }
        }

        return true;
      } catch (Exception ex) {
        logger.Log($"{ex}");
        return false;
      }
    }
  }
}