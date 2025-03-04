using Microsoft.Office.Interop.Word;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Point = System.Drawing.Point;

namespace NodeSetDoctor
{
    internal class Utils
    {
        public static void CenterWindowOverApp(Control ctrlWindow)
        {
            Size szChildSize = ctrlWindow.Size;

            Point ptParentLocation = new Point();
            Size szParent = new Size();

            if (FetchAppWindowsLocation(ctrlWindow, ref ptParentLocation, ref szParent))
            {
                int xParentCenter = ptParentLocation.X + (szParent.Width / 2);
                int yParentCenter = ptParentLocation.Y + (szParent.Height / 2);

                int xChildNew = xParentCenter - (szChildSize.Width / 2);
                int yChildNew = yParentCenter - (szChildSize.Height / 2);

                Point ptChildNewLocation = new Point(xChildNew, yChildNew);

                //Screen scr = Screen.FromPoint(ptChildNewLocation);

                // Ask the system for help figuring out which screen the parent resides on.
                Point ptParentCenter = new Point(xParentCenter, yParentCenter);
                Screen scr = Screen.FromPoint(ptParentCenter);

                // Adjust to make sure that lower-right corner is in bounds to the nearest screen.
                int cxRight = scr.Bounds.X + scr.Bounds.Width;
                if (ptChildNewLocation.X + szChildSize.Width > cxRight)
                    ptChildNewLocation.X = cxRight - szChildSize.Width;

                int cyBottom = scr.Bounds.Y + scr.Bounds.Height;
                if (ptChildNewLocation.Y + szChildSize.Height > cyBottom)
                    ptChildNewLocation.Y = cyBottom - szChildSize.Height;

                // Adjust to make sure that upper-left corner is in bounds to the nearest screen.
                if (ptChildNewLocation.X < scr.Bounds.X)
                    ptChildNewLocation.X = scr.Bounds.X;

                if (ptChildNewLocation.Y < scr.Bounds.Y)
                    ptChildNewLocation.Y = scr.Bounds.Y;

                ctrlWindow.Location = ptChildNewLocation;
            }
        }

        private static bool FetchAppWindowsLocation(Control ctrlWindow, ref Point ptParentLocation, ref Size szParent)
        {
            int cLogicalInch = 0;
            bool bSuccess = false;

            using (Graphics g = ctrlWindow.CreateGraphics())
            {
                cLogicalInch = (int)g.DpiX;
            }

            // object comActiveWindow = AddinModule.g_AddinModule.GetActiveWindow();
            object comActiveWindow = GetActiveWindow();
            if (comActiveWindow != null)
            {
                try
                {
                    // In Word
                    Window w = (Window)comActiveWindow;
                    ptParentLocation.X = PointToPixels((int)w.Left, cLogicalInch);
                    ptParentLocation.Y = PointToPixels((int)w.Top, cLogicalInch);
                    szParent.Height = PointToPixels((int)w.Height, cLogicalInch);
                    szParent.Width = PointToPixels((int)w.Width, cLogicalInch);
                    bSuccess = true;
                }
                catch
                {
                }

                Utils.ComRelease(comActiveWindow);
            }

            return bSuccess;
        }

        /// <summary>
        /// GetActiveWindow
        /// </summary>
        /// <returns></returns>
        private static object GetActiveWindow()
        {
            object comActiveWindow = ThisAddIn.g_WordApp.ActiveWindow;
            return comActiveWindow;
        }

        public static int ComRelease(object comObject)
        {
            int cRefCount = -1;
            if (comObject != null)
            {
                cRefCount = Marshal.ReleaseComObject(comObject);
            }

            return cRefCount;
        }

        public static int PointToPixels(int cPoint, int cLogicalInch)
        {
            return (cPoint * cLogicalInch / 72);
        }

    } // class
} // namespace NodeSetDoctor
