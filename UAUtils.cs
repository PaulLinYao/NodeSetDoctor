using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlScan
{
    class Utils
    {
        public static void Debug_WriteLine(bool b, string str)
        {
            if (b)
                System.Diagnostics.Debug.WriteLine(str);
        }

    } // class
} // namespace
