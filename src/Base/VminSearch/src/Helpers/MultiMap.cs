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

namespace Prime.TestMethods.VminSearch.Helpers
{
    using System.Collections.Generic;

    /// <summary>
    /// Multimap helper class for a Dictionary with multiple values per key.
    /// </summary>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TValue">Value type.</typeparam>
    internal class MultiMap<TKey, TValue>
    {
        private readonly Dictionary<TKey, List<TValue>> dictionary =
            new Dictionary<TKey, List<TValue>>();

        /// <summary>
        /// Gets the collection of keys.
        /// </summary>
        public IEnumerable<TKey> Keys => this.dictionary.Keys;

        /// <summary>
        /// Indexer for keys.
        /// </summary>
        /// <param name="key">Key value.</param>
        /// <returns>List of elements.</returns>
        public List<TValue> this[TKey key]
        {
            get
            {
                // Get list at a key.
                if (!this.dictionary.TryGetValue(key, out var list))
                {
                    list = new List<TValue>();
                    this.dictionary[key] = list;
                }

                return list;
            }
        }

        /// <summary>
        /// Adds a value to the structure.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        public void Add(TKey key, TValue value)
        {
            // Add a key.
            if (this.dictionary.TryGetValue(key, out var list))
            {
                list.Add(value);
            }
            else
            {
                list = new List<TValue> { value };
                this.dictionary[key] = list;
            }
        }

        /// <summary>
        /// Checks if a key exists in the map.
        /// </summary>
        /// <param name="key">The key value.</param>
        /// <returns>True if the key exists.</returns>
        public bool KeyExists(TKey key) => this.dictionary.ContainsKey(key);
    }
}
