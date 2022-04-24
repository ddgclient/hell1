// ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
// INTEL CONFIDENTIAL
// Copyright (2019) (2020) Intel Corporation
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

namespace PrimeValTool
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Creates html script for each in detail report of each Tp/Plan and for all Tps/Plans run on tos.
    /// </summary>
    public class GenerateHtmlReports
    {
        /// <summary>
        /// Gets or sets name.
        /// </summary>
        public string TplName;

        /// <summary>
        /// Gets or sets number of total loaded instances for report.
        /// </summary>
        public int TotalInstances;

        /// <summary>
        /// Stores number of instances executed on tester.
        /// </summary>
        public int ActualExecutedTests;

        /// <summary>
        /// Stores number of missing tests.
        /// </summary>
        public int MissingTests;

        /// <summary>
        /// Stores number of test with name issues.
        /// </summary>
        public int NotExpectedPort;

        /// <summary>
        /// Stores ituff fail info.
        /// </summary>
        public string Itufffails;

        /// <summary>
        /// Stores html file path of each individual TP/Plan report.
        /// </summary>
        public string Reportfile;

        /// <summary>
        /// Stores all pass instances names.
        /// </summary>
        public int PassInstances;

        /// <summary>
        /// Stores all failing instance names.
        /// </summary>
        public int FailInstances;

        /// <summary>
        /// Total time for loading, running and unloading TP.
        /// </summary>
        public string TotalRuntime;

        /// <summary>
        /// Total Instances bypassed
        /// </summary>
        public int BypassInstances;

        /// <summary>
        /// Generates report for each TP/Plan.
        /// </summary>
        /// <param name="tplName">Name of individual TP/Plan.</param>
        /// <param name="tosLogPath">Path to log the html file.</param>
        public static void GenerateTpBasedReport(string tplName, string tosLogPath, ref List<TestProgramResults> finalTpResultsList)
        {
            var i = 1;
            bool instRepo = false;
            DataTable table = new DataTable();

            table.Columns.Add("Index", typeof(int));
            table.Columns.Add("Module Name", typeof(string));
            table.Columns.Add("Instance Name", typeof(string));
            table.Columns.Add("TesterPort", typeof(string));
            table.Columns.Add("Expected port", typeof(string));
            table.Columns.Add("Passfailstatus", typeof(string));
            table.Columns.Add("TestInstanceStatus", typeof(string));

            foreach (var results in finalTpResultsList)
            {
                if (i <= finalTpResultsList.Count)
                {
                    table.Rows.Add(i, results.TpModuleName, results.TestInstanceName, string.Join("|", results.TestInstancePort), string.Join("|", results.ExpectedPort), results.PassFailStatus, results.TestInstanceStatus);
                }

                i++;
            }

            var htmlBody = ExportDataTableToHtml(table, instRepo);
            var pathForHtml = Path.Combine(tosLogPath, "Results.HTML");
            File.WriteAllText(pathForHtml, htmlBody);
        }

        /// <summary>
        /// Generates report for all TPs/Plans ran on tester.
        /// </summary>
        public static void GenerateLoadedTpsReport(string mainLogsFolderPath)
        {
            var i = 1;
            var tpRepo = true;
            var table = new DataTable();

            table.Columns.Add("Index", typeof(int));
            table.Columns.Add("Test plan Name", typeof(string));
            table.Columns.Add("Total Instances", typeof(int));
            table.Columns.Add("Instances Executed", typeof(int));
            table.Columns.Add("Missing Tests", typeof(int));
            table.Columns.Add("Bypass Instances", typeof(int));
            table.Columns.Add("Passing Tests", typeof(int));
            table.Columns.Add("Failing Tests", typeof(int));
            table.Columns.Add("ITUFF", typeof(string));
            table.Columns.Add("TestUnittime", typeof(TimeSpan));
            table.Columns.Add("Summary Report", typeof(string));

            foreach (var results in PValMain.Tplreportlist)
            {
                if (i <= PValMain.Tplreportlist.Count)
                {
                    table.Rows.Add(i, results.TplName, results.TotalInstances, results.ActualExecutedTests, results.MissingTests, results.BypassInstances, results.PassInstances, results.FailInstances + results.NotExpectedPort, results.Itufffails, results.TotalRuntime, results.Reportfile);
                }

                i++;
            }

            var htmlBody = ExportDataTableToHtml(table, tpRepo);
            tpRepo = false;
            var pathForHtml = Path.Combine(mainLogsFolderPath, PValMain.TosName, "Resultsummary.HTML");
            File.WriteAllText(pathForHtml, htmlBody);
        }

        /// <summary>
        /// Generate html format script for all the reports.
        /// </summary>
        /// <param name="tableData">datatable generated in above functions.</param>
        /// <param name="isInstTableOrTplTable">Indicates whether to generate report for each plan or for all plans.</param>
        /// <returns>Return hdmt text.</returns>
        private static string ExportDataTableToHtml(DataTable tableData, bool isInstTableOrTplTable)
        {
            var strHtmlBuilder = new StringBuilder();

            strHtmlBuilder.Append("<html >\n");
            strHtmlBuilder.Append("<head>\n");
            strHtmlBuilder.Append("</head>\n");
            strHtmlBuilder.Append("<body>\n");
            strHtmlBuilder.Append("<hr size = \"3\" noshade = \"\" >\n");
            strHtmlBuilder.AppendFormat("<h2>pVal Results Summary Date=[{0}] Time=[{1}], TOS=[{2}]</h2>\n", DateTime.Now.ToString("MM-dd-yyyy"), DateTime.Now.ToString("HH:mm:ss.fff"), PValMain.TosName);
            strHtmlBuilder.Append("<hr size = \"3\" noshade = \"\" >\n");
            strHtmlBuilder.Append("<table border='1px' cellpadding='2' cellspacing='2' bgcolor='white' style='font-family:Garamond; font-size:medium'>\n");

            strHtmlBuilder.Append("<tr >\n");
            foreach (DataColumn myColumn in tableData.Columns)
            {
                strHtmlBuilder.Append("<td bgcolor = 'Blue' cellpadding='3' cellspacing='3' style='font-family:Verdana;color:White'><b>\n");
                strHtmlBuilder.Append(myColumn.ColumnName);
                strHtmlBuilder.Append("</b></td>\n");
            }

            strHtmlBuilder.Append("</tr>\n");
            for (int i = 0; i < tableData.Rows.Count; i++)
            {
                strHtmlBuilder.Append(i % 2 == 0 ? "<tr >\n" : "<tr bgcolor = 'WHITESMOKE'>\n");
                foreach (DataColumn myColumn in tableData.Columns)
                {
                    strHtmlBuilder.Append("<td");
                    if (!isInstTableOrTplTable)
                    {
                        switch (tableData.Rows[i]["Passfailstatus"].ToString())
                        {
                            case "FAIL":
                            case "Not Executed":
                            case "No Expected Port":
                                if (myColumn.ColumnName == "Passfailstatus")
                                {
                                    strHtmlBuilder.Append(" bgcolor = 'Red' style='font-family:Verdana;color:White; font-weight: bold;'>");
                                    break;
                                }

                                strHtmlBuilder.Append(" bgcolor = 'ALICEBLUE' style='font-weight: bold;' >"); // style=\"border: 1px solid red;\">");
                                break;
                            default:
                                strHtmlBuilder.Append(">");
                                break;
                        }
                    }
                    else if (isInstTableOrTplTable)
                    {
                        switch (myColumn.ColumnName)
                        {
                            case "ITUFF" when tableData.Rows[i]["ITUFF"].ToString() == "Do not match":
                            case "Failing Tests" when int.Parse(tableData.Rows[i]["Failing Tests"].ToString()) > 0:
                            case "Missing Tests" when int.Parse(tableData.Rows[i]["Missing Tests"].ToString()) > 0:
                                strHtmlBuilder.Append(" bgcolor = 'Red' style='font-family:Verdana;color:White'>");
                                break;
                            default:
                                strHtmlBuilder.Append(">");
                                break;
                        }
                    }

                    strHtmlBuilder.Append(tableData.Rows[i][myColumn.ColumnName].ToString());
                    strHtmlBuilder.Append("</td>\n");
                }

                strHtmlBuilder.Append("</tr>\n");
            }

            strHtmlBuilder.Append("</table>\n");
            if (isInstTableOrTplTable)
            {
                strHtmlBuilder.Append("<hr size = \"5\" noshade = \"\" >\n");
                strHtmlBuilder.Append("<hr size = \"3\" noshade = \"\" >\n");
                foreach (var entry in PValMain.FailInstanceDict)
                {
                    strHtmlBuilder.AppendFormat("<b><u><h2 style=\"text-align:left;font-size:20\">{0} Results Summary</h2></u></b>\n", entry.Key);
                    strHtmlBuilder.AppendFormat("<b><u><h2 style=\"text-align:left;font-size:16\">{0} Failing tests</h2></u></b>\n", entry.Key);
                    foreach (var item in entry.Value)
                    {
                        strHtmlBuilder.AppendFormat("<i><h2 style=\"text-align:left;font-size:14;font-family:'Antic Slab',Rockwell,serif;font-weight: normal;line-height:10px\">{0}</h2><i>\n", item);
                    }

                    strHtmlBuilder.Append("<hr size = \"3\" noshade = \"\" >\n");
                    foreach (KeyValuePair<string, List<string>> entry1 in PValMain.MissingInstanceDict)
                    {
                        if (entry1.Key == entry.Key)
                        {
                            strHtmlBuilder.AppendFormat(
                                "<b><u><h2 style=\"text-align:left;font-size:16\">{0} Missing tests</h2></u></b>\n",
                                entry.Key);
                            foreach (var item in entry1.Value)
                            {
                                strHtmlBuilder.AppendFormat(
                                    "<i><h2 style=\"text-align:left;font-size:14;font-family:'Antic Slab',Rockwell,serif;font-weight: normal;line-height:10px\">{0}</h2><i>\n",
                                    item);
                            }

                            strHtmlBuilder.Append("<hr size = \"3\" noshade = \"\" >\n");
                        }
                    }

                    foreach (KeyValuePair<string, List<string>> entry1 in PValMain.NotExpectedPort)
                    {
                        if (entry1.Key == entry.Key)
                        {
                            strHtmlBuilder.AppendFormat(
                                "<b><u><h2 style=\"text-align:left;font-size:16\">{0} Not Expected port in test name</h2></u></b>\n",
                                entry.Key);
                            foreach (var item in entry1.Value)
                            {
                                strHtmlBuilder.AppendFormat(
                                    "<i><h2 style=\"text-align:left;font-size:14;font-family:'Antic Slab',Rockwell,serif;font-weight: normal;line-height:10px\">{0}</h2><i>\n",
                                    item);
                            }

                            strHtmlBuilder.Append("<hr size = \"3\" noshade = \"\" >\n");
                        }
                    }
                }

                strHtmlBuilder.Append("<hr size = \"4\" noshade = \"\" >\n");
                strHtmlBuilder.Append("<hr size = \"4\" noshade = \"\" >\n");
            }

            strHtmlBuilder.Append("</body>\n");
            strHtmlBuilder.Append("</html>\n");

            var htmlText = strHtmlBuilder.ToString();

            return htmlText;
        }
    }
}
