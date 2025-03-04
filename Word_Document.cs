using Microsoft.Office.Interop.Word;
using Microsoft.Office.Interop;
using NodeSetDoctor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace XmlScan
{
    internal class Word_Document
    {
        private static string[] astrTitlesLine1 = {    "Attribute",
                                                       "Value" };

        private static string[] astrTitlesLine2 = { "BrowseName" };
        private static string[] astrTitlesLine3 = { "IsAbstract" };
        private static string[] astrTitlesLine4 = { "References",
                                                    "Node Class",
                                                    "BrowseName",
                                                    "Data Type",
                                                    "Type Definition",
                                                    "Modelling Rule"  };
        private static string[] astrTitlesLine5 = { "SubType of" };
        private static string[] astrTitlesLast2 = { "Conformance Units" };
        private static string[] astrTitlesLast1 = { "Base info " };


        private static object missing = System.Type.Missing;
        private static void GotoEndAndDeselect()
        {
            object storyUnit = WdUnits.wdStory;
            object lineUnit = WdUnits.wdLine;
            object cnt = (int)1;
            object extend = (bool)false;
            ThisAddIn.g_WordApp.Selection.EndKey(ref storyUnit, ref missing);
        }


        public static void GenDocumentation(Model m)
        {
            if (m.listOutputObjectDetails != null)
            {
                // Get the application object
                Microsoft.Office.Interop.Word.Application wordApp = ThisAddIn.g_WordApp;

                // Get the current document
                Document currentDoc = wordApp.ActiveDocument;

                // Remember the current location so we can return to it.
                Range comRangeAtStart = wordApp.Selection.Range;

                Range comRangeCurrent = wordApp.Selection.Range;

                int iItem = 1;
                int cTotalItems = m.listOutputObjectDetails.Count;
                foreach (UAObjectType ua in m.listOutputObjectDetails)
                {
                    // if (iItem == 6 || iItem == 7)
                    {
                        // New Page
                        comRangeCurrent.InsertBreak(WdBreakType.wdPageBreak);
                        comRangeCurrent.Collapse(WdCollapseDirection.wdCollapseEnd);

                        // Optional -- Random title with page number.
                        comRangeCurrent.Text = $"Item: {iItem}/{cTotalItems}\r\n";
                        comRangeCurrent.Collapse(WdCollapseDirection.wdCollapseEnd);

                        // Table 1 -- First 3 rows (3x2)
                            // Debug string with info from table 1.
                            Utils.Debug_WriteLine(Model.bDumpDocDetails, $"Attribute:Value");
                            string strIsAbstract = (ua.bIsAbstract) ? "True" : "False";
                            Utils.Debug_WriteLine(Model.bDumpDocDetails, $"Browse Name:{ua.strBrowseName}");
                            Utils.Debug_WriteLine(Model.bDumpDocDetails, $"IsAbstract:{strIsAbstract}");

                        comRangeCurrent = Ouput_Table_1(currentDoc, comRangeCurrent, ua, out float flTotalWidth);

                        // Table 2 -- Row 4 (title row)
                            Utils.Debug_WriteLine(Model.bDumpDocDetails, $"{astrTitlesLine4[0],-12}\t{astrTitlesLine4[1],-16}\t{astrTitlesLine4[2],-32}\t{astrTitlesLine4[3],-36}\t{astrTitlesLine4[4],-40}\t{astrTitlesLine4[5],-12}");

                        comRangeCurrent = Output_Table_2(currentDoc, comRangeCurrent, flTotalWidth);

                        // Table 3 -- Row 5 (information row)
                            Utils.Debug_WriteLine(Model.bDumpDocDetails, $"Sub Type:{ua.strSubType}");
                        comRangeCurrent = Output_Table_3(currentDoc, comRangeCurrent, ua, flTotalWidth);

                        // Table 4 -- Rows 6 to end (title row)
                            if (ua.references != null)
                            {
                                foreach (UAReference uar in ua.references)
                                {
                                    if (uar != null)
                                    {
                                        if (uar.strReferenceType != "HasSubtype")
                                            Utils.Debug_WriteLine(Model.bDumpDocDetails, $"{uar.strReferenceType,-12}\t{uar.strNodeClass,-16}\t{uar.strBrowseName,-32}\t{uar.strDataType,-36}\t{uar.strTypeDefinition,-40}\t{uar.strModellingRule,-12}");
                                    }
                                }
                            }
                        comRangeCurrent = Output_Table_4(currentDoc, comRangeCurrent, ua, flTotalWidth);


                        Utils.Debug_WriteLine(Model.bDumpDocDetails, $"Category:{ua.strCategory}");


                        // And now, we leave room for the documentation that someone is going to write about all of the items that are referenced above
                        if (ua.references != null)
                        {
                            comRangeCurrent.Collapse(WdCollapseDirection.wdCollapseEnd);
                            comRangeCurrent.InsertParagraphAfter();
                            comRangeCurrent.InsertParagraphAfter();
                            foreach (UAReference uar in ua.references)
                            {
                                if (uar != null)
                                {
                                    if (uar.strReferenceType != "HasSubtype")
                                    {
                                            Utils.Debug_WriteLine(Model.bDumpDocDetails, $"{uar.strBrowseName,-32}");

                                        comRangeCurrent.Collapse(WdCollapseDirection.wdCollapseEnd);
                                        comRangeCurrent.Text = $"{uar.strBrowseName} ...field details...";
                                        comRangeCurrent.InsertParagraphAfter();
                                        comRangeCurrent.InsertParagraphAfter();
                                    }
                                }
                            }
                        }

                        // Select the range to position the cursor.
                        comRangeCurrent.Collapse(WdCollapseDirection.wdCollapseEnd);
                        comRangeCurrent.MoveStart(WdUnits.wdCharacter, 1); // Move start 1 character beyond.
                    }

                    iItem++;

                } // foreach (UAObjectType ua in listOutputObjectDetails)

                RemoveSpaceBetweenTables(currentDoc);
            } // if (listOutputObjectDetails != null)
        }

        private static void RemoveSpaceBetweenTables(Document currentDoc)
        {
            int cTables = currentDoc.Tables.Count;

            int cEndTable = 1; 
            // Start at the end of the list and move backwards
            for (int iTable = cTables; iTable > cEndTable; iTable--)
            {
                Table comTableLast = currentDoc.Tables[iTable];
                Table comTablePrev = currentDoc.Tables[iTable-1];
                Range comRangeLast = comTableLast.Range;
                Range comRangePrev = comTablePrev.Range;
                int iItemLast = comRangeLast.Start;
                int iItemPrev = comRangePrev.End;

                int cDistance = iItemLast - iItemPrev;
                Utils.Debug_WriteLine(true, $"RemoveSpaceBetweenTables: Between {iTable-1} and {iTable} there is {cDistance} items.");

                if (cDistance == 1)
                {
                    Range comDeleteMe = currentDoc.Range(iItemPrev, iItemLast);
                    comDeleteMe.Delete();
                    Marshal.ReleaseComObject(comDeleteMe);
                }
                Marshal.ReleaseComObject(comRangePrev);
                Marshal.ReleaseComObject(comRangeLast);
                Marshal.ReleaseComObject(comTablePrev);
                Marshal.ReleaseComObject(comTableLast);

                //comRange.Collapse(WdCollapseDirection.wdCollapseEnd);
                //comRange.MoveStart(WdUnits.wdCharacter, 1); // Move start 1 character beyond the table end.
                //comRange.InsertParagraphAfter();
                //comRange.Collapse(WdCollapseDirection.wdCollapseEnd);
            }
        }


        private static Range Ouput_Table_1(Document currentDoc, Range comRangeCurrent, UAObjectType ua, out float flTotalWidth)
        {
            Table comWordTable = currentDoc.Tables.Add(comRangeCurrent, 3, 2);
            float flCol1Width1, flCol1Width2;

            // Format the table by setting borders, etc.
            comWordTable.Borders.Enable = 1; // Enable borders for readability

            flCol1Width1 = comWordTable.Columns[1].Width;
            flCol1Width2 = comWordTable.Columns[2].Width;
            flTotalWidth = flCol1Width1 + flCol1Width2;
            flCol1Width1 = flTotalWidth * 0.1333F;
            flCol1Width2 = flTotalWidth - flCol1Width1;

            try
            {
                comWordTable.Columns[1].SetWidth(flCol1Width1, WdRulerStyle.wdAdjustNone);
                comWordTable.Columns[2].SetWidth(flCol1Width2, WdRulerStyle.wdAdjustNone);
            }
            catch (Exception ex)
            {
                string str1 = ex.ToString();
            }


            comWordTable.Cell(1, 1).Range.Text = "Attribute";
            comWordTable.Cell(1, 1).Range.Bold = 1;
            comWordTable.Cell(1, 2).Range.Text = "Value";
            comWordTable.Cell(1, 2).Range.Bold = 1;
            comWordTable.Cell(2, 1).Range.Text = "BrowseName";
            comWordTable.Cell(2, 2).Range.Text = ua.strBrowseName;
            comWordTable.Cell(3, 1).Range.Text = "IsAbstract";
            comWordTable.Cell(3, 2).Range.Text = (ua.bIsAbstract) ? "True" : "False";

            comRangeCurrent = comWordTable.Range;
            comRangeCurrent.Collapse(WdCollapseDirection.wdCollapseEnd);
            comRangeCurrent.MoveStart(WdUnits.wdCharacter, 1); // Move start 1 character beyond the table end.
            comRangeCurrent.InsertParagraphAfter();
            comRangeCurrent.Collapse(WdCollapseDirection.wdCollapseEnd);

            Marshal.ReleaseComObject(comWordTable);
            comWordTable = null;
            return comRangeCurrent;
        }

        private static Range Output_Table_2(Document currentDoc, Range comRangeCurrent, float flTotalWidth)
        {
            Table comWordTable = currentDoc.Tables.Add(comRangeCurrent, 1, 5);

            // Format the table by setting borders, etc.
            comWordTable.Borders.Enable = 1; // Enable borders for readability

            int cRowCount = comWordTable.Rows.Count;
            int cColCount = comWordTable.Columns.Count;

            try
            {
                // We use the total width from the previous table.
                // float flTotalWidth
                comWordTable.Columns[1].SetWidth(flTotalWidth * 0.1667F, WdRulerStyle.wdAdjustNone);
                comWordTable.Columns[2].SetWidth(flTotalWidth * 0.1333F, WdRulerStyle.wdAdjustNone);
                comWordTable.Columns[3].SetWidth(flTotalWidth * 0.3F, WdRulerStyle.wdAdjustNone);
                comWordTable.Columns[4].SetWidth(flTotalWidth * 0.3F, WdRulerStyle.wdAdjustNone);
                comWordTable.Columns[5].SetWidth(flTotalWidth * 0.1F, WdRulerStyle.wdAdjustNone);

                comWordTable.Cell(1, 1).Range.Text = "References";
                comWordTable.Cell(1, 1).Range.Bold = 1;

                comWordTable.Cell(1, 2).Range.Text = "NodeClass";
                comWordTable.Cell(1, 2).Range.Bold = 1;

                comWordTable.Cell(1, 3).Range.Text = "BrowseName";
                comWordTable.Cell(1, 3).Range.Bold = 1;

                comWordTable.Cell(1, 4).Range.Text = "DataType / TypeDefinition";
                comWordTable.Cell(1, 4).Range.Bold = 1;

                comWordTable.Cell(1, 5).Range.Text = "Modelling\r\nRule";
                comWordTable.Cell(1, 5).Range.Bold = 1;
            }
            catch (Exception ex)
            {
                string str1 = ex.ToString();
            }

            comRangeCurrent = comWordTable.Range;
            comRangeCurrent.Collapse(WdCollapseDirection.wdCollapseEnd);
            comRangeCurrent.MoveStart(WdUnits.wdCharacter, 1); // Move start 1 character beyond the table end.
            comRangeCurrent.InsertParagraphAfter();
            comRangeCurrent.Collapse(WdCollapseDirection.wdCollapseEnd);

            Marshal.ReleaseComObject(comWordTable);
            comWordTable = null;
            return comRangeCurrent;
        }

        private static Range Output_Table_3(Document currentDoc, Range comRangeCurrent, UAObjectType ua, float flTotalWidth)
        {
            Table comWordTable = currentDoc.Tables.Add(comRangeCurrent, 1, 1);

            // Format the table by setting borders, etc.
            comWordTable.Borders.Enable = 1; // Enable borders for readability

            int cRowCount = comWordTable.Rows.Count;
            int cColCount = comWordTable.Columns.Count;

            try
            {
                comWordTable.Cell(1, 1).Range.Text = $"Subtype of {ua.strSubType}";
            }
            catch (Exception ex)
            {
                string str1 = ex.ToString();
            }

            comRangeCurrent = comWordTable.Range;
            comRangeCurrent.Collapse(WdCollapseDirection.wdCollapseEnd);
            comRangeCurrent.MoveStart(WdUnits.wdCharacter, 1); // Move start 1 character beyond the table end.
            comRangeCurrent.InsertParagraphAfter();
            comRangeCurrent.Collapse(WdCollapseDirection.wdCollapseEnd);

            Marshal.ReleaseComObject(comWordTable);
            comWordTable = null;
            return comRangeCurrent;
        }

        private static Range Output_Table_4(Document currentDoc, Range comRangeCurrent, UAObjectType ua, float flTotalWidth)
        {
            int cRows = ua.references.Count;
            cRows = (cRows > 0) ? cRows : 1;    
            Table comWordTable = currentDoc.Tables.Add(comRangeCurrent, cRows, 5);

            // Format the table by setting borders, etc.
            comWordTable.Borders.Enable = 1; // Enable borders for readability

            int cRowCount = comWordTable.Rows.Count;
            int cColCount = comWordTable.Columns.Count;

            try
            {
                // We use the total width from the previous table.
                // float flTotalWidth
                comWordTable.Columns[1].SetWidth(flTotalWidth * 0.1667F, WdRulerStyle.wdAdjustNone);
                comWordTable.Columns[2].SetWidth(flTotalWidth * 0.1333F, WdRulerStyle.wdAdjustNone);
                comWordTable.Columns[3].SetWidth(flTotalWidth * 0.3F, WdRulerStyle.wdAdjustNone);
                comWordTable.Columns[4].SetWidth(flTotalWidth * 0.3F, WdRulerStyle.wdAdjustNone);
                comWordTable.Columns[5].SetWidth(flTotalWidth * 0.1F, WdRulerStyle.wdAdjustNone);

                for (int iRow = 1; iRow <= cRows; iRow++)
                {
                    comWordTable.Cell(iRow, 1).Range.Text = ua.references[iRow-1].strReferenceType;
                    comWordTable.Cell(iRow, 2).Range.Text = ua.references[iRow-1].strNodeClass;
                    comWordTable.Cell(iRow, 3).Range.Text = ua.references[iRow-1].strBrowseName;
                    comWordTable.Cell(iRow, 4).Range.Text = $"{ua.references[iRow-1].strDataType}\r\n{ua.references[iRow-1].strTypeDefinition}";
                    comWordTable.Cell(iRow, 5).Range.Text = ua.references[iRow-1].strModellingRule;
                }
            }
            catch (Exception ex)
            {
                string str1 = ex.ToString();
            }

            comRangeCurrent = comWordTable.Range;
            comRangeCurrent.Collapse(WdCollapseDirection.wdCollapseEnd);
            comRangeCurrent.MoveStart(WdUnits.wdCharacter, 1); // Move start 1 character beyond the table end.
            comRangeCurrent.InsertParagraphAfter();
            comRangeCurrent.Collapse(WdCollapseDirection.wdCollapseEnd);

            Marshal.ReleaseComObject(comWordTable);
            comWordTable = null;
            return comRangeCurrent;
        }

    } // class
} // namespace
