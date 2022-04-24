// ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
// INTEL CONFIDENTIAL
// Copyright (2019) (2022) Intel Corporation
//
// The source code contained or described herein and all documents related to the source code ("Material") are
// owned by Intel Corporation or its suppliers or licensors. Title to the Material remains with Intel Corporation
// or its suppliers and licensors. The Material contains trade secrets and proprietary and confidential
// information of Intel Corporation or its suppliers and licensors. The Material is protected by worldwide copyright
// and trade secret laws and treaty provisions. No part of the Material may be used, copied, reproduced, modified,
// published, uploaded, posted, transmitted, distributed, or disclosed in any way without Intel Corporation's prior express
// written permission.
//
// No license under any patent, copyright, trade secret or other intellectual property right is granted to or
// conferred upon you by disclosure or delivery of the Materials, either expressly, by implication, inducement,
// estoppel or otherwise. Any license under such intellectual property rights must be express and approved by
// Intel in writing.
// ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace PValTC
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Defines PlistElement.
    /// </summary>
    public class PlistElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlistElement"/> class.
        /// </summary>
        /// <param name="burst">Burst index.</param>
        /// <param name="position">Position.</param>
        /// <param name="name">Name.</param>
        /// <param name="type">Type.</param>
        public PlistElement(string name, int position, string type, int burst)
        {
            _ = Enum.TryParse<PlistElementType>(type, true, out var plistElementType);
            this.Type = plistElementType;
            this.Position = position;
            this.Burst = burst;
            this.Name = name;
        }

        /// <summary>
        /// Defines Plist Element Types.
        /// </summary>
        public enum PlistElementType
        {
            /// <summary>
            /// Pattern type.
            /// </summary>
            Pattern,

            /// <summary>
            /// Plist type.
            /// </summary>
            PList,
        }

        /// <summary>
        /// Gets the position.
        /// </summary>
        public int Position { get; }

        /// <summary>
        /// Gets the burst index.
        /// </summary>
        public int Burst { get; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the Type.
        /// </summary>
        public PlistElementType Type { get; }

        /// <summary>
        /// Gets or sets the children.
        /// </summary>
        public List<PlistElement> Children { get; set; }
    }
}
