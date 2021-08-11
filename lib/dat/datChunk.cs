// ***********************************************************************
// Assembly         : FFXI NAVMESH BUILDER
// Author           : Xenonsmurf
// Created          : 04-29-2021
//
// Last Modified By : dcasadevall
// Last Modified On : 07-10-2021
// Original: https://github.com/LandSandBoat/FFXI-NavMesh-Builder-/blob/master/src/FFXI%20Navmesh%20Builder/Common/dat/datChunk.cs
// ***********************************************************************
// <copyright file="datChunk.cs" company="Xenonsmurf">
//     Copyright © Xenonsmurf 2021
// </copyright>
// <summary></summary>
// ***********************************************************************
using Ffxi_Navmesh_Builder.Common.dat.Types;

namespace Ffxi_Navmesh_Builder.Common.dat
{
    /// <summary>
    /// Class DatChunk.
    /// </summary>
    public class DatChunk {
        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>The data.</value>
        public byte[] data = new byte[0];
        
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string name = "";
        
        /// <summary>
        /// Gets or sets the size.
        /// </summary>
        /// <value>The size.</value>
        public uint size = 0;
        
        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>The type.</value>
        public ResourceType type = ResourceType.Terminate;
    }
}