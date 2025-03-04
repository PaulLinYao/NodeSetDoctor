using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace XmlScan
{
    internal class Utils_xml
    {
        public static bool InitXmlNodesetFile(string strXmlFilePath, out XmlDocument xmlDoc, out XmlNode uanodeset, out string strResult)
        {
            // Init return values
            uanodeset = null;
            strResult = "Success";

            // Load file strXmlFilePath into an XML document
            xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(strXmlFilePath);
            }
            catch (Exception ex)
            {
                strResult = $"Error XmlDocument.Load for {strXmlFilePath}: {ex.Message}";
                Utils.Debug_WriteLine(Model.g_bDumpInitResults, $"InitXmlNodesetFile: {strResult}");
                
                return false;
            }

            uanodeset = null;

            XmlNodeList rootnode = xmlDoc.SelectNodes("/");
            bool bSuccess = (rootnode != null && rootnode.Count > 0);
            if (bSuccess)
            {
                XmlNode nodeStart = (rootnode == null) ? null : rootnode[0];
                uanodeset = FindOneChildWithTag(nodeStart, "UANodeSet");
                bSuccess = (uanodeset != null);
                if (!bSuccess)
                {
                    XmlNode xmlModelDesign = FindOneChildWithTag(nodeStart, "ModelDesign");
                    XmlNode xmlNodeState = FindOneChildWithTag(nodeStart, "uax:ListOfNodeState");

                    if (xmlModelDesign != null)
                    {
                        strResult = $"Error: No UANodeSet found. {strXmlFilePath} is a Model Design file.";
                        Utils.Debug_WriteLine(Model.g_bDumpInitResults, $"InitXmlNodesetFile: {strResult}");
                    }
                    else if (xmlNodeState != null)
                    {
                        strResult = $"Error: No UANodeSet node. {strXmlFilePath} is a List Of Node State file.";
                        Utils.Debug_WriteLine(Model.g_bDumpInitResults, $"InitXmlNodesetFile: {strResult}");
                    }
                    else
                    {
                        strResult = $"Error: No UANodeSet node in {strXmlFilePath}.";
                        Utils.Debug_WriteLine(Model.g_bDumpInitResults, $"InitXmlNodesetFile: {strResult}");
                    }
                }   
            }
            else
            {
                strResult = "Error in Xml: Expected root node not found.";
                Utils.Debug_WriteLine(Model.g_bDumpInitResults, strResult);
            }

            return bSuccess;
        }

        /// <summary>
        /// CreateNodeIdDictionary - Initialize dictionary of an Nodeset with all 
        /// child nodes with a "NodeId" attribute. These are the core set of XML 
        /// nodes of interest in an Opc/Ua NodeSet file.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static Dictionary<string, XmlNode> CreateNodeIdDictionary(XmlNode node)
        {
            Dictionary<string, XmlNode> dictInit = new Dictionary<string, XmlNode>();

            if (node != null && node.HasChildNodes)
            {
                for (int iNode = 0; iNode < node.ChildNodes.Count; iNode++)
                {
                    XmlNode myChild = node.ChildNodes[iNode];
                    string strChildName = (myChild == null) ? "" : myChild.Name;

                    if (strChildName != null &&
                        myChild != null &&
                        myChild.Attributes != null &&
                        myChild.Attributes.Count > 0)
                    {
                        try
                        {
                            string strNodeId = (myChild.Attributes["NodeId"] != null && myChild.Attributes["NodeId"].Value != null) ? myChild.Attributes["NodeId"].Value : "";

                            if (!string.IsNullOrEmpty(strNodeId))
                            {
                                dictInit.Add(strNodeId, myChild);
                            }
                        }
                        catch { }
                    }
                }
            }

            return dictInit;
        }


        public static XmlNode FindOneChildWithTag(XmlNode node, string strName)
        {
            XmlNode nodeReturn = null;

            if (node != null && !string.IsNullOrEmpty(strName))
            {
                if (node != null && node.HasChildNodes)
                {
                    for (int i = 0; i < node.ChildNodes.Count; i++)
                    {
                        XmlNode myChild = node.ChildNodes[i];
                        string strChildName = (myChild == null) ? "" : myChild.Name;

                        if (strChildName != null && strChildName == strName)
                        {
                            nodeReturn = myChild;
                        }
                    }
                }
            }

            return nodeReturn;
        }

        public static List<XmlNode> FindAllChildrenWithTag(XmlNode node, string strName)
        {
            List<XmlNode> listReturn = new List<XmlNode>();

            if (node != null && !string.IsNullOrEmpty(strName))
            {
                if (node != null && node.HasChildNodes)
                {
                    for (int i = 0; i < node.ChildNodes.Count; i++)
                    {
                        XmlNode myChild = node.ChildNodes[i];
                        string strChildName = (myChild == null) ? "" : myChild.Name;

                        if (strChildName != null && strChildName == strName)
                        {
                            listReturn.Add(myChild);
                        }
                    }
                }
            }

            return listReturn;
        }

        public static Dictionary<string,int> QueryMissingInternalReferences(Dictionary<string, XmlNode> dictNodeId, XmlNode nodeset, string strTag)
        {
            Dictionary<string, int> dictMissingNodeIdValues = new Dictionary<string, int>();

            // Input to our processing is all nodes of type <UAObjectType>
            List<XmlNode> listInputXmlNodesetData = Utils_xml.FindAllChildrenWithTag(nodeset, strTag);

            return dictMissingNodeIdValues;
        }

        public static string GetDataType(string strRefNode, string strDataType, string strArrayDimensions, string strSubRefTypeDefinition)
        {

            if (strDataType != null)
            {
                if (strArrayDimensions != null && strArrayDimensions == "0")
                    return $"{strDataType}[]";
                else
                    return strDataType;
            }

            return "-";
        }

        public static void IncrementDictionaryCount(Dictionary<string, int> dict, string strSubTargetId)
        {
            if (strSubTargetId != null)
            {
                if (dict.TryGetValue(strSubTargetId, out int iValue))
                {
                    iValue++;
                    dict[strSubTargetId] = iValue;
                }
                else
                {
                    dict.Add(strSubTargetId, 1);
                }
            }
        }


        public static string GetNodeAttribute(XmlNode input, string strAttName)
        {
            string strReturn = null;
            if (input != null &&
                input.Attributes != null &&
                strAttName != null)
            {
                try
                {
                    if (input.Attributes[strAttName] != null &&
                        input.Attributes[strAttName].Value != null)
                    {
                        strReturn = input.Attributes[strAttName].Value;
                    }
                }
                catch { }
            }

            return strReturn;
        }

        public static string GetNodeClassName(string strTypeName)
        {
            if (strTypeName == "UAVariable") return "Variable";
            if (strTypeName == "UAObject") return "Object";
            if (strTypeName == "UAMethod") return "Method";
            return "**Unknown**";
        }

        public static string FindChildDisplayName(XmlNode node, string strReturn)
        {
            if (node != null)
            {
                XmlNode xmlDisplay = Utils_xml.FindOneChildWithTag(node, "DisplayName");
                if (xmlDisplay != null && xmlDisplay.InnerText != null)
                {
                    strReturn = xmlDisplay.InnerText;
                }
            }

            return strReturn;
        }

    } // class Utils_Xml
} // namespace
