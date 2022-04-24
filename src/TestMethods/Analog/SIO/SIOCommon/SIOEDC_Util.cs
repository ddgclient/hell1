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

namespace SIO
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    /// <summary>Contains utility functions for SIO EDC Test Classes.</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:Element should begin with upper-case letter", Justification = "NA.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:Elements should appear in the correct order", Justification = "More readable this way.")]
    public class SIOEDC_Util
    {
        /// <summary>
        /// Contains a single Field definition from a Format file.
        /// This includes at a minimum the field name, but can
        /// also include bit locations, expect values and which port
        /// to exit on a failure.
        /// </summary>
        public class SIOFormatField
        {
            /// <summary>Gets or sets Field Name.</summary>
            public string name { get; set; } = string.Empty;

            /// <summary>Gets or sets Port.</summary>
            public string port { get; set; } = string.Empty;

            /// <summary>Gets or sets LANE.</summary>
            public string lane { get; set; } = string.Empty;

            /// <summary>Gets or sets MSB Bit locations.</summary>
            public List<int> msbList { get; set; } = new List<int>();

            /// <summary>Gets or sets LSB Bit Locations.</summary>
            public List<int> lsbList { get; set; } = new List<int>();

            /// <summary>Gets or sets How to format the data (dec, bin, whatever).</summary>
            public string dataFormat { get; set; } = string.Empty;

            /// <summary>Gets or sets Expected value to match.</summary>
            public string expectExactVal { get; set; } = string.Empty;

            /// <summary>Gets or sets For dec type, low limit.</summary>
            public int expectLowLimit { get; set; } = -99;

            /// <summary>Gets or sets For dec type, upper limit.</summary>
            public int expectHighLimit { get; set; } = -99;

            /// <summary>Gets or sets Port to exit on failure.</summary>
            public int exitPort { get; set; } = -99;

            /// <summary>
            /// Initializes a new instance of the <see cref="SIOFormatField"/> class.
            /// </summary>
            /// <param name="field">Field Name.</param>
            public SIOFormatField(string field)
            {
                this.name = field;
            }
        }

        /// <summary>
        /// Represents a single line in a Format File.
        /// </summary>
        public class SIOFormat
        {
            /// <summary>Gets or sets default PORT.</summary>
            public string port { get; set; }

            /// <summary>Gets or sets default LANE.</summary>
            public string lane { get; set; }

            /// <summary>Gets or sets indivitual Field objects..</summary>
            public List<SIOFormatField> fields { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="SIOFormat"/> class.
            /// </summary>
            /// <param name="basePort">Default PORT to use if not specified in the field.</param>
            /// <param name="baseLane">Default LANE to use if not specified in the field.</param>
            public SIOFormat(string basePort, string baseLane)
            {
                this.port = basePort;
                this.lane = baseLane;
                this.fields = new List<SIOFormatField>();
            }
        }

        /// <summary>
        /// Represents the content of a Format File.
        /// </summary>
        public class SIOFormatFile
        {
            /// <summary>Gets or sets the Format objects in this file.</summary>
            public List<SIOFormat> data { get; set; } = new List<SIOFormat>();

            /// <summary>Gets or sets the fiels HEADER.</summary>
            public string header { get; set; }

            /// <summary>Gets or sets a value indicating whether this file contains valid data.</summary>
            public bool valid { get; set; } = false;
        }

        /// <summary>
        /// Contains a single line from a Sequence file.
        /// </summary>
        public class SIOSequence
        {
            /// <summary>Gets or sets PORT.</summary>
            public string port { get; set; }

            /// <summary>Gets or sets BUNDLE.</summary>
            public string bundle { get; set; }

            /// <summary>Gets or sets LANE.</summary>
            public string lane { get; set; }

            /// <summary>Gets or sets NUMOFBITS.</summary>
            public int numbits { get; set; }

            /// <summary>Gets or sets CAPTURENAME.</summary>
            public string signal { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="SIOSequence"/> class.
            /// </summary>
            /// <param name="port">PORT (element 2).</param>
            /// <param name="bundle">BUNDLE (element 3).</param>
            /// <param name="lane">LANE (element 4).</param>
            /// <param name="bits">NUMOFBITS (element 5).</param>
            /// <param name="signal">CAPTURENAME (element 6).</param>
            public SIOSequence(string port, string bundle, string lane, int bits, string signal)
            {
                this.port = port;
                this.bundle = bundle;
                this.lane = lane;
                this.numbits = bits;
                this.signal = signal;
            }
        }

        /// <summary>
        /// Container for the port/lane/signal data extracted from the capture data.
        /// </summary>
        public class HashedData
        {
            /// <summary>
            /// Adds data for the geven port/lane/field.
            /// </summary>
            /// <param name="port">PORT.</param>
            /// <param name="lane">LANE.</param>
            /// <param name="signal">FIELD.</param>
            /// <param name="data">Data to add.</param>
            public void Add(string port, string lane, string signal, string data)
            {
                this.contents[this.GetKey(port, lane, signal)] = data;
            }

            /// <summary>
            /// Gets the data for the given port/lane/field.
            /// </summary>
            /// <param name="port">PORT.</param>
            /// <param name="lane">LANE.</param>
            /// <param name="signal">FIELD.</param>
            /// <returns>data for this port/lane/field.</returns>
            public string Get(string port, string lane, string signal)
            {
                try
                {
                    return this.contents[this.GetKey(port, lane, signal)];
                }
                catch (KeyNotFoundException)
                {
                    Prime.Services.ConsoleService.PrintError($"No data found for Port=[{port}] Lane=[{lane}] Signal=[{signal}].");
                    throw;
                }
            }

            /// <summary>
            /// Gets the current number of saved data.
            /// </summary>
            /// <returns>Number of savd data elements.</returns>
            public int Count() => this.contents.Count;

            /// <summary>
            /// Initializes a new instance of the <see cref="HashedData"/> class.
            /// </summary>
            public HashedData()
            {
                this.contents = new Dictionary<string, string>();
            }

            private Dictionary<string, string> contents { get; set; }

            private string GetKey(string port, string lane, string signal)
            {
                return $"{port}|{lane}|{signal}";
            }
        }

        private bool debugMode { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SIOEDC_Util"/> class.
        /// </summary>
        /// <param name="debug">Debug mode flag.</param>
        public SIOEDC_Util(bool debug)
        {
            this.debugMode = debug;
        }

        // FIXME: duplicating this for now...making it static had a performance hit
        // and the other option to include siolib as object is messy

        /// <summary>
        /// Prints the message/error to the HDMT console window.
        /// </summary>
        /// <param name="msgType">MsgEnum setting for this message.  Prime doesn't
        /// support multiple debug modes, so only SIO_ERROR matters.  Everything else
        /// is treated the same.</param>
        /// <param name="message">Message to write to the consol.</param>
        public void MsgToConsole(MsgEnum msgType, string message)
        {
            // Prime only has Debug and Error messages so this isn't an exact translation...
            if (msgType == MsgEnum.SIO_ALWAYS)
            { // hijack printerror
                Prime.Services.ConsoleService.PrintError($"[SIO_ALWAYS] {message}", 0, " ", " ");
            }
            else if (msgType == MsgEnum.SIO_ERROR)
            {
                Prime.Services.ConsoleService.PrintError(message);
            }
            else if (this.debugMode)
            {
                Prime.Services.ConsoleService.PrintDebug($"[{msgType}] {message}");
            }
        }

        /// <summary>
        /// Loads the given format file into an object.
        /// </summary>
        /// <param name="remoteFileName">Format file to load.</param>
        /// <returns>SIOFormatFile fobject.</returns>
        public SIOFormatFile LoadFormatFile(string remoteFileName)
        {
            var formats = new SIOFormatFile();

            // create a local copy of the file.
            var localFileName = SIOLib.GetFile(remoteFileName);
            if (string.IsNullOrEmpty(localFileName))
            {
                return formats;  // GetFile should have already printed an error message, so just return an empty obj.
            }

            using (StreamReader sr = new StreamReader(localFileName))
            {
                string line;
                int lineNumber = 0;
                while ((line = sr.ReadLine()) != null)
                {
                    // Remove any comments and skip blank lines.
                    if (line.StartsWith("#"))
                    {
                        continue;
                    }

                    if (line.Contains("#"))
                    {
                        line = line.Split('#')[0];
                    }

                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    lineNumber++;

                    // first line is the header
                    if (lineNumber == 1)
                    {
                        formats.header = line;
                        continue;
                    }

                    // Each line is a comma separated list.
                    var data = line.Split(',');
                    if (data.Length < 3)
                    {
                        this.MsgToConsole(MsgEnum.SIO_ERROR, $"Invalid number of elements in Line=[{line}] at lineNum=[{lineNumber}] File=[{remoteFileName}], Expected at least 3, got [{data.Length}].");
                        return formats;
                    }

                    // MsgToConsole(MsgEnum.SIO_DEBUG, $"Read line [{line}]");

                    // First to items in the line are PORT and LANE.
                    var basePort = data[0];
                    var baseLane = data[1];
                    SIOFormat formatObj = new SIOFormat(basePort, baseLane);

                    // the remaining elements are field definitions.
                    for (var index = 2; index < data.Length; index++)
                    {
                        var field = data[index];

                        SIOFormatField formatFieldObj = new SIOFormatField(field);
                        formatFieldObj.port = basePort;
                        formatFieldObj.lane = baseLane;

                        // handle special field formatting
                        if (field == "-" || string.IsNullOrWhiteSpace(field))
                        {
                            formatObj.fields.Add(formatFieldObj);
                            continue;
                        }

                        if (field.Contains('!'))
                        {
                            var tmp = field.Split('!');
                            if (tmp.Length == 3)
                            {
                                formatFieldObj.port = tmp[0];
                                formatFieldObj.lane = tmp[1];
                                field = tmp[2];
                            } // --FIXME-- add an else with error catching (not in python code)
                        }

                        // most fields are : separated.
                        var fieldList = field.Split(':');

                        // element 0 is the name of the field.
                        var fieldName = fieldList[0];
                        formatFieldObj.name = fieldName;

                        if (fieldList.Length == 1)
                        {
                            continue;  // field only contains a name.
                        }

                        // fieldList[1] is a list of bit ranges separated by ;
                        //    ie.  "0-7;9-12;16-32"
                        var bitRanges = fieldList[1].Split(';');
                        foreach (var bitRange in bitRanges)
                        {
                            var bit_loc = bitRange.Split('-');
                            if (bit_loc.Length == 2)
                            {
                                try
                                {
                                    var lsbBit = int.Parse(bit_loc[0]);
                                    var msbBit = int.Parse(bit_loc[1]);

                                    formatFieldObj.lsbList.Add(lsbBit);
                                    formatFieldObj.msbList.Add(msbBit);
                                }
                                catch (FormatException)
                                {
                                    this.MsgToConsole(MsgEnum.SIO_ERROR, $"Invalid Line at lineNum=[{lineNumber}] File=[{remoteFileName}] Expecting Integers in bitrange got [{bitRange}]");
                                    return formats;
                                }
                            }

                            /*else - this isn't an error in python so skip checking.
                            {
                                this.MsgToConsole(MsgEnum.SIO_ERROR, $"Invalid Line at lineNum=[{lineNumber}] File=[{localFileName}] Expecting bitrange lsb-msb got [{bitRange}] from Field=[{field}]");
                                return formats;
                            }*/
                        }

                        // fieldList[2] is always a data format (bin, hex, etc...) if it exists.
                        if (fieldList.Length >= 3)
                        {
                            formatFieldObj.dataFormat = fieldList[2].ToLower();
                        }

                        if (fieldList.Length == 5)
                        {
                            // Format = Capture Name:bits to capture:decoding format:value to match:user defined exit port if captured data doesn't match value
                            formatFieldObj.expectExactVal = fieldList[3];
                            try
                            {
                                formatFieldObj.exitPort = int.Parse(fieldList[4]);
                            }
                            catch (FormatException)
                            {
                                this.MsgToConsole(MsgEnum.SIO_ERROR, $"Invalid Line at lineNum=[{lineNumber}] File=[{remoteFileName}] Expecting an integer for exit_port, got [{fieldList[4]}]");
                                return formats;
                            }
                        }
                        else if (fieldList.Length == 6)
                        {
                            // Format = Capture Name:bits to capture:decoding format:low limit:high limit:user defined exit port if captured data not within limits
                            if (formatFieldObj.dataFormat == "bin" || formatFieldObj.dataFormat == "hex")
                            {
                                this.MsgToConsole(MsgEnum.SIO_ERROR, $"Invalid Line at lineNum=[{lineNumber}] File=[{remoteFileName}] Data format cannot be bin or hex when upper/lower limits are specified.");
                                return formats;
                            }

                            try
                            {
                                formatFieldObj.expectLowLimit = int.Parse(fieldList[3]);
                                formatFieldObj.expectHighLimit = int.Parse(fieldList[4]);
                                formatFieldObj.exitPort = int.Parse(fieldList[5]);
                            }
                            catch (FormatException)
                            {
                                this.MsgToConsole(MsgEnum.SIO_ERROR, $"Invalid Line at lineNum=[{lineNumber}] File=[{remoteFileName}] Expecting an integer for lowlimit,highlimit and exit_port, got [{fieldList[3]},{fieldList[4]},{fieldList[5]}]");
                                return formats;
                            }
                        }
                        else if (fieldList.Length != 3)
                        {
                            // FIXME...python code doesn't have any errors here.
                            this.MsgToConsole(MsgEnum.SIO_ERROR, $"WARNING: Invalid Line at lineNum=[{lineNumber}] File=[{remoteFileName}] Expecting 3, 5, or 6 elements, got [{fieldList.Length}] in [{field}]");
                            /* return formats; */
                        }

                        this.MsgToConsole(MsgEnum.SIO_DEBUG, $"Adding Field [{formatFieldObj.port}/{formatFieldObj.lane}/{formatFieldObj.name}] to current Format");
                        formatObj.fields.Add(formatFieldObj);
                    } // end for (var index = 2; index < data.Length; index++)

                    this.MsgToConsole(MsgEnum.SIO_DEBUG, $"Saving format [{formatObj.port}/{formatObj.lane}]");
                    formats.data.Add(formatObj);
                } // end while ((line = sr.ReadLine()) != null)
            } // end using (StreamReader sr = new StreamReader(localFileName))

            formats.valid = true;
            return formats;
        }

        /// <summary>
        /// Loads a sequence file into a Dictionary, where the Keys are equal to the
        /// Sequence ID (1st element of each line) and the Values are list of the objects
        /// represeneting each line.
        /// </summary>
        /// <param name="remoteFileName">Sequence file to load.</param>
        /// <returns>All Sequences from the file.</returns>
        public Dictionary<string, List<SIOSequence>> LoadSequenceFile(string remoteFileName)
        {
            // FIXME ... should probably wrap the return value in a class
            this.MsgToConsole(MsgEnum.SIO_DEBUG, $"Starting LoadSequenceFile File=[{remoteFileName}] debug=[{this.debugMode}]");

            var sequence = new Dictionary<string, List<SIOSequence>>();

            // create a local copy of the file.
            var localFileName = SIOLib.GetFile(remoteFileName);
            if (string.IsNullOrEmpty(localFileName))
            {
                return sequence;  // GetFile should have already printed an error message, so just return an empty obj.
            }

            // file format KEY,PORT,BUNDLE,LANE,NUMOFBITS,CAPTURENAME
            using (StreamReader sr = new StreamReader(localFileName))
            {
                string line;
                int lineNumber = 0;
                while ((line = sr.ReadLine()) != null)
                {
                    // Remove any comments and skip blank lines.
                    if (line.StartsWith("#"))
                    {
                        continue;
                    }

                    if (line.Contains("#"))
                    {
                        line = line.Split('#')[0];
                    }

                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    lineNumber++;

                    // first line is the header
                    if (lineNumber == 1)
                    {
                        continue;
                    }

                    var data = line.Split(',');
                    if (data.Length != 6)
                    {
                        this.MsgToConsole(MsgEnum.SIO_ERROR, $"Invalid number of elements in Line=[{line}] at lineNum=[{lineNumber}] File=[{remoteFileName}], Expected 6, got [{data.Length}].");
                        return new Dictionary<string, List<SIOSequence>>();
                    }

                    // MsgToConsole(MsgEnum.SIO_DEBUG, $"Read line [{line}]");
                    string seqKey = data[0];
                    if (!sequence.ContainsKey(seqKey))
                    {
                        sequence[seqKey] = new List<SIOSequence>();
                    }

                    int bits;
                    try
                    {
                        bits = int.Parse(data[4]);
                    }
                    catch (FormatException)
                    {
                        this.MsgToConsole(MsgEnum.SIO_ERROR, $"Invalid Line=[{line}] at lineNum=[{lineNumber}] File=[{remoteFileName}] Expecting a number for Bits, not [{data[4]}]");
                        return new Dictionary<string, List<SIOSequence>>();
                    }

                    sequence[seqKey].Add(new SIOSequence(data[1], data[2], data[3], bits, data[5]));
                } // end while ((line = sr.ReadLine()) != null)
            } // end using (StreamReader sr = new StreamReader(localFileName))

            this.MsgToConsole(MsgEnum.SIO_DEBUG, $"Loaded [{sequence.Count}] Sequence(s).");

            return sequence;
        } // end LoadSequenceFile(string localFileName)

        /// <summary>
        /// Extracts data from a binary capture string into port/lane/signal based on a sequence file.
        /// </summary>
        /// <param name="sequenceList">List of data from a Sequence fil.</param>
        /// <param name="fullDatastring">binary capture data.</param>
        /// <returns>A HashedData object.</returns>
        public HashedData HashBitStream(List<SIOSequence> sequenceList, string fullDatastring)
        {
            int bitLoc = 0;
            HashedData dataHash = new HashedData();

            foreach (var seq in sequenceList)
            {
                if (fullDatastring.Length < (bitLoc + seq.numbits))
                {
                    this.MsgToConsole(MsgEnum.SIO_ERROR, $"Ran out of data at BitLocation=[{bitLoc}]+Length=[{seq.numbits}], RawData=({fullDatastring.Length})[{fullDatastring}]");
                    return new HashedData();
                }

                // FIXME - reverse data from how it is in the raw capture string...this matches EmbPython, but why?
                var raw_data = new string(fullDatastring.Substring(bitLoc, seq.numbits).ToCharArray().Reverse().ToArray());
                this.MsgToConsole(MsgEnum.SIO_DEBUG, $"Assigned {raw_data} to {seq.port}/{seq.lane}/{seq.signal}");
                dataHash.Add(seq.port, seq.lane, seq.signal, raw_data);
                bitLoc += seq.numbits;
            }

            if (fullDatastring.Length != bitLoc)
            {
                this.MsgToConsole(MsgEnum.SIO_ERROR, $"Size of Captured data [{fullDatastring.Length}] is not equal to size [{bitLoc}] defined in sequence file. Exit out of port 0");
                return new HashedData();
            }

            return dataHash;
        }

        /// <summary>
        /// Takes the data from a HashedData struct and uses a format file to convert and compare the
        /// data for each field and build a human readable output.
        /// </summary>
        /// <param name="formats">Format file to use to format the data.</param>
        /// <param name="dataHash">Hashed data values for each port/lane/signa.</param>
        /// <param name="regDef">Name of the register defintion parser or an empty string.</param>
        /// <param name="outputList">(output) Human readable text output, each element is one line.</param>
        /// <param name="exitPort">(output) Exit port based on data comparisons.</param>
        /// <returns>true on success.</returns>
        public bool GenerateOutput(SIOFormatFile formats, HashedData dataHash, string regDef, out List<string> outputList, out int exitPort)
        {
            outputList = new List<string>();
            exitPort = 1;
            StringBuilder sb = new StringBuilder();
            var regParser = RegDefFactory.GetParser(regDef, this);

            if (formats == null || !formats.valid)
            {
                this.MsgToConsole(MsgEnum.SIO_ERROR, $"GenerateOutput given invalid format file.");
                return false;
            }

            this.MsgToConsole(MsgEnum.SIO_DEBUG, $"GenerateOutput, FormatFile has {formats.data.Count} formats");
            foreach (var format in formats.data)
            {
                // MsgToConsole(MsgEnum.SIO_DEBUG, $"Examining Format=[{format.port}/{format.lane}]");
                sb.Append($",{format.port},{format.lane}");
                foreach (var field in format.fields)
                {
                    this.MsgToConsole(MsgEnum.SIO_DEBUG, $"Examining Format Field=[{field.port}/{field.lane}/{field.name}]");
                    if (field.name == "-" || string.IsNullOrWhiteSpace(field.name))
                    {
                        sb.Append(",");
                        continue;
                    }

                    if (regParser != null && regParser.Contains(field.port, field.lane, field.name))
                    {
                        var data = regParser.GetData(dataHash, field.port, field.lane, field.name);

                        if (field.expectLowLimit != -99 && field.expectHighLimit != -99 && field.exitPort != -99)
                        {
                            try
                            {
                                if (int.Parse(data) < field.expectLowLimit || int.Parse(data) > field.expectHighLimit)
                                {
                                    this.MsgToConsole(MsgEnum.SIO_INFO, $"Output Data \"{int.Parse(data)}\" doesn\'t match with Low Limit \"{field.expectLowLimit}\", High Limit \"{field.expectHighLimit}\" for PORT:\"{field.port}\", Lane:\"{field.lane}\", FieldName:\"{field.name}\". Exiting out of ExitPort \"{field.exitPort}\" ");
                                    exitPort = field.exitPort;
                                }
                            }
                            catch (Exception)
                            {
                                this.MsgToConsole(MsgEnum.SIO_INFO, $"cannot convert data [{data}] to signed dec. PORT:\"{field.port}\", Lane:\"{field.lane}\", FieldName:\"{field.name}\". Exiting out of ExitPort \"{field.exitPort}\" ");
                                exitPort = field.exitPort;
                            }
                        }
                        else if (!string.IsNullOrWhiteSpace(field.expectExactVal) && field.exitPort != -99)
                        {
                            if (data != field.expectExactVal)
                            {
                                this.MsgToConsole(MsgEnum.SIO_INFO, $"Output Data \"{data}\" doesn\'t match with compare data \"{field.expectExactVal}\" for PORT:\"{field.port}\", Lane:\"{field.lane}\", FieldName:\"{field.name}\". Exiting out of ExitPort \"{field.exitPort}\" ");
                                exitPort = field.exitPort;
                            }
                        }

                        sb.Append($",{data}");
                        continue;  // decoding was done by external regparser, no need for any more.
                    }

                    // No regparser for this field, use the raw data.
                    var raw_data = dataHash.Get(field.port, field.lane, field.name);
                    this.MsgToConsole(MsgEnum.SIO_DEBUG, $"Assigned {raw_data} from {field.port}/{field.lane}/{field.name}");

                    string subdata = string.Empty;
                    for (var i = 0; i < field.msbList.Count; i++)
                    {
                        // get_bit_field can handle msb<lsb now.
                        subdata += this.Get_bit_field(raw_data, field.lsbList[i], field.msbList[i]);
                    } // end for (var i = 0; i < field.msbList.Count(); i++)

                    raw_data = subdata != string.Empty ? subdata : raw_data;

                    int dataInt = -99;
                    string dataStr = string.Empty;
                    bool dataIsInt = false;

                    if (!SIOLib.IsBinaryRegex.IsMatch(raw_data))
                    {
                        this.MsgToConsole(MsgEnum.SIO_ERROR, $"GenerateOutput: Error Data=[{raw_data}] is not binary for Format Field [{field.port}, {field.lane}, {field.name}] in sequence data.");
                        return false;
                    }

                    if (!string.IsNullOrWhiteSpace(field.dataFormat))
                    {
                        // FIXME -- for now, make sure its <32 bits of data...otherwise need to update the conversion functions
                        if (raw_data.Length >= 32)
                        {
                            this.MsgToConsole(MsgEnum.SIO_ERROR, $"GenerateOutput: Error Data=[{raw_data}] is too many bits to convert for for Format Field [{field.port}, {field.lane}, {field.name}] in sequence data.");
                            return false;
                        }

                        if (field.dataFormat == "dec")
                        {
                            dataIsInt = true;
                            dataInt = Convert.ToInt32(raw_data, 2);
                        }
                        else if (field.dataFormat == "2cdec")
                        {
                            dataIsInt = true;
                            dataInt = this.Conv_2c(Convert.ToInt32(raw_data, 2), raw_data.Length); // conv_2c(int(data,2),data_len)
                        }
                        else if (field.dataFormat == "gray2dec")
                        {
                            dataIsInt = true;
                            dataInt = this.Gray2dec(raw_data); // gray2dec(data)
                        }
                        else if (field.dataFormat == "mcd14_gray2dec")
                        {
                            dataIsInt = true;
                            dataInt = this.Mcd14_gray2dec(raw_data); // mcd14_gray2dec(data)
                        }
                        else if (field.dataFormat == "sgray2dec")
                        {
                            dataIsInt = true;
                            dataInt = this.Sgray2dec(raw_data); // sgray2dec(data)
                        }
                        else if (field.dataFormat == "sdec")
                        {
                            dataIsInt = true;
                            dataInt = this.Conv_signeddec(raw_data); // conv_signeddec(data)
                        }
                        else if (field.dataFormat == "hex")
                        {
                            dataStr = Convert.ToInt32(raw_data, 2).ToString("X");
                        }
                        else if (field.dataFormat == "bin")
                        {
                            dataStr = raw_data;
                        }
                        else if (field.dataFormat == "gray2bin")
                        {
                            dataStr = this.Gray2bin(raw_data); // gray2bin(data)
                        }
                        else
                        {
                            this.MsgToConsole(MsgEnum.SIO_ERROR, $"Output data format [{field.dataFormat}] is not valid.");
                            return false;
                        }

                        if (dataIsInt)
                        {
                            if (field.expectLowLimit != -99 && field.expectHighLimit != -99 && field.exitPort != -99)
                            {
                                if (dataInt < field.expectLowLimit || dataInt > field.expectHighLimit)
                                {
                                    this.MsgToConsole(MsgEnum.SIO_INFO, $"Output Data \"{dataInt}\" doesn\'t match with Low Limit \"{field.expectLowLimit}\", High Limit \"{field.expectHighLimit}\" for PORT:\"{field.port}\", Lane:\"{field.lane}\", FieldName:\"{field.name}\". Exiting out of ExitPort \"{field.exitPort}\" ");
                                    exitPort = field.exitPort;
                                }
                            }
                            else if (!string.IsNullOrWhiteSpace(field.expectExactVal))
                            {
                                int compareVal = 0;
                                try
                                {
                                    compareVal = int.Parse(field.expectExactVal);
                                }
                                catch
                                {
                                    this.MsgToConsole(MsgEnum.SIO_ERROR, $"Error in format file. Value=[{field.expectExactVal}] to match must be integer for PORT:\"{field.port}\", Lane:\"{field.lane}\", FieldName:\"{field.name}\". Exiting out of ExitPort \"{field.exitPort}\" ");
                                    exitPort = field.exitPort;
                                    compareVal = -99; // set to default
                                }

                                if (compareVal != -99 && field.exitPort != -99)
                                {
                                    if (dataInt != compareVal)
                                    {
                                        this.MsgToConsole(MsgEnum.SIO_INFO, $"Output Data \"{dataInt}\" doesn\'t match with compare data \"{compareVal}\" for PORT:\"{field.port}\", Lane:\"{field.lane}\", FieldName:\"{field.name}\". Exiting out of ExitPort \"{field.exitPort}\" ");
                                        exitPort = field.exitPort;
                                    }
                                }
                            }
                        }
                        else if (!string.IsNullOrWhiteSpace(field.expectExactVal) && field.exitPort != -99)
                        {
                            if (dataStr != field.expectExactVal)
                            {
                                this.MsgToConsole(MsgEnum.SIO_INFO, $"Output Data \"{dataStr}\" doesn\'t match with compare data \"{field.expectExactVal}\" for PORT:\"{field.port}\", Lane:\"{field.lane}\", FieldName:\"{field.name}\". Exiting out of ExitPort \"{field.exitPort}\" ");
                                exitPort = field.exitPort;
                            }
                        }
                    } // end if (!string.IsNullOrWhiteSpace(field.dataFormat))
                    else
                    {
                        try
                        {
                            dataStr = Convert.ToUInt64(raw_data, 2).ToString(); // FIXME - support more than 64 bits
                        }
                        catch
                        {
                            this.MsgToConsole(MsgEnum.SIO_ERROR, $"Raw format only supports up to 64 bits.");
                            return false;
                        }
                    }

                    if (dataIsInt)
                    {
                        dataStr = dataInt.ToString();
                    }

                    sb.Append($",{dataStr}");
                } // end foreach (var field in format.fields)

                sb.Append('\n');
            } // end foreach (var format in formats.data)

            outputList = new List<string>(sb.ToString().Trim().Split('\n'));
            this.MsgToConsole(MsgEnum.SIO_DEBUG, $"GenerateOutput, Completed building output list.");
            return true;
        }

        /// <summary>
        /// Outputs HashedData based on a sequence file.
        /// </summary>
        /// <param name="sequenceList">Sequence file dat.</param>
        /// <param name="seqID">Sequence ID used as a header for the outpu.</param>
        /// <param name="dataHash">Hashed data to write ou.</param>
        /// <returns>True.</returns>
        public bool DisplaySequenceData(List<SIOSequence> sequenceList, string seqID, HashedData dataHash)
        {
            this.MsgToConsole(MsgEnum.SIO_DEBUG, $"Attempting to display sequence data.");
            StringBuilder outputSb = new StringBuilder(string.Empty, 5000);
            foreach (var seq in sequenceList)
            {
                var raw_data = dataHash.Get(seq.port, seq.lane, seq.signal);
                var hexData = Convert.ToInt32(raw_data, 2).ToString("X");
                outputSb.AppendFormat("{0,-15}{1,-15}{2,-15}{3,-50}{4} (0x{5})\n", seq.port, seq.lane, seq.numbits, seq.signal, raw_data, hexData);
            }

            var dataout = $"Sequence File Info for sequence id \"{seqID}\"\n";
            dataout += string.Format("{0,-15}{1,-15}{2,-15}{3,-50}{4}\n", "PORT", "LANE", "NUMOFBITS", "REGISTER", "DATA");
            dataout += outputSb.ToString();
            this.MsgToConsole(MsgEnum.SIO_INFO, $"**\n{dataout}**");
            return true;
        }

        /// <summary>
        /// Writes the output created by GenerateOutput.
        /// </summary>
        /// <param name="header">Header info from format fil.</param>
        /// <param name="outputList">output generated by GenerateOutpu.</param>
        /// <returns>true on success.</returns>
        public bool DisplayOutput(string header, List<string> outputList)
        {
            this.MsgToConsole(MsgEnum.SIO_DEBUG, $"Attempting to display output list data.");

            // outputList (output from GenerateOutput) looks like...
            // ,pcie,-,1,,,,,,,,,,,,,,,,,,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1
            // ,pcie,L0,,0,0,1,0,1,1,1,0,0,0,1,2,3,8,8,9,9,,,,,,,,,,,,,,,,,,,,,,,,,,,
            // ,pcie,L1,,0,0,1,0,1,1,1,0,0,0,1,2,3,8,8,9,9,,,,,,,,,,,,,,,,,,,,,,,,,,,
            // ,pcie,L2,,0,0,1,0,1,1,1,0,0,0,1,2,3,8,8,9,9,,,,,,,,,,,,,,,,,,,,,,,,,,,
            // ,pcie,L3,,0,0,1,0,1,1,1,0,0,0,1,2,3,8,8,9,9,,,,,,,,,,,,,,,,,,,,,,,,,,,
            var width_list = new List<int>();

            var fields = header.Split(',');

            string formated_header = string.Empty;
            foreach (var item in fields)
            {
                if (string.IsNullOrWhiteSpace(item))
                {
                    continue;
                }

                var length = item.Length;
                if (length < 10)
                {
                    width_list.Add(10);
                }
                else
                {
                    width_list.Add(length + 2);
                }

                formated_header += string.Format("{0}", item.PadRight(width_list[width_list.Count - 1]));
            }

            string seperator = new string('-', formated_header.Length);

            if (outputList.Count > 0)
            {
                this.MsgToConsole(MsgEnum.SIO_INFO, seperator);
                this.MsgToConsole(MsgEnum.SIO_INFO, formated_header);
                this.MsgToConsole(MsgEnum.SIO_INFO, seperator);
            }

            // FIXME -- should really use a StringBuilder here, but its debug only
            // so performance isn't critical.
            foreach (var outputLine in outputList)
            {
                string formated_data = string.Empty;
                fields = outputLine.Split(',');

                // if (fields.Length != width_list.Count)
                // {
                //    MsgToConsole(MsgEnum.SIO_ERROR, $"ERROR: Header has {width_list.Count} items, data has {fields.Length} items, cannot display correctly");
                //    return false;
                // }
                for (int i = 1; i < fields.Length; i++)
                {
                    if (width_list.Count >= i)
                    {
                        formated_data += string.Format("{0}", fields[i].PadRight(width_list[i - 1]));
                    }
                    else
                    {
                        formated_data += string.Format("{0} ", fields[i]);
                    }
                }

                this.MsgToConsole(MsgEnum.SIO_INFO, formated_data);
            }

            return true;
        }

        // -----------------------------------------------------------------------------------------------------
        // helper methods
        // -----------------------------------------------------------------------------------------------------

        /// <summary>
        /// Extracts a substring from a string based on lsb/msb indexes.
        /// </summary>
        /// <param name="data">Base string to extract data from.  MSB first.</param>
        /// <param name="start">Index of the first bit to extract. (index 0 is lsb.</param>
        /// <param name="end">Index of the last bit to extract. (index 0 is lsb.</param>
        /// <returns>substring.</returns>
        public string Get_bit_field(string data, int start, int end)
        {
            // data is msb first and so is the return.
            // Begin at the end index and move towards the start index.
            // if start > end the return string will be reversed.
            var index = (data.Length - 1) - end;    // index in the base "data" string
            var direction = (end > start) ? 1 : -1; // direction to move through the base "data" string
            var count = Math.Abs(end - start) + 1;  // total number of bits in field
            char[] subData = new char[count];
            for (var i = 0; i < count; i++)
            {
                subData[i] = data[index];
                index += direction;
            }

            return new string(subData);

            // l         9876543210
            // string = "abcdefghij"
            // start = 1; end = 3;  --> final = ghi

            // below is a direct translation of the python implementation.
            // its really slow in c# and only works if start<end.
            // var dataReverse = new string(data.ToCharArray().Reverse().ToArray());
            // var subData = dataReverse.Substring(start, end - start + 1);
            // return new string(subData.ToCharArray().Reverse().ToArray());
        }

        /// <summary>
        /// I have no idea what this does.
        /// </summary>
        /// <param name="val">Integer Value.</param>
        /// <param name="bits">Number of bits to convert.</param>
        /// <returns>conv_2c conversion.</returns>
        public int Conv_2c(int val, int bits)
        {
            // val == decimal value; bits == binary length of val
            // e.g bin = '1010', conv_2c(int(bin,2),len(bin))
            val &= (1 << bits) - 1;  // mask bits above the limit
            if ((val & (1 << (bits - 1))) != 0)
            {
                val -= 1 << bits;
            }

            this.MsgToConsole(MsgEnum.SIO_DEBUG, $"SIOEDC_Util::Conv_2c({val}, {bits}) -> {val}");
            return val;
        }

        /// <summary>
        /// Convert GrayCode to Decimal.
        /// </summary>
        /// <param name="grayBin">Binary Graycode number.</param>
        /// <returns>Decimal number.</returns>
        public int Gray2dec(string grayBin)
        {
            string sbin = this.Gray2bin(grayBin);
            var retval = Convert.ToInt32(sbin, 2);
            this.MsgToConsole(MsgEnum.SIO_DEBUG, $"SIOEDC_Util::Gray2dec({grayBin}) -> {retval}");
            return retval;
        }

        /// <summary>
        /// Convert GrayCode to Binary.
        /// </summary>
        /// <param name="grayBin">Graycode binary.</param>
        /// <returns>Normal binary.</returns>
        public string Gray2bin(string grayBin)
        {
            string binary = grayBin.Substring(0, 1);
            int i = 0;
            while (grayBin.Length > (i + 1))
            {
                var xor = int.Parse(binary.Substring(i, 1)) ^ int.Parse(grayBin.Substring(i + 1, 1));
                binary += xor.ToString();
                i += 1;
            }

            this.MsgToConsole(MsgEnum.SIO_DEBUG, $"SIOEDC_Util::Gray2bin({grayBin}) -> {binary}");
            return binary;
        }

        /// <summary>
        /// Conversion function.
        /// </summary>
        /// <param name="sBinary">Binary String.</param>
        /// <returns>converted number.</returns>
        public int Mcd14_gray2dec(string sBinary)
        {
            // Decode phase interpolator code and return as decimal
            // Encoding is
            // [7:6] 2 - bit quadrant in grey code
            // [5:1] PI coefficient MSBs in grey code
            // [  0] PI coefficient LSB
            if (sBinary.Length != 8)
            {
                this.MsgToConsole(MsgEnum.SIO_ERROR, $"mcd14_gray2dec requires an 8 bit binary number, got [{sBinary}]");
                return -99;
            }

            int nLsb = int.Parse(sBinary.Substring(sBinary.Length - 1, 1));
            string[] lDecoder =
                {
                    "0000000",  // 0
                    "0000001",  // 1
                    "0000011",  // 2
                    "0000010",  // 3
                    "0000110",  // 4
                    "0000111",  // 5
                    "0000101", "0000100", "0001100", "0001101", "0001111", "0001110", "0001010", "0001011", "0001001", "0001000", "0011000", "0011001", "0011011", "0011010",
                    "0011110", "0011111", "0011101", "0011100", "0010100", "0010101", "0010111", "0010110", "0010010", "0010011", "0010001", "0010000", "0110000", "0110001",
                    "0110011", "0110010", "0110110", "0110111", "0110101", "0110100", "0111100", "0111101", "0111111", "0111110", "0111010", "0111011", "0111001", "0111000",
                    "0101000", "0101001", "0101011", "0101010", "0101110", "0101111", "0101101", "0101100", "0100100", "0100101", "0100111", "0100110", "0100010", "0100011",
                    "0100001", "0100000", "1100000", "1100001", "1100011", "1100010", "1100110", "1100111", "1100101", "1100100", "1101100", "1101101", "1101111", "1101110",
                    "1101010", "1101011", "1101001", "1101000", "1111000", "1111001", "1111011", "1111010", "1111110", "1111111", "1111101", "1111100", "1110100", "1110101",
                    "1110111", "1110110", "1110010", "1110011", "1110001", "1110000", "1010000", "1010001", "1010011", "1010010", "1010110", "1010111", "1010101", "1010100",
                    "1011100", "1011101", "1011111", "1011110", "1011010", "1011011", "1011001", "1011000", "1001000", "1001001", "1001011", "1001010", "1001110", "1001111",
                    "1001101", "1001100", "1000100", "1000101", "1000111", "1000110", "1000010", "1000011", "1000001", "1000000",
                };
            string sEncoded = sBinary.Substring(0, 7);
            int nDecimal_Msbs = Array.IndexOf(lDecoder, sEncoded);  // should return -1 on failure
            if (nDecimal_Msbs < 0)
            {
                this.MsgToConsole(MsgEnum.SIO_ERROR, $"mcd14_gray2dec failed to find [{sEncoded}] in decoder.");
                return -99;
            }

            int nDecimal = (nDecimal_Msbs << 1) + nLsb;
            this.MsgToConsole(MsgEnum.SIO_DEBUG, $"SIOEDC_Util::Mcd14_gray2dec({sBinary}) -> {nDecimal}");
            return nDecimal;
        }

        /// <summary>
        /// Conversion function.
        /// </summary>
        /// <param name="graybin">Binary string.</param>
        /// <returns>Converted number.</returns>
        public int Sgray2dec(string graybin)
        {
            if (graybin.Length < 2)
            {
                this.MsgToConsole(MsgEnum.SIO_ERROR, $"sgray2dec: Cannot convert binary [{graybin}] to signed dec. Length of binary should be at least 2");
                return -99;
            }

            string sign = graybin.Substring(0, 1);
            string sbin = this.Gray2bin(graybin.Substring(1));
            int dec = Convert.ToInt32(sbin, 2);
            if (sign == "1")
            {
                return (-1) * dec;
            }

            this.MsgToConsole(MsgEnum.SIO_DEBUG, $"SIOEDC_Util::Sgray2dec({graybin}) -> {dec}");
            return dec;
        }

        /// <summary>
        /// Conversion function.
        /// </summary>
        /// <param name="bin">Binary string.</param>
        /// <returns>Converted number.</returns>
        public int Conv_signeddec(string bin)
        {
            // convert binary to signed integer
            // e.g 10001 = -1 , 00001 = +1
            if (bin.Length < 2)
            {
                this.MsgToConsole(MsgEnum.SIO_ERROR, $"conv_signeddec: Cannot convert binary [{bin}] to signed dec. Length of binary should be at least 2");
                return -99;
            }

            int val = Convert.ToInt32(bin.Substring(1), 2);
            string sign = bin.Substring(0, 1);
            if (sign == "1")
            {
                val *= -1;
            }

            this.MsgToConsole(MsgEnum.SIO_DEBUG, $"SIOEDC_Util::Conv_signeddec({bin}) -> {val}");
            return val;
        }
    }
}
