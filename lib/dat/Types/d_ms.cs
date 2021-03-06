// ***********************************************************************
// Assembly         : FFXI NAVMESH BUILDER
// Author           : Xenonsmurf
// Created          : 04-29-2021
//
// Last Modified By : dcasadevall
// Last Modified On : 07-10-2021
// Original: https://github.com/xenonsmurf/Ffxi_Navmesh_Builder/blob/master/src/FFXI%20Navmesh%20Builder/Common/dat/Types/d_ms.cs
// ***********************************************************************
// <copyright file="d_ms.cs" company="Xenonsmurf">
//     Copyright © Xenonsmurf 2021
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using FFXI_Navmesh_Builder_Forms.Logging;

namespace Ffxi_Navmesh_Builder.Common.dat.Types {
  /// <summary>
  /// Class d_ms.
  /// </summary>
  public class d_ms {
    private readonly ILogger logger;

    /// <summary>
    /// The ignore
    /// </summary>
    private readonly List<int> _ignore = new List<int> {
      133,
      278,
      286,
      189,
      199,
      210,
      214,
      219,
      229,
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="d_ms"/> class.
    /// </summary>
    /// <param name="l">The l.</param>
    /// <param name="mf">The mf.</param>
    /// <param name="rp">The rp.</param>
    public d_ms(RomPath rp, ILogger logger) {
      this.logger = logger;
      
      romPath = rp;
      _zones = new ObservableCollection<Zones>();
    }

    /// <summary>
    /// Gets or sets the zones.
    /// </summary>
    /// <value>The zones.</value>
    public ObservableCollection<Zones> _zones { get; set; }

    /// <summary>
    /// Gets or sets the position.
    /// </summary>
    /// <value>The position.</value>
    private int position { get; set; } = 0;

    /// <summary>
    /// Gets or sets the rom path.
    /// </summary>
    /// <value>The rom path.</value>
    private RomPath romPath { get; set; }

    /// <summary>
    /// Decrypteds the specified data.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
    public bool Decrypted(byte[] data) {
      try {
        while (position < data.Length) {
          data[position] ^= 0xff;
          position++;
        }

        return true;
      } catch (Exception ex) {
        logger.Log($"{ex}");
        return false;
      }
    }

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
        var outFile = File.Create($@"{path}\\ZoneList.xml");
        var formatter = new XmlSerializer(_zones.GetType());
        formatter.Serialize(outFile, _zones);
        outFile.Close();
      } catch (Exception ex) {
        logger.Log($"{ex}");
      }
    }

    /// <summary>
    /// Parses the d MSG.
    /// </summary>
    /// <param name="data">The data.</param>
    public void ParseD_MSG(byte[] data) {
      try {
        ResetList();
        if (!Decrypted(data))
          return;
        var pos = 0x9C0;
        var i = 0;
        while (pos < data.Length) {
          var offset = 1;
          var text = Encoding.ASCII.GetString(data, pos, offset);
          while (!text.Contains("\0")) {
            text = Encoding.ASCII.GetString(data, pos, offset);
            offset++;
          }

          var name = text.TrimEnd('\0');
          if (name != "" && name != "\u0001" && name != "\f") {
            var zone = new Zones();
            if (_ignore.Contains(i)) {
              i++;
            }

            var fileId = i < 256 ? i + 100 : i + 83635;
            zone.Id = i;
            zone.Name = name;
            zone.Path = romPath.GetRomPath(fileId, romPath.TableDirectory);

            _zones.Add(zone);
            i++;
          }

          pos += text.Length;
          if (pos > data.Length) {
            break;
          }
        }

        var sortedList = new ObservableCollection<Zones>(_zones.OrderBy(x => x.Id).ToList());
        _zones = sortedList;
      } catch (Exception ex) {
        logger.Log($"{ex}");
      }
    }

    /// <summary>
    /// Resets the list.
    /// </summary>
    private void ResetList() {
      _zones.Clear();
      _zones.Add(new Zones {
        Id = 133,
        Name = "Lobby",
        Path = "ROM/1/5.DAT"
      });
      _zones.Add(new Zones {
        Id = 278,
        Name = "None1",
        Path = romPath.GetRomPath(83913, romPath.TableDirectory)
      });
      _zones.Add(new Zones {
        Id = 286,
        Name = "None2",
        Path = romPath.GetRomPath(83921, romPath.TableDirectory)
      });
      _zones.Add(new Zones {
        Id = 189,
        Name = "Residential_Area1",
        Path = romPath.GetRomPath(289, romPath.TableDirectory)
      });
      _zones.Add(new Zones {
        Id = 199,
        Name = "Residential_Area2",
        Path = romPath.GetRomPath(299, romPath.TableDirectory)
      });
      _zones.Add(new Zones {
        Id = 210,
        Name = "GM_Home",
        Path = romPath.GetRomPath(310, romPath.TableDirectory)
      });
      _zones.Add(new Zones {
        Id = 214,
        Name = "Residential_Area3",
        Path = romPath.GetRomPath(314, romPath.TableDirectory)
      });
      _zones.Add(new Zones {
        Id = 219,
        Name = "Residential_Area4",
        Path = romPath.GetRomPath(319, romPath.TableDirectory)
      });
      _zones.Add(new Zones {
        Id = 229,
        Name = "None",
        Path = romPath.GetRomPath(329, romPath.TableDirectory)
      });
    }
  }
}