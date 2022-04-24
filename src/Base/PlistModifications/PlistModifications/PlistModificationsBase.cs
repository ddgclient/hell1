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

namespace PlistModificationsBase
{
    using Prime.PhAttributes;
    using Prime.TestMethods;

    /// <summary>
    /// Voltage Converter Test Class. Run during INIT to read configuration.
    /// </summary>
    [PrimeTestMethod]
    public class PlistModificationsBase : TestMethodBase
    {
        /// <summary>
        /// Enumerates different operation modes.
        /// </summary>
        public enum Mode
        {
            /// <summary>
            /// Restore; it reverts back all changes to original state.
            /// </summary>
            Restore,

            /// <summary>
            /// Removes node from tree; it will lose track of restore data.
            /// </summary>
            Clean,
        }

        /// <summary>
        /// Gets or sets the list of patlists to restore.
        /// </summary>
        public TestMethodsParams.CommaSeparatedString Patlists { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the operation mode.
        /// </summary>
        public Mode OperationMode { get; set; } = Mode.Restore;

        /// <inheritdoc />
        public override void Verify()
        {
        }

        /// <inheritdoc />
        [Returns(1, PortType.Pass, "Pass!")]
        [Returns(0, PortType.Fail, "Fail!")]
        public override int Execute()
        {
            var patlists = this.Patlists.ToList();
            if (patlists.Count > 0)
            {
                foreach (var patlist in patlists)
                {
                    switch (this.OperationMode)
                    {
                        case Mode.Restore:
                            DDG.PlistModifications.Service.RestoreTree(patlist);
                            break;
                        case Mode.Clean:
                            DDG.PlistModifications.Service.CleanTree(patlist);
                            break;
                    }
                }
            }
            else
            {
                switch (this.OperationMode)
                {
                    case Mode.Restore:
                        DDG.PlistModifications.Service.RestoreTree(string.Empty);
                        break;
                    case Mode.Clean:
                        DDG.PlistModifications.Service.CleanTree(string.Empty);
                        break;
                }
            }

            return 1;
        }
    }
}