// ***********************************************************************
// Assembly         : FFXI NAVMESH BUILDER
// Author           : Xenonsmurf
// Created          : 04-29-2021
//
// Last Modified By : dcasadevall
// Last Modified On : 07-10-2021
// Original: https://github.com/LandSandBoat/FFXI-NavMesh-Builder-/blob/master/src/FFXI%20Navmesh%20Builder/Common/dat/Types/MZB.cs
// ***********************************************************************
// <copyright file="MZB.cs" company="Xenonsmurf">
//     Copyright © Xenonsmurf 2021
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using FFXI_Navmesh_Builder_Forms.Logging;

namespace Ffxi_Navmesh_Builder.Common.dat.Types {
  /// <summary>
  /// Class Mzb.
  /// </summary>
  public class Mzb {
    private readonly ILogger logger;

    /// <summary>
    /// All normals
    /// </summary>
    private readonly List<Normal> _allNormals = new List<Normal>();

    /// <summary>
    /// All triangles
    /// </summary>
    private readonly List<Triangle> _allTriangles = new List<Triangle>();

    /// <summary>
    /// All vertices
    /// </summary>
    private readonly List<Vertex> _allVertices = new List<Vertex>();

    /// <summary>
    /// The vismapid
    /// </summary>
    private readonly Dictionary<int, uint> _vismapid = new Dictionary<int, uint>();

    public Mzb(ILogger logger) {
      this.logger = logger;
    }

    /// <summary>
    /// Writes the object.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
    public bool WriteObj(string fileName) {
      var path = $@"{Directory.GetCurrentDirectory()}\Map Collision obj files\{fileName}.obj";
      if (File.Exists(path)) {
        logger.Log($"Not overwriting {fileName}");
        return false;
      }
      
      try {
        using (var sw = new StreamWriter(path)) {
          foreach (var v in _allVertices) {
            sw.WriteLine($@"v {v.X.ToString(CultureInfo.InvariantCulture)} {v.Y} {-v.Z}");
          }

          foreach (var n in _allNormals) {
            sw.WriteLine($@"vn {n.Nx} {n.Ny} {-n.Nz}");
          }

          foreach (var t in _allTriangles) {
            sw.WriteLine($@"f {1 + t.Iv0}//{1 + t.In0} {1 + t.Iv1}//{1 + t.In0} {1 + t.Iv2}//{1 + t.In0}");
          }

          return true;
        }
      } catch (Exception ex) {
        logger.Log($"{ex}");
        return false;
      }
    }

    /// <summary>
    /// Decodes the MZB.
    /// </summary>
    /// <param name="data">The data.</param>
    internal void DecodeMzb(byte[] data) {
      try {
        if (data[3] < 0x1B) {
          return;
        }

        var decode_length = BitConverter.ToInt32(data, 0) & 0x00FFFFFF;

        var key = (int)KeyTables.KeyTable[data[7] ^ 0xFF];
        var keyCount = 0;

        for (var pos = 8; pos < decode_length;) {
          var xorLength = ((key >> 4) & 7) + 16;

          if ((key & 1) == 1 && (pos + xorLength < decode_length)) {
            for (var i = 0; i < xorLength; i++) {
              data[pos + i] ^= 0xFF;
            }
          }

          key += ++keyCount;
          pos += xorLength;
        }

        var nodeCount = BitConverter.ToInt32(data, 4) & 0x00FFFFFF;

        for (var i = 0; i < nodeCount; i++) {
          for (var j = 0; j < 16; j++) {
            data[0x20 + i * 0x64 + j] ^= 0x55;
          }
        }
      } catch (Exception ex) {
        logger.Log($"{ex}");
      }
    }

    /// <summary>
    /// Parses the MZB.
    /// </summary>
    /// <param name="block">The block.</param>
    internal void ParseMzb(byte[] block) {
      try {
        int meshoffset;
        var mOffset = 8;

        //I added this to help with ships.
        while (true) {
          meshoffset = BitConverter.ToInt32(block, mOffset);
          if (meshoffset == 0) {
            mOffset += 4;
          }

          if (meshoffset != 0) {
            break;
          }
        }

        if (meshoffset <= 0 || meshoffset >= block.Length) {
          return;
        }

        var quadtreeoffset = BitConverter.ToInt32(block, 0x10);
        var meshCount = BitConverter.ToInt32(block, meshoffset);
        var meshData = BitConverter.ToInt32(block, meshoffset + 0x4);

        var offset = meshData;
        for (var i = 0; i < meshCount; ++i) {
          offset = ParseMesh(block, offset);
        }

        var gridOffset = BitConverter.ToInt32(block, meshoffset + 0x10);
        if (gridOffset > 0) {
          //added *10 here to help with Port Jeuno and Chateau_dOraguille
          var gridwidth = block[0x0c] * 10;
          var gridheight = block[0x0d] * 10;
          for (var y = 0; y < gridheight * 10; ++y) {
            for (var x = 0; x < gridwidth * 10; ++x) {
              var offsets = (y * gridwidth * 10 + x) * 4;
              if (offsets <= 0 || offsets >= block.Length) {
                continue;
              }

              if (gridOffset + offsets >= block.Length) {
                continue;
              }

              var entryOffset = BitConverter.ToInt32(block, gridOffset + offsets);
              if (entryOffset > 0 && entryOffset < block.Length) {
                ParseGridEntry(block, entryOffset, x, y);
              }
            }
          }
        }

        var maplistOffset = BitConverter.ToInt32(block, 0x14);
        var maplistCount = BitConverter.ToInt32(block, 0x18);

        for (var i = 0; i < maplistCount; ++i) {
          var pos = (maplistOffset + 0xc0 * i);
          if (pos >= block.Length) {
            continue;
          }

          var offsetEncoded = pos + 0x29 * 4;
          if (offsetEncoded >= block.Length) {
            continue;
          }

          var mapidEncoded = BitConverter.ToInt32(block, offsetEncoded);
          if (pos + 0x2a * 4 >= block.Length) {
            continue;
          }

          var objvisOffset = BitConverter.ToInt32(block, pos + 0x2a * 4);
          if (pos + 0x2b * 4 >= block.Length) {
            continue;
          }

          var objvisCount = BitConverter.ToInt32(block, pos + 0x2b * 4);
          if (pos + 0x2d * 4 >= block.Length) {
            continue;
          }

          var x = BitConverter.ToSingle(block, pos + 0x2d * 4);
          if (pos + 0x2e * 4 >= block.Length) {
            continue;
          }

          var y = BitConverter.ToSingle(block, pos + 0x2e * 4);
          var mapid = ((mapidEncoded >> 3) & 0x7) | (((mapidEncoded >> 26) & 0x3) << 3);
          if (!_vismapid.ContainsKey(i)) {
            _vismapid.Add(i, (uint)mapid);
          }
        }

        var quadtree = ParseQuadTree(block, quadtreeoffset);
      } catch (Exception ex) {
        logger.Log($"{ex}");
      }
    }

    /// <summary>
    /// Parses the grid entry.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <param name="entryoffs">The entryoffs.</param>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    private void ParseGridEntry(byte[] data, int entryoffs, int x, int y) {
      try {
        if (entryoffs <= 0 || entryoffs > data.Length) {
          return;
        }

        var entries = new List<int>();

        while (true) {
          var c = BitConverter.ToInt32(data, entryoffs);
          if (c == 0) {
            break;
          }

          entries.Add(c);
          entryoffs += 4;
        }

        if (!entries.Any()) {
          return;
        }

        var pos = (uint)entries[0];
        var xx = (int)((pos >> 14) & 0x1ff);
        var yy = (int)((pos >> 23) & 0x1ff);
        var flags = (int)(pos & 0x3fff);

        for (var i = 1; i < entries.Count; i += 2) {
          if (i + 1 < entries.Count) {
            var visentryoffset = entries[i + 0];
            var geometryoffset = entries[i + 1];
            if (visentryoffset > 0 &&
                geometryoffset > 0 &&
                visentryoffset < data.Length &&
                geometryoffset < data.Length) {
              ParseGridMesh(data, x, y, visentryoffset, geometryoffset);
            } else {
              break;
            }
          }
        }
      } catch (Exception ex) {
        logger.Log($"{ex}");
      }
    }

    /// <summary>
    /// Parses the grid mesh.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <param name="visentryoffset">The visentryoffset.</param>
    /// <param name="geometryoffset">The geometryoffset.</param>
    private void ParseGridMesh(byte[] data,
                               int x,
                               int y,
                               int visentryoffset,
                               int geometryoffset) {
      try {
        var m1 = new float[16];
        var m = new Matrix4x4();
        var m2 = new float[9];
        for (var i = 0; i < 16; i++) {
          var currValue = BitConverter.ToSingle(data, visentryoffset + i * 4);
          m1[i] = currValue;
          switch (i) {
            case 0:
              m.M11 = currValue;
              m2[0] = currValue;
              break;

            case 1:
              m.M12 = currValue;
              m2[1] = currValue;
              break;

            case 2:
              m.M13 = currValue;
              m2[2] = currValue;
              break;

            case 3:
              m.M14 = currValue;
              break;

            case 4:
              m.M21 = currValue;
              m2[3] = currValue;
              break;

            case 5:
              m.M22 = currValue;
              m2[4] = currValue;
              break;

            case 6:
              m.M23 = currValue;
              m2[5] = currValue;
              break;

            case 7:
              m.M24 = currValue;
              break;

            case 8:
              m.M31 = currValue;
              m2[6] = currValue;
              break;

            case 9:
              m.M32 = currValue;
              m2[7] = currValue;
              break;

            case 10:
              m.M33 = currValue;
              m2[8] = currValue;
              break;

            case 11:
              m.M34 = currValue;
              break;

            case 12:
              m.M41 = currValue;
              break;

            case 13:
              m.M42 = currValue;
              break;

            case 14:
              m.M43 = currValue;
              break;

            case 15:
              m.M44 = currValue;
              break;

            default:
              break;
          }
        }

        var vertices = BitConverter.ToInt32(data, geometryoffset + 0x00);
        var normals = BitConverter.ToInt32(data, geometryoffset + 0x04);
        var tris = BitConverter.ToInt32(data, geometryoffset + 0x08);
        int tricount = BitConverter.ToInt16(data, geometryoffset + 0x0c);
        int flags = BitConverter.ToInt16(data, geometryoffset + 0x0e);

        var doesntBlockLineOfSight = (flags & 1) != 0;
        var numvert = (normals - vertices) / 12;
        if (numvert <= 0) {
          return;
        }

        {
          var numnorm = (tris - normals) / 12;

          var basevert = _allVertices.Count;
          var basenorm = _allNormals.Count;
          var basetri = _allTriangles.Count;

          var determ = m2[0] * (m2[4] * m2[8] - m2[5] * m2[7]) -
                       m2[1] * (m2[3] * m2[8] - m2[5] * m2[6]) +
                       m2[2] * (m2[3] * m2[7] - m2[4] * m2[6]);
          for (var i = 0; i < numvert; i++) {
            if (vertices > 0) {
              var x1 = BitConverter.ToSingle(data, vertices + i * 3 + 0);
              var y1 = BitConverter.ToSingle(data, vertices + i * 3 + 1);
              var z1 = BitConverter.ToSingle(data, vertices + i * 3 + 2);
              var w1 = 1.0f;

              _allVertices.Add(new Vertex() {
                X = m1[0] * x1 + m1[4] * y1 + m1[8] * z1 + m1[12] * w1,
                Y = -(m1[1] * x1 + m1[5] * y1 + m1[9] * z1 + m1[13] * w1),
                Z = m1[2] * x1 + m1[6] * y1 + m1[10] * z1 + m1[14] * w1
              });
            }
          }

          if (numnorm > 0) {
            for (var i = 0; i < numnorm; i++) {
              _allNormals.Add(new Normal {
                Nx = BitConverter.ToSingle(data, normals + i * 3 + 0),
                Ny = -BitConverter.ToSingle(data, normals + i * 3 + 1),
                Nz = BitConverter.ToSingle(data, normals + i * 3 + 2)
              });
            }
          }

          if (tricount <= 0) {
            return;
          }

          {
            for (var i = 0; i < tricount; i++) {
              if (determ > 0) {
                _allTriangles.Add(new Triangle {
                  Iv0 =
                    basevert +
                    BitConverter.ToUInt16(data, (tris + i * 4 + 2) & 0x3fff),
                  Iv1 =
                    basevert +
                    BitConverter.ToUInt16(data, (tris + i * 4 + 1) & 0x3fff),
                  Iv2 = basevert +
                        BitConverter.ToUInt16(data, (tris + i * 4 + 0) & 0x3fff),
                  In0 = basenorm +
                        BitConverter.ToUInt16(data, (tris + i * 4 + 3) & 0x3fff)
                });
              } else {
                _allTriangles.Add(new Triangle() {
                  Iv0 =
                    basevert +
                    BitConverter.ToUInt16(data, (tris + i * 4 + 0) & 0x3fff),
                  Iv1 =
                    basevert +
                    BitConverter.ToUInt16(data, (tris + i * 4 + 1) & 0x3fff),
                  Iv2 = basevert +
                        BitConverter.ToUInt16(data, (tris + i * 4 + 2) & 0x3fff),
                  In0 = basenorm +
                        BitConverter.ToUInt16(data, (tris + i * 4 + 3) & 0x3fff)
                });
              }
            }
          }
        }
      } catch (Exception ex) {
        logger.Log($"{ex}");
      }
    }

    /// <summary>
    /// Parses the mesh.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <param name="pos">The position.</param>
    /// <returns>System.Int32.</returns>
    private int ParseMesh(byte[] data, int pos) {
      try {
        if (pos < data.Length && pos > 0) {
          var verticesOffset = BitConverter.ToInt32(data, pos + 0x00);
          var normalsOffset = BitConverter.ToInt32(data, pos + 0x04);
          var triangles = BitConverter.ToInt32(data, pos + 0x08);
          var triangleCount = BitConverter.ToInt32(data, pos + 0x0c);
          var flags = BitConverter.ToInt16(data, pos + 0x0e);
          return triangles + triangleCount * sizeof(UInt16) * 4;
        } else {
          return 0;
        }
      } catch (Exception ex) {
        logger.Log($"{ex}");
        return 0;
      }
    }

    /// <summary>
    /// Parses the quad tree.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <param name="pos">The position.</param>
    /// <returns>Bbt.</returns>
    private Bbt ParseQuadTree(byte[] data, int pos) {
      try {
        var bb = new float[8 * 3];
        var children = new int[4];

        for (var i = 0; i < 8 * 3; i++) {
          bb[i] = BitConverter.ToSingle(data, pos + i * 4);
        }

        var vislistoffset = BitConverter.ToInt32(data, pos + 8 * 3 * 4);
        var vislistcount = BitConverter.ToInt32(data, pos + 8 * 3 * 4 + 4);
        for (var i = 0; i < 4; i++) {
          children[i] = BitConverter.ToInt32(data, pos + 8 * 3 * 4 + 4 + 4 + i * 4);
        }

        var b = new Bbt(bb);
        var visset = new HashSet<uint>();

        if (vislistcount > 0) {
          for (var i = 0; i < vislistcount; i++) {
            var node = BitConverter.ToInt32(data, vislistoffset + i * 4);
            b.Matchids.Add(node);

            var mapid = _vismapid[node];
            if (!visset.Contains(mapid)) {
              visset.Add(mapid);
            }
          }

          foreach (var mapid in visset) {
            b.Matches.Add(mapid);
          }
        }

        for (var i = 0; i < 4; i++) {
          if (children[i] != 0) {
            var child = ParseQuadTree(data, children[i]);
            b.Children.Add(child);
          }
        }

        return b;
      } catch (Exception ex) {
        logger.Log($"{ex}");
        return null;
      }
    }
  }
}