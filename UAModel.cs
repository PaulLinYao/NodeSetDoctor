using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace XmlScan
{
    public class Model
    {
        public static void Clear()
        {
            g_AllModels.Clear();
            // g_AllModelsReverseLookup.Clear();
        }

        public static Dictionary<Guid,Model> g_AllModels = new Dictionary<Guid,Model>();
        // public static Dictionary<string,Guid> g_AllModelsReverseLookup = new Dictionary<string, Guid>();

        public Guid Id;
        public string ModelUri = string.Empty;
        public string PublicationDate = string.Empty;
        public string Version = string.Empty;
        public List<Guid> RequiredModels = new List<Guid>();
        public string strNodesetFilePath = null;
        public XmlDocument xmlDoc;
        public XmlNode xmlNodeset;
        public string strLastErrorMessage = string.Empty;
        public Dictionary<string, XmlNode> dictNodeId = null;
        public List<XmlNode> listObjectTypeDefinitions = null;
        public List<UAObjectType> listOutputObjectDetails = null;
        public Dictionary<string, int> dictMissingNodeIdValues = new Dictionary<string, int>();
        public bool bPlaceholder = false;
        public bool bInitialised = false;

        public static bool bDumpDocDetails = true;
        public static bool g_bDebugGetReferenceDetails = true;
        public static bool g_bGetXmlNode = false;
        public static bool g_bDumpInitResults = true;

        /// <summary>
        /// Constructor for a model that is defined by a file path.
        /// </summary>
        /// <param name="strPath"></param>
        public Model(string strPath)
        {
            Id = Guid.NewGuid();
            strNodesetFilePath = strPath;
        }

        /// <summary>
        /// Create a placeholder model that was referenced in an actual model.
        /// We remove this later after all known models have been loaded.
        /// </summary>
        /// <param name="strModelUri"></param>
        /// <param name="strPublicationDate"></param>
        /// <param name="strVersion"></param>
        public Model(string strModelUri, string strPublicationDate, string strVersion)
        {
            Id = Guid.NewGuid();
            ModelUri = strModelUri;
            PublicationDate = strPublicationDate;
            Version = strVersion;
            bPlaceholder = true;
        }

        ~Model()
        {
            this.xmlDoc = null;  // Free the XML document object so that it may be garbage collected.
            g_AllModels.Remove(Id);
        }

        private string GetComboKey(string strModelUri, string strPublicationDate, string strVersion)
        {
            return $"{strModelUri}{strPublicationDate}{strVersion}";
        }

        private string GetComboKey()
        {
            return $"{ModelUri}{PublicationDate}{Version}";
        }

        /// <summary>
        /// Init - in keeping with C# best practices, we do little in the constructor.
        /// All real initialization occurs here.
        /// </summary>
        /// <returns></returns>
        public bool Init(out string strResult)
        {
            bool bRet = false;
            strResult = "";
            if (!bPlaceholder && !string.IsNullOrEmpty(strNodesetFilePath))
            {
                if (File.Exists(strNodesetFilePath))
                {
                    // Check whether we have an xml file with a root "UANodeSet" tag.
                    bRet = Utils_xml.InitXmlNodesetFile(strNodesetFilePath, out xmlDoc, out xmlNodeset, out strResult);
                    if (bRet)
                    {
                        bRet = InitModelRequiredModels(out strResult);
                        if (bRet)
                        {
                            int cNodes = InitNodeIdDictionary();
                            Utils.Debug_WriteLine(Model.g_bDumpInitResults, $"Model.Init: {cNodes} NodeId values found.");

                            //string strComboKey = GetComboKey();
                            //if (g_AllModelsReverseLookup.ContainsKey(strComboKey))
                            //{
                            //    strResult = $"Information: Model already exists - not getting added: {strComboKey}";
                            //    Utils.Debug_WriteLine(Model.g_bDumpInitResults, $"Model.Init: {strResult}");
                            //    bRet = false;
                            //}
                            //else
                            {
                                g_AllModels.Add(Id, this);
                                //g_AllModelsReverseLookup.Add(GetComboKey(), Id);
                                bInitialised = true;
                            }
                        }
                        else
                        {
                            strLastErrorMessage = strResult;
                        }
                    }
                }
            }
            return bRet;
        }

        public int InitNodeIdDictionary()
        {
            int cNodes = 0;

            Dictionary<string, XmlNode> dict = Utils_xml.CreateNodeIdDictionary(xmlNodeset);
            cNodes = dict.Count;
            if (cNodes > 0)
            {
                dictNodeId = dict;
            }

            return cNodes;
        }

        private bool InitModelRequiredModels(out string strResult)
        {
            bool bRet = false;
            strResult = "";
            XmlNode nodeOuter = Utils_xml.FindOneChildWithTag(xmlNodeset, "Models");
            if (nodeOuter != null)
            {
                XmlNode nodeInner = Utils_xml.FindOneChildWithTag(nodeOuter, "Model");
                if (nodeInner != null && nodeInner.NodeType == XmlNodeType.Element)
                {
                    bRet = FetchModelAttributes(nodeInner, ref strResult); 
                }
                else
                {
                    strResult = "Error: Expected Modes/Model node not found.";
                    Utils.Debug_WriteLine(Model.g_bDumpInitResults, $"InitModelDetails: {strResult}");
                }
            }
            else
            {
                strResult = "Error in Xml: Expected Models node not found.";
                Utils.Debug_WriteLine(Model.g_bDumpInitResults, $"InitModelDetails: {strResult}");
            }

            return bRet;
        }


        private bool FetchModelAttributes(XmlNode nodeInner, ref string strResult)
        {
            bool bRet = false;
            strResult = "";
            if (nodeInner != null && nodeInner.Attributes != null)
            {
                this.ModelUri = nodeInner.Attributes["ModelUri"].Value;
                this.PublicationDate = nodeInner.Attributes["PublicationDate"].Value;
                this.Version = nodeInner.Attributes["Version"].Value;
                bRet = true;
                if (bRet)
                {
                    bRet = InitRequiredModels(nodeInner, out strResult);
                }
            }
            else
            {
                strResult = "Error in Xml: Expected Model node attributes not found.";
                Utils.Debug_WriteLine(Model.g_bDumpInitResults, $"FetchModelAttributes: {strResult}");
            }

            return bRet;
        }

        private bool InitRequiredModels(XmlNode nodeInner, out string strResult)
        {
            bool bRet = false;
            strResult = "";
            int cRequiredModelNodes = 0;
            int cNodesAdded = 0;
            if (nodeInner != null)
            {
                List<XmlNode> nodes = Utils_xml.FindAllChildrenWithTag(nodeInner, "RequiredModel");
                cRequiredModelNodes = nodes.Count;
                if (cRequiredModelNodes > 0)
                {
                    foreach (XmlNode n in nodes)
                    {
                        if (n != null && n.Attributes != null)
                        {
                            string strModelUri = n.Attributes["ModelUri"].Value;
                            string strPublicationDate = n.Attributes["PublicationDate"].Value;
                            string strVersion = n.Attributes["Version"].Value;
                            if (!string.IsNullOrEmpty(strModelUri))
                            {
                                //string strPubDate = (strPublicationDate == null) ? "" : strPublicationDate;
                                //string strVer = (strVersion == null) ? "" : strVersion;
                                //string strComboKey = GetComboKey(strModelUri, strPubDate, strVer);
                                //if (g_AllModelsReverseLookup.TryGetValue(strComboKey, out Guid guidFound))
                                //{
                                //    RequiredModels.Add(guidFound);
                                //}
                                //else
                                {
                                    Model m = new Model(strModelUri, strPublicationDate, strVersion);
                                    RequiredModels.Add(m.Id);
                                    g_AllModels.Add(m.Id, m);
                                    //g_AllModelsReverseLookup.Add(strComboKey, m.Id);  // Just an empty placeholder, but we still add this to the reverse lookup.
                                    cNodesAdded++;
                                }
                            }
                        }
                    }
                }
            }

            if (cRequiredModelNodes == RequiredModels.Count)
            {
                bRet = true;
            }
            else
            {
                strResult = $"Error: RequiredModel had {cRequiredModelNodes} nodes. Only {cNodesAdded} valid values found.";
                Utils.Debug_WriteLine(Model.g_bDumpInitResults, $"InitRequiredModels: {strResult}");
            }

            return bRet;
        }

        /// <summary>
        /// LinkDependentNodeSets - Set all required models to actual models that we have loaded,
        /// removing temporary placeholder since we no longer need them.
        /// </summary>
        public static void LinkDependentNodeSets()
        {
            int cInit=0;
            int cNotInit = 0;
            foreach (KeyValuePair<Guid, Model> kvpCount in g_AllModels)
            {
                if (kvpCount.Value.bInitialised)
                {
                    cInit++;
                }
                else
                {
                    cNotInit++;
                }
            }

            List<Guid> listToBeRemoved = new List<Guid>();


            // Loop through all models.
            foreach (KeyValuePair<Guid, Model> kvp in g_AllModels)
            {
                Model m = kvp.Value;
                for (int i = 0; i < m.RequiredModels.Count; i++)
                {
                    Guid id = m.RequiredModels[i];
                    if (g_AllModels.TryGetValue(id, out Model mRequired))
                    {
                        // When a required model has not been initialzed, 
                        // look for one that HAS been initialized to replace it.
                        if (!mRequired.bInitialised)
                        {
                            bool bFound = false;
                            foreach (KeyValuePair<Guid, Model> kvpInner in g_AllModels)
                            {
                                Model mCandidate = kvpInner.Value;
                                if (mRequired.ModelUri == mCandidate.ModelUri && 
                                    mRequired.Version == mCandidate.Version && 
                                    mRequired.PublicationDate == mCandidate.PublicationDate &&
                                    mCandidate.bInitialised)
                                {
                                    m.RequiredModels[i] = mCandidate.Id;
                                    listToBeRemoved.Add(id);
                                    bFound = true;
                                    break;
                                }
                            }
                            if (!bFound)
                            {
                                Utils.Debug_WriteLine(Model.g_bDumpInitResults, $"Error: LinkDependentNodeSets - Required model not found: {id}");
                                throw new Exception($"Error: LinkDependentNodeSets - Required model not found: {id}");
                            }
                        }
                    }
                    else
                    {
                        Utils.Debug_WriteLine(Model.g_bDumpInitResults, $"Error: LinkDependentNodeSets - Required model not found: {id}");
                    }
                }
            }

            // Remove the models that are no longer required.
            foreach (Guid id in listToBeRemoved)
            {
                if (g_AllModels.TryGetValue(id, out Model m))
                {
                    //string strComboKey = m.GetComboKey();
                    //if (g_AllModelsReverseLookup.TryGetValue(strComboKey, out Guid guidFound))
                    //{
                    //    g_AllModelsReverseLookup.Remove(strComboKey);
                    //    Utils.Debug_WriteLine(Model.g_bDumpInitResults, $"Info: LinkDependentNodeSets - Item FOUND in g_AllModelsReverseLookup: {guidFound} - strComboKey:{strComboKey}");
                    //}
                    //else
                    //{
                    //    Utils.Debug_WriteLine(Model.g_bDumpInitResults, $"Error: LinkDependentNodeSets - Item not found in g_AllModelsReverseLookup: {id}  - strComboKey: {strComboKey}");
                    //}

                    Utils.Debug_WriteLine(Model.g_bDumpInitResults, $"Info: LinkDependentNodeSets - Item FOUND (and removed) from g_AllModels: {id}");
                    g_AllModels.Remove(id);
                }
                else
                {
                    Utils.Debug_WriteLine(Model.g_bDumpInitResults, $"Error: LinkDependentNodeSets - Item not found in g_AllModels: {id}. Already removed?!?");
                }
            }
        }

        /// <summary>
        /// GetAllPlaceholderModels - Return a list of all placeholder models.
        /// </summary>
        /// <returns></returns>
        public static List<Model> GetAllPlaceholderModels()
        {
            List<Model> list = new List<Model>();
            foreach (KeyValuePair<Guid, Model> kvp in g_AllModels)
            {
                if (kvp.Value.bPlaceholder)
                {
                    Utils.Debug_WriteLine(Model.g_bDumpInitResults, $"Info: Placeholder item left in list:  {kvp.Value.ModelUri}-{kvp.Value.Version}-{kvp.Value.PublicationDate}.");
                    list.Add(kvp.Value);
                }

                Guid guid = kvp.Key;
                foreach (KeyValuePair<Guid, Model> kvpInner in g_AllModels)
                {
                    Model m = kvpInner.Value;
                    Guid g = kvpInner.Key;
                    if (m.RequiredModels.Contains(guid))
                    {
                        Utils.Debug_WriteLine(Model.g_bDumpInitResults, $"Error: Placeholder items in required list for  {m.ModelUri}-{m.Version}-{m.PublicationDate}. Placeholder item: {kvp.Value.ModelUri}-{kvp.Value.Version}{kvp.Value.PublicationDate}");
                    }
                }
            }

            return list;
        }

        public static Guid FindModel(string strModelUri, string strPublicationDate, string strVersion)
        {
            Guid id = Guid.Empty;
            foreach (KeyValuePair<Guid, Model> kvp in g_AllModels)
            {
                Model m = kvp.Value;
                if (m.ModelUri == strModelUri && m.PublicationDate == strPublicationDate && m.Version == strVersion)
                {
                    id = m.Id;
                    break;
                }
            }

            return id;
        }


        public static bool TryGetModel(Guid g, out Model m)
        {
            bool bRet = false;
            m = null;
            if (g_AllModels.TryGetValue(g, out m))
            {
                bRet = true;
            }
            return bRet;
        }



        public bool TryGetXmlNodeValue(string strNodeId, out XmlNode xmlNode)
        {
            bool bRet = false;
            xmlNode = null;
            if (dictNodeId != null)
            {
                // Look in our own NodeId dictionary.
                bRet = dictNodeId.TryGetValue(strNodeId, out xmlNode);
                Utils.Debug_WriteLine(Model.g_bGetXmlNode, $"GetXmlNode(line 391): strNodeId [{strNodeId}] Return={bRet}");

                // if we cannot find it there, look in our required models.
                if (!bRet)
                {
                    if (strNodeId.Contains("ns="))
                    {
                        bRet = TryGetXmlNodeValueNamespace(strNodeId, out xmlNode);
                        Utils.Debug_WriteLine(Model.g_bGetXmlNode, $"GetXmlNode(line 399): strNodeId [{strNodeId}] Return={bRet}");
                    }
                    else
                    {
                        foreach (Guid g in RequiredModels)
                        {
                            if (g_AllModels.TryGetValue(g, out Model m))
                            {
                                bRet = m.TryGetXmlNodeValue(strNodeId, out xmlNode);
                                Utils.Debug_WriteLine(Model.g_bGetXmlNode, $"GetXmlNode(line 408): strNodeId [{strNodeId}] Return={bRet}");
                                if (bRet)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            return bRet;
        }

        public bool TryGetXmlNodeValueNamespace(string strNodeId, out XmlNode xmlNode)
        {
            bool bRet = false;
            xmlNode = null;

            if (strNodeId.Contains("ns="))
            {
                // Example:
                // Referenced Node "ns=2;s=AnnulusValvesType"
                string[] astrItems = strNodeId.Split(new char[] {';' } );
                if (astrItems.Length == 2)
                {
                    string strValue = astrItems[0].Replace("ns=", "");
                    if (int.TryParse(strValue, out int iNamespace))
                    {
                        for (; iNamespace > 0; iNamespace--)
                        {
                            string strTestNodeId = $"ns={iNamespace};{astrItems[1]}";

                            // We only look at RequireModels, since we assume
                            // they didn't confuse things in their own nodeset. 
                            foreach (Guid g in RequiredModels)
                            {
                                if (g_AllModels.TryGetValue(g, out Model m))
                                {
                                    bRet = m.TryGetXmlNodeValue(strTestNodeId, out xmlNode);
                                    if (bRet)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return bRet;
        }

        public string GetBrowseName(string strNodeId, bool bSearchOtherNodesets = false)
        {
            string strReturn = strNodeId;
            if (strNodeId != null && dictNodeId !=null)
            {
                if (dictNodeId.TryGetValue(strNodeId, out XmlNode node))
                {
                    if (node != null)
                    {
                        strReturn = Utils_xml.GetNodeAttribute(node, "BrowseName");
                        strReturn = Utils_xml.FindChildDisplayName(node, strReturn);
                    }
                }
                else
                {
                    if (bSearchOtherNodesets)
                    {
                        if (TryGetXmlNodeValue(strNodeId, out XmlNode xmlNode))
                        {
                            if (xmlNode != null)
                            {
                                strReturn = Utils_xml.GetNodeAttribute(xmlNode, "BrowseName");
                                strReturn = Utils_xml.FindChildDisplayName(node, strReturn);
                            }
                        }
                    }
                }
            }

            return strReturn;
        }

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

        public void GenDocumentation()
        {
            if (listOutputObjectDetails != null)
            {
                foreach (UAObjectType ua in listOutputObjectDetails)
                {
                    // This following is contained within a table that is 2 columns wide by 3 columns high
                    string strIsAbstract = (ua.bIsAbstract) ? "True" : "False";
                    Utils.Debug_WriteLine(Model.bDumpDocDetails, $"Browse Name:{ua.strBrowseName}");
                    Utils.Debug_WriteLine(Model.bDumpDocDetails, $"IsAbstract:{strIsAbstract}");

                    // The following row holds titles for the rest of the table. This one row is 5 columns wide
                    Utils.Debug_WriteLine(Model.bDumpDocDetails, $"{astrReferenceColumnTitles[0],-12}\t{astrReferenceColumnTitles[1],-16}\t{astrReferenceColumnTitles[2],-32}\t{astrReferenceColumnTitles[3],-36}\t{astrReferenceColumnTitles[4],-40}\t{astrReferenceColumnTitles[5],-12}");

                    // The following row is one column wide and one column high. Just important information.
                    Utils.Debug_WriteLine(Model.bDumpDocDetails, $"Sub Type:{ua.strSubType}");

                    // The following details are in a table that is five columns wide with as many rows as we need.
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
                    Utils.Debug_WriteLine(Model.bDumpDocDetails, $"Category:{ua.strCategory}");


                    // And now, we leave room for the documentation that someone is going to write about all of the items that are referenced above
                    if (ua.references != null)
                    {
                        foreach (UAReference uar in ua.references)
                        {
                            if (uar != null)
                            {
                                if (uar.strReferenceType != "HasSubtype")
                                    Utils.Debug_WriteLine(Model.bDumpDocDetails, $"{uar.strBrowseName,-32}");
                            }
                        }
                    }


                } // foreach (UAObjectType ua in listOutputObjectDetails)
            }
        }

        private static string[] astrReferenceColumnTitles =
            {
                "References",
                "Node Class",
                "BrowseName",
                "Data Type",
                "Type Definition",
                "Modelling Rule"
                };

        public static void DumpDocumentationDetails (List<UAObjectType> listOutputObjectDetails)
        {
            // Dump out the data we have accumulated:
            int iItem = 1;
            int cObjects = listOutputObjectDetails.Count;
            foreach (UAObjectType ua in listOutputObjectDetails)
            {
                string strIsAbstract = (ua.bIsAbstract) ? "True" : "False";
                Utils.Debug_WriteLine(Model.bDumpDocDetails, $"=====================================================");
                Utils.Debug_WriteLine(Model.bDumpDocDetails, $"UA Object # {iItem} / {cObjects}");
                Utils.Debug_WriteLine(Model.bDumpDocDetails, $"Browse Name:{ua.strBrowseName}");
                Utils.Debug_WriteLine(Model.bDumpDocDetails, $"IsAbstract:{strIsAbstract}");
                // Utils.Debug_WriteLine(Model.bDumpDocDetails, $"Base Type:{ua.strBaseType}");
                Utils.Debug_WriteLine(Model.bDumpDocDetails, $"Sub Type:{ua.strSubType}");
                Utils.Debug_WriteLine(Model.bDumpDocDetails, $"Category:{ua.strCategory}");

                Utils.Debug_WriteLine(Model.bDumpDocDetails, $"{astrReferenceColumnTitles[0],-12}\t{astrReferenceColumnTitles[1],-16}\t{astrReferenceColumnTitles[2],-32}\t{astrReferenceColumnTitles[3],-36}\t{astrReferenceColumnTitles[4],-40}\t{astrReferenceColumnTitles[5],-12}");
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
                iItem++;
            }
        }



    } // class
} // namespace
