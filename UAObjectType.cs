using System;
using System.Collections.Generic;

namespace XmlScan
{
    public class UAObjectType
    {
        public UAObjectType() { }

        public string strBrowseName;
        public bool bIsAbstract;
        // public string? strBaseType;
        public string strSubType;
        public string strCategory;

        public List<UAReference> references = new List<UAReference>();

    } // class
} // namespace
