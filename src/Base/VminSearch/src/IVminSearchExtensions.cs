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

namespace Prime.TestMethods.VminSearch
{
    using System.Collections;
    using System.Collections.Generic;
    using Prime.FunctionalService;
    using Prime.VoltageService;

    /// <summary>
    /// Class to define extendable methods.
    /// </summary>
    public interface IVminSearchExtensions
    {
        /// <summary>
        /// 0.1. Called in Verify for voltage object initialization.
        /// Returned object determines how the Voltages are applied.
        /// Default implementation returns a DPS or a FIVR (if FeatureSwitchSettings parameter value contains "fivr_mode_on") IVoltage object.
        /// </summary>
        /// <param name="targets">target key names from current instance VoltageTargets parameter.
        /// These will be used to apply search voltages to each of them with a a method that depends of the type of returned voltage object.</param>
        /// <param name="plistName">plist name from current instance Patlist parameter.</param>
        /// <returns>IVoltage object which must be created with one of the corresponding available options in VoltageService.</returns>
        /// <exception cref="Prime.Base.Exceptions.FatalException">If either of the parameters is invalid.</exception>
        IVoltage GetSearchVoltageObject(List<string> targets, string plistName);

        /// <summary>
        /// 0.2. Called in Verify during initialization of SetPointTest object.
        /// Returned object determines how the plist is executed and which results are captured.
        /// Default implementation returns a IFunctionalTest of type ICaptureFailureTest.
        /// </summary>
        /// <param name="patlist">plist name from current instance Patlist parameter.</param>
        /// <param name="levelsTc">level name from current instance LevelsTc parameter.</param>
        /// <param name="timingsTc">level name from current instance TimingsTc parameter.</param>
        /// <param name="prePlist">The callback to run before the plist execution.</param>
        /// <returns>IFunctionalTest object which must be created with one of the corresponding available options in FunctionalService.</returns>
        /// <exception cref="Prime.Base.Exceptions.FatalException">If either of the parameters is invalid.</exception>
        IFunctionalTest GetFunctionalTest(string patlist, string levelsTc, string timingsTc, string prePlist);

        /// <summary>
        /// 0.3. Called in Verify during initialization of SetPointTest object.
        /// Returned boolean determines if the search continues or stops after execution of the first search point.
        /// Default implementation returns false.
        /// A True value affects the behavior of the extensions default implementations:
        /// - ExecuteScoreboard: skip the final plist execution if the functional object is a Capture Failure type and per-pattern capture mode is enabled.
        /// - PostProcessSearchResults: does not print the search results to the ituff.
        /// - ApplySearchVoltage: skip execution.
        /// </summary>
        /// <returns>True if single point test mode should be executed instead of a complete search.</returns>>
        bool IsSinglePointMode();

        /// <summary>
        /// 0.4. Called in Verify during initialization of SetPointTest object.
        /// Returned boolean determines if the base method evaluates extra rules on the result bits returned by ProcessPlistResults extension.
        /// If the check is enabled and it fails the extension returned bits are ignored and replaced by an all bits failing result.
        /// These extra checks includes verifying that masked positions are passing and there is at least on fail position if the plist fails.
        /// Default implementation returns true.
        /// </summary>
        /// <returns>True if it is intended to evaluate extra checking in the results bits returned by ProcessPlistResults extension.</returns>>
        bool IsCheckOfResultBitsEnabled();

        /// <summary>
        /// 0. Called at the beginning of the Execute. Allows the user to create a custom check for a complete instance bypass and returning a custom port.
        /// Default implementation returns a -1, allowing the execution.
        /// </summary>
        /// <returns>Port number: value = -1 to continue, value >= 0 to bypass execution and return the port number.</returns>
        int GetBypassPort();

        /// <summary>
        /// 1. Called before any other test method code in Execute.
        /// Allows execution of any pre-instance required setup (for example variable initializations from external sources).
        /// </summary>
        /// <param name="plistName">plist name from current instance Patlist parameter, for optional use.</param>
        void ApplyPreExecuteSetup(string plistName);

        /// <summary>
        /// 2. Called after level and timing test conditions are loaded at the beginning in Execute.
        /// Allows implementation of additional initial voltage setups.
        /// Default implementation calls to voltageObject.?Apply(), where voltageObject is valid only if FivrCondition parameter is defined.
        /// </summary>
        /// <param name="voltageObject">IVoltage object that handle apply method, in FIVR mode only.</param>
        void ApplyInitialVoltage(IVoltage voltageObject);

        /// <summary>
        /// 3. Called as first step of each iteration of a for loop, which is intended to optionally execute more than one search.
        /// Allows execution of any pre-search required setup (for example setting a condition value that must be different for each search).
        /// </summary>
        /// <param name="plistName">plist name from current instance Patlist parameter, for optional use.</param>
        void ApplyPreSearchSetup(string plistName);

        /// <summary>
        /// 4.1 Called as part of the SearchPointTest Reset() method which is called before entering into the search loop.
        /// Allows replacing voltages values used for search start.
        /// Default implementation calls StringUtilities.ConvertKeysToDouble(startVoltageKeys) to return a list of double values.
        /// Keys in startVoltageKeys can be double format values or key names that for default implementation must already exist
        /// in the shared storage system as doubles.
        /// </summary>
        /// <param name="startVoltageKeys">list of key names taken from StartVoltages parameter.</param>
        /// <returns>Voltage values to be used to start the search.</returns>
        List<double> GetStartVoltageValues(List<string> startVoltageKeys);

        /// <summary>
        /// 4.2. Called as part of the SearchPointTest IsSearchCompleted whenever overshoot required and enabled.
        /// Allows initializing the lower start voltage values for the new search in the overshoot.
        /// Default implementation calls StringUtilities.ConvertKeysToDouble(lowerStartVoltageKeys) to return a list of double values.
        /// Keys in lowerStartVoltageKeys can be double format values or key names that for default implementation must already exist
        /// in the shared storage system as doubles.
        /// </summary>
        /// <param name="lowerStartVoltageKeys">list of key names taken from LowerStartVoltageKeys parameter.</param>
        /// <returns>Lower start voltage values to be used in overshoot.</returns>
        List<double> GetLowerStartVoltageValues(List<string> lowerStartVoltageKeys);

        /// <summary>
        /// 5. Called as part of the SearchPointTest Reset() method which is called before entering into the search loop.
        /// Allows replacing voltages values used for search end limit.
        /// Default implementation calls StringUtilities.ConvertKeysToDouble(endVoltageLimitKeys) to return a list of double values.
        /// Keys in endVoltageLimitKeys can be double format values or key names that for default implementation must already exist
        /// in the shared storage system as doubles.
        /// </summary>
        /// <param name="endVoltageLimitKeys">list of key names taken from EndVoltageLimits parameter.</param>
        /// <returns>Voltage values to be used as search end limits.</returns>
        List<double> GetEndVoltageLimitValues(List<string> endVoltageLimitKeys);

        /// <summary>
        /// 6. Called as part of the SearchPointTest Reset() method which is called before entering into the search loop.
        /// Allows setting the initial mask value for the current search loop. This mask value is used to ignore (for the bits set to true)
        /// voltage increases for corresponding targets, which corresponding results are set to a default value of -8888.0.
        /// </summary>
        /// <returns>bit array to be used as initial search mask value.</returns>
        BitArray GetInitialMaskBits();

        /// <summary>
        /// 7. Called before search point test execution.
        /// Allows to override or complement methodology for voltage apply.
        /// Default implementation calls to IVoltage.Apply(voltageValues).
        /// </summary>
        /// <param name="voltageObject">handler object for applying of the required voltages, Depending of the type chosen in Verify with GetSearchVoltageObject.</param>
        /// <param name="voltageValues">Voltage values required to be applied for current search point.</param>
        void ApplySearchVoltage(IVoltage voltageObject, List<double> voltageValues);

        /// <summary>
        /// 8. Called before each search point plist execution.
        /// Method to be implemented by the user to effectively apply a mask that makes the plist execution ignoring (not report) any fail for the masked target positions.
        /// This method receives the internal required mask value (combination of initial mask and any other target that already reached to the search limit) but depending
        /// of the user implementation requirements the user can use other inputs. The convention for this input is that a true in the bit array must be masked.
        /// </summary>
        /// <param name="maskBits"> bit array with the value of current search point required mask.</param>
        /// <param name="functionalTest">functional test object to optionally be used to apply the mask in the case of a pin masking method.</param>
        void ApplyMask(BitArray maskBits, IFunctionalTest functionalTest);

        /// <summary>
        /// 9. Called after each search point plist execution.
        /// Method to be implemented by the user to decode plist results into a bit array. This returned bit array will be used by the test method to decide which targets
        /// must increase voltage for next search point. The convention is that a true in the bit array correspond to a fail position and so will be increased or disabled
        /// (if reached to the limit) for next search point.
        /// Default implementation returns all bits in true if plistExecuteResult is false, or all bits in false if plistExecuteResult is true.
        /// </summary>
        /// <param name="plistExecuteResult">result of previous plist execution (IFunctionalTest.Execute), true for a passing case and false for a failing one.</param>
        /// <param name="functionalTest">functional test object that provide methods to extract captured data from previous plist execution.</param>
        /// <returns>bit string.</returns>
        BitArray ProcessPlistResults(bool plistExecuteResult, IFunctionalTest functionalTest);

        /// <summary>
        /// 10. Called whenever a search is completed.
        /// If IsSinglePointMode = true and per-pattern capture mode is enabled only the Scoreboard counters are printed to ituff (without plist re-execution). Otherwise the Scoreboard execution is called.
        /// Execution happens only if the plist failed at least once.
        /// </summary>
        /// <param name="executionIdentifier">String of the form MxRy. Where x is the multi pass execution count and y is the repetition count.</param>
        /// <param name="isLastSearchPointPass">Indicates whether the last search point passed or failed.</param>
        void ExecuteScoreboard(string executionIdentifier, bool isLastSearchPointPass);

        /// <summary>
        /// 11. Called whenever a search is completed to determine if repetition is required or not.
        /// Default implementation returns false.
        /// </summary>
        /// <param name="searchResults">accumulated results from all completed searches.</param>
        /// <returns>A boolean value indicating if search needs to be repeated or not.</returns>>
        bool HasToRepeatSearch(List<SearchResultData> searchResults);

        /// <summary>
        /// 12. Called as last step of a single search.
        /// Allows abortion of any remaining search executions (configured through MultipleSearchStates parameter). The input provided to take this decision are the accumulated
        /// results for all searches executed from the list of states until the current completed search. Each search result object contains final data from each previous search,
        /// like final voltage results, final mask, limiting patterns, start voltages, search end limits, and pass/fail search result. A second input provided to optionally
        /// be used to take the decision is the functional test object to query the captured data from last search point plist execution.
        /// Default implementation returns true to allow next search state to be executed.
        /// </summary>
        /// <param name="searchResults">accumulated results from all already completed searches.</param>
        /// <param name="functionalTest">functional test object that provide methods to extract captured data from last search plist execution.</param>
        /// <returns>A boolean indicating whether the test method has to continue with next search state or not.</returns>
        bool HasToContinueToNextSearch(List<SearchResultData> searchResults, IFunctionalTest functionalTest);

        /// <summary>
        /// 13. Called after all searches are completed.
        /// Allows overriding of the returned port of the test instance (with the exception of the case where the plist execution didn't end any of the searches with a pass result
        /// at the corresponding last search point). One of the main usages of this extension would be to export the final voltage results for any required usage beyond the test
        /// instance boundaries. Each search result object contains final data from each previous search, like final voltage results, final mask, limiting patterns, start voltages,
        /// search end limits, and pass/fail search result.
        /// Default implementation returns 1 if at least on search passed or 0 if all searches failed to find any passing voltage value.
        /// </summary>>
        /// <param name="searchResults">accumulated results from all completed searches.</param>>
        /// <returns>A value indicating the exit port.</returns>>
        int PostProcessSearchResults(List<SearchResultData> searchResults);
    }
}
