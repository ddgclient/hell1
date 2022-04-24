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

namespace DDG
{
    using Prime.SharedStorageService;

    /// <summary>
    /// Defines the <see cref="VminForwarding" />.
    /// </summary>
    public static partial class VminForwarding
    {
        /// <summary>
        /// Defines the <see cref="Globals" />.
        /// </summary>
        public static class Globals
        {
            /// <summary>
            /// Gets the name of the Vmin Forwarding SharedStorage Object which contains the snapshot data.
            /// </summary>
            public static readonly string VminForwardingSnapshot = "__DDG_VminForwardingSnapshotValues__";

            /// <summary>
            /// Gets the context of the Vmin Forwarding SharedStorage Object which contains the snapshot data.
            /// </summary>
            public static readonly Context VminForwardingSnapshotContext = Context.DUT;

            /// <summary>
            /// Gets the name of the Vmin Forwarding SharedStorage Object which contains the VminForwarding Domain/Corner to DFF Token mapping.
            /// </summary>
            public static readonly string VminForwardingDffMap = "__DDG_VminForwardingDffMap__";

            /// <summary>
            /// Gets the context of the Vmin Forwarding SharedStorage Object which contains the VminForwarding Domain/Corner to DFF Token mapping.
            /// </summary>
            public static readonly Context VminForwardingDffMapContext = Context.DUT;

            /// <summary>
            /// Gets the context for any of the VminForwarding flags.
            /// </summary>
            public static readonly Context VminForwardingFlagContext = Context.DUT;

            /// <summary>
            /// Gets the name of the Vmin Forwarding SharedStorage Object which contains flag which forces Vmin searches to be single point tests.
            /// </summary>
            public static readonly string VminForwardingSinglePointMode = "__DDG_VminForwardingSinglePointMode__";

            /// <summary>
            /// Gets the name of the Vmin Forwarding SharedStorage Object which contains flag for VminTC SearchGuardband.
            /// </summary>
            public static readonly string VminForwardingSearchGuardbandEnable = "__DDG_VminForwardingSearchGuardbandEnable__";

            /// <summary>
            /// Gets the name of the Vmin Forwarding SharedStorage Object which contains StoreVoltages flag.
            /// </summary>
            public static readonly string VminForwardingUseLimitCheckAsSourceEnable = "__DDG_VminForwardingUseLimitCheckAsSourceFlag__";

            /// <summary>
            /// Gets the name of the Vmin Forwarding SharedStorage Object which contains StoreVoltages flag.
            /// </summary>
            public static readonly string VminForwardingUseVoltagesSourcesEnable = "__DDG_VminForwardingUseVoltagesSourcesFlag__";

            /// <summary>
            /// Gets the name of the Vmin Forwarding SharedStorage Object which contains StoreVoltages flag.
            /// </summary>
            public static readonly string VminForwardingUseLimitCheckEnable = "__DDG_VminForwardingUseLimitCheckFlag__";

            /// <summary>
            /// Gets the name of the Vmin Forwarding SharedStorage Object which contains StoreVoltages flag.
            /// </summary>
            public static readonly string VminForwardingStoreVoltagesEnable = "__DDG_VminForwardingStoreVoltagesFlag__";

            /// <summary>
            /// Gets the string used as a separator in shared storage naming.
            /// </summary>
            public static readonly string NameSeparator = "!";
        }
    }
}
