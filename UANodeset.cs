using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace XmlScan
{
    public class NodeSet
    {
        public static string strResultBuffer = null;

        public static int CountAllFiles(string strRootFolder)
        {
            int cTotalFiles = 0;
            if (strRootFolder != null && Directory.Exists(strRootFolder))
            {
                // Enumerate all XML files in the directory and subdirectories
                List<string> listFiles = Directory.GetFiles(strRootFolder, "*.xml", SearchOption.AllDirectories).ToList();
                cTotalFiles = listFiles.Count;
            }

            return cTotalFiles;
        }

        public static bool LoadAllFiles(string strRootFolder, System.Windows.Forms.TextBox tbStatus)
        {
            bool bSuccess = true;

            // Reset both of the static dictionaries
            Model.Clear();

            if (strRootFolder != null && Directory.Exists(strRootFolder))
            {
                // Enumerate all XML files in the directory and subdirectories
                List<string> listFiles = Directory.GetFiles(strRootFolder, "*.xml", SearchOption.AllDirectories).ToList();

                StringBuilder sb = new StringBuilder();

                int cTotalFiles = listFiles.Count;
                int cFilesScanned = 0;

                foreach (string f in listFiles)
                {
                    Utils.Debug_WriteLine(Model.g_bDumpInitResults, $"LoadAllFiles: {cFilesScanned} of {cTotalFiles}: {f}");
                    Model m = new Model(f);
                    bool bInitSuccess = m.Init(out string strErrorMessage);
                    if (Model.g_bDumpInitResults && sb != null)
                    {
                        sb.Clear();
                        sb.AppendLine($"File Path: {f}");
                        sb.AppendLine($"Id: {m.Id}");
                        sb.AppendLine($"Model {m.ModelUri} {m.PublicationDate} {m.Version}");
                    }
                    if (bInitSuccess)
                    {
                        Utils.Debug_WriteLine(Model.g_bDumpInitResults, $"{sb.ToString()}\r\n==============================");
                    }
                    else
                    {
                        Utils.Debug_WriteLine(Model.g_bDumpInitResults, $"Error: {strErrorMessage}\r\n\r\n{sb.ToString()}\r\n==============================");
                    }

                    cFilesScanned++;
                    tbStatus.Text = $"Scanned: {cFilesScanned} of {cTotalFiles} files found. (Click Cancel to stop.)";
                    tbStatus.Refresh();
                }

                Model.LinkDependentNodeSets();
            }

            return bSuccess;
        }

        public static bool LoadNodeSetData(Guid g)
        {
            bool bSuccess = false;
            if (Model.TryGetModel(g, out Model m))
            {
                if (m != null && m.bInitialised)
                {
                    foreach (Guid gRequired in m.RequiredModels)
                    {
                        NodeSet.LoadNodeSetData(gRequired);
                    }

                    if (m.listObjectTypeDefinitions == null)
                    {
                        // Input to our processing is all nodes of type <UAObjectType>
                        m.listObjectTypeDefinitions = Utils_xml.FindAllChildrenWithTag(m.xmlNodeset, "UAObjectType");

                        // Output from our processing are the details that typically go into documentation for
                        // Opc/Ua NodeSet files. This includes the object details and the references.
                        m.listOutputObjectDetails = new List<UAObjectType>();

                        // Loop through all objects defined in this nodeset
                        for (int iObject = 0; iObject < m.listObjectTypeDefinitions.Count; iObject++)
                        {
                            XmlNode xmlNode = m.listObjectTypeDefinitions[iObject];

                            if (xmlNode != null)
                            {
                                //XmlNode nodeDisplayName = Utils_xml.FindOneChildWithTag(xmlNode, "DisplayName");
                                //string strDisplayName = null;
                                //if (nodeDisplayName != null && nodeDisplayName.InnerText != null)
                                //    strDisplayName = nodeDisplayName.InnerText;
                                string strDisplayName = Utils_xml.FindChildDisplayName(xmlNode, "");

                                string strBrowseName = (strDisplayName != null) ? strDisplayName : xmlNode.Attributes["BrowseName"].Value;
                                string strNodeId = xmlNode.Attributes["NodeId"].Value;
                                // string strParentNodeId = xmlNode.Attributes["ParentNodeId"].Value;
                                string strIsAbstract = "";
                                if (xmlNode.Attributes["IsAbstract"] != null && xmlNode.Attributes["IsAbstract"].Value != null)
                                    strIsAbstract = xmlNode.Attributes["IsAbstract"].Value;

                                string strCategory = null;
                                XmlNode xmlNodeCat = Utils_xml.FindOneChildWithTag(xmlNode, "Category");
                                if (xmlNodeCat != null && xmlNodeCat.InnerText != null)
                                    strCategory = xmlNodeCat.InnerText;
                                strCategory = (strCategory == null) ? strCategory = "" : strCategory;

                                Utils.Debug_WriteLine(Model.g_bDebugGetReferenceDetails, $"LoadModelObjectTypes(): Object [{iObject}]: " +
                                                                                         $"Browse Name:{strBrowseName}," +
                                                                                         $" Node Id: {strNodeId}," +
                                                                                         // $" strParentNodeId: {strParentNodeId}," +
                                                                                         $" IsAbstract: {strIsAbstract}," +
                                                                                         $" Category: {strCategory}");


                                UAObjectType uat = new UAObjectType();
                                uat.strBrowseName = strBrowseName;
                                uat.bIsAbstract = (strIsAbstract == "true");
                                uat.strCategory = (strCategory == null) ? "" : strCategory;

                                m.listOutputObjectDetails.Add(uat);

                                // Get node with "References" tag
                                XmlNode xmlnodeReferences = Utils_xml.FindOneChildWithTag(xmlNode, "References");

                                if (xmlnodeReferences != null &&
                                    xmlnodeReferences.HasChildNodes &&
                                    xmlnodeReferences.ChildNodes.Count > 0)
                                {
                                    if (m.dictNodeId != null)
                                    {
                                        uat.references = NodeSet.GetReferenceDetails(m, uat, xmlnodeReferences);
                                    }
                                    else
                                    {
                                        throw new Exception("Error: Model.LoadModelObjectTypes() - m.dictNodeId is null. This should not happen.");
                                    }
                                } // if (myRefernces != null...
                            } // if (nodeObject != null...
                        } // for (int iObject...
                    }
                }
                else
                {
                    throw new Exception("Error: Model.LoadModelObjectTypes() - Model not initialised.");
                }
            }

            return bSuccess;
        }

        public static List<UAReference> GetReferenceDetails(Model m, /* Dictionary<string, XmlNode> dictNodeId, */ UAObjectType uat, XmlNode xmlnodeReferences)
        {
            List<UAReference> listReferenceDetails = new List<UAReference>();

            int cReferences = (xmlnodeReferences == null) ? 0 : xmlnodeReferences.ChildNodes.Count;

            // Outer References Loop
            // The calling XmlNode has a list of references. The following loop
            // processes each of the references for this outermost XmlNode
            for (int iReference = 0; xmlnodeReferences != null &&
                                     iReference < xmlnodeReferences.ChildNodes.Count; iReference++)
            {
                // Allocate data structure for the output we are going to create.
                // Add it to the list of items.
                UAReference uaReference = new UAReference();
                bool bAddReference = true;

                XmlNode xmlnodeOuterReference = xmlnodeReferences.ChildNodes[iReference];
                if (xmlnodeOuterReference != null &&
                    xmlnodeOuterReference.InnerText != null &&
                    xmlnodeOuterReference.Attributes != null)
                {
                    // The reference type is one of several possible values:
                    // -- HasSubtype -- indicates base type for current type
                    // -- HasProperty -- indicates a "HasProperty" type of reference
                    // -- HasComponent -- indicates a "HasComponent" type of reference
                    // -- HasModellingRule -- indicates a modelling rule:
                    //         --- "Mandatory"
                    //         --- "Optional"   
                    // Within <UAObjectType><References><Reference ReferenceType="xxxxx>...</Reference>[strTargetId]</References></UAObjectType>
                    string strReferenceType = (xmlnodeOuterReference.Attributes["ReferenceType"] != null && xmlnodeOuterReference.Attributes["ReferenceType"].Value != null) ? xmlnodeOuterReference.Attributes["ReferenceType"].Value : "";

                    // The [strTargetId] is a node identifier like "i=1234", or "i=xxxxx"
                    string strTargetId = xmlnodeOuterReference.InnerText;
                    if (!string.IsNullOrEmpty(strReferenceType) && !string.IsNullOrEmpty(strTargetId))
                    {
                        // Search for the node references as the text in the outer reference
                        bool bNodeFound = m.TryGetXmlNodeValue(strTargetId, out XmlNode xmlnodeInner);
                        if (!bNodeFound)
                        {
                            Utils.Debug_WriteLine(Model.g_bDebugGetReferenceDetails, $"GetReferenceDetails():  Referenced Node \"{strTargetId}\" not found (204). ");
                            Utils_xml.IncrementDictionaryCount(m.dictMissingNodeIdValues, strTargetId);
                        }
                        else
                        {
                            if (xmlnodeInner != null &&
                                xmlnodeInner.Attributes != null)
                            {
                                string strRefBrowseName = (xmlnodeInner.Attributes["BrowseName"] != null && xmlnodeInner.Attributes["BrowseName"].Value != null) ? xmlnodeInner.Attributes["BrowseName"].Value : "";

                                string strRefDataType = (xmlnodeInner.Attributes["DataType"] != null && xmlnodeInner.Attributes["DataType"].Value != null) ? xmlnodeInner.Attributes["DataType"].Value : "";

                                string strRefArrayDimensions = (xmlnodeInner.Attributes["ArrayDimensions"] != null && xmlnodeInner.Attributes["ArrayDimensions"].Value != null) ? xmlnodeInner.Attributes["ArrayDimensions"].Value : "";

                                string strRefTagType = xmlnodeInner.Name;

                                string strDataTypeName = m.GetBrowseName(strRefDataType);
                                Utils.Debug_WriteLine(Model.g_bDebugGetReferenceDetails, $"GetReferenceDetails():  --- Reference {iReference}/{cReferences} Name:{strRefBrowseName}  Type:{strRefTagType} RefDataType:{strRefDataType} DataType:{strDataTypeName}   ArrayDimensions:{strRefArrayDimensions} Reference Type: {strReferenceType}");

                                // Capture the Sub type if any are mentioned.
                                if (strReferenceType == "HasSubtype")
                                {
                                    bAddReference = false;
                                    //XmlNode xmlDisplay = Utils_xml.FindOneChildWithTag(xmlnodeInner, "DisplayName");
                                    //if (xmlDisplay != null && xmlDisplay.InnerText != null)
                                    //{
                                    //    uat.strSubType = xmlDisplay.InnerText;
                                    //}
                                    //else
                                    //{
                                    //    uat.strSubType = strRefBrowseName;
                                    //}

                                    uat.strSubType = Utils_xml.FindChildDisplayName(xmlnodeInner, strRefBrowseName);
                                }

                                // Two variables that might get filled in while parsing through the references
                                string strSubReferenceTypeDefinition = ""; // when strSubReferenceType == "HasTypeDefinition"
                                string strModellingRule = "";              // when strSubReferenceType == "HasModellingRule"

                                //XmlNode nodeDisplayName = Utils_xml.FindOneChildWithTag(xmlnodeInner, "DisplayName");
                                //string strDisplayName = nodeDisplayName.InnerText;
                                string strDisplayName = Utils_xml.FindChildDisplayName(xmlnodeInner, "");

                                // Inner References Loop
                                // Get node with "References" tag in the referenced tag.
                                XmlNode xmlnodeInnerReferences = Utils_xml.FindOneChildWithTag(xmlnodeInner, "References");

                                if (xmlnodeInnerReferences != null &&
                                    xmlnodeInnerReferences.HasChildNodes &&
                                    xmlnodeInnerReferences.ChildNodes.Count > 0)
                                {
                                    for (int iSubReference = 0; iSubReference < xmlnodeInnerReferences.ChildNodes.Count; iSubReference++)
                                    {
                                        XmlNode xmlnodeChildNode = xmlnodeInnerReferences.ChildNodes[iSubReference];
                                        if (xmlnodeChildNode != null &&
                                            xmlnodeChildNode.InnerText != null)
                                        {
                                            int cSubReferences = xmlnodeInnerReferences.ChildNodes.Count;

                                            if (xmlnodeChildNode.Attributes != null)
                                            {
                                                // The reference type is one of several possible values:
                                                // -- HasSubtype -- indicates base type for current type
                                                // -- HasProperty -- indicates a "HasProperty" type of reference
                                                // -- HasComponent -- indicates a "HasComponent" type of reference
                                                // -- HasModellingRule -- indicates a modelling rule:
                                                //         --- "Mandatory"
                                                //         --- "Optional"   
                                                string strSubReferenceType = (xmlnodeChildNode.Attributes["ReferenceType"] != null && xmlnodeChildNode.Attributes["ReferenceType"].Value != null) ? xmlnodeChildNode.Attributes["ReferenceType"].Value : "";

                                                string strSubTargetId = xmlnodeChildNode.InnerText;
                                                if (!string.IsNullOrEmpty(strSubReferenceType) && !string.IsNullOrEmpty(strSubTargetId))
                                                {
                                                    // Get the target node
                                                    // XmlNode? xmlnodeChildNodeLookupSubReference = dictNodeId[strSubTargetId];
                                                    bool bFound = m.TryGetXmlNodeValue(strSubTargetId, out XmlNode xmlnodeChildNodeLookupSubReference);

                                                    if (!bFound)
                                                    {
                                                        Utils.Debug_WriteLine(Model.g_bDebugGetReferenceDetails, $"GetReferenceDetails():  Referenced Node \"{strSubTargetId}\" not found (264). ");
                                                        Utils_xml.IncrementDictionaryCount(m.dictMissingNodeIdValues, strSubTargetId);
                                                    }
                                                    else
                                                    {
                                                        if (xmlnodeChildNodeLookupSubReference != null &&
                                                            xmlnodeChildNodeLookupSubReference.Attributes != null)
                                                        {
                                                            if (strSubReferenceType == "HasTypeDefinition")
                                                            {
                                                                string strName = m.GetBrowseName(strSubTargetId, true);
                                                                strName = (strName == null) ? "" : strName;
                                                                Utils.Debug_WriteLine(Model.g_bDebugGetReferenceDetails, $"GetReferenceDetails():      --- SubReference {iSubReference}/{cSubReferences} Name:HasTypeDefinition  Type: {strSubTargetId} {strName}");
                                                                strSubReferenceTypeDefinition = strName;
                                                            }
                                                            else if (strSubReferenceType == "HasModellingRule")
                                                            {
                                                                string strName = m.GetBrowseName(strSubTargetId, true);
                                                                strName = (strName == null) ? "" : strName;
                                                                Utils.Debug_WriteLine(Model.g_bDebugGetReferenceDetails, $"GetReferenceDetails():      --- SubReference {iSubReference}/{cSubReferences} Name: HasModellingRule  Type:{strSubTargetId} {strName}");
                                                                strModellingRule = strName;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        uaReference.strReferenceType = strReferenceType;
                                        uaReference.strNodeClass = Utils_xml.GetNodeClassName(strRefTagType);
                                        uaReference.strBrowseName = (strDisplayName != null) ? strDisplayName :
                                                                    (strRefBrowseName != null) ? strRefBrowseName :
                                                                    "";
                                        uaReference.strDataType = Utils_xml.GetDataType(strRefDataType, strDataTypeName, strRefArrayDimensions, strSubReferenceTypeDefinition);
                                        uaReference.strTypeDefinition = strSubReferenceTypeDefinition;
                                        uaReference.strModellingRule = strModellingRule;
                                    } // for (int iSubReference = 0...
                                } // if (nodeSubReferences != null...

                                if (bAddReference)
                                {
                                    listReferenceDetails.Add(uaReference);
                                }

                            } // if (xmlnodeInner != null...
                        } // if (!bNodeFound...
                    } // if (!string.IsNullOrEmpty(strReferenceType) && !string.IsNullOrEmpty(strTargetId)...


                } // if (myRef != null...


            }  // for (int iReference...

            return listReferenceDetails;
        }



    } // class NodeSet
} // namespace
