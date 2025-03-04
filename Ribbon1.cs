using Microsoft.Office.Tools.Ribbon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using XmlScan;

namespace NodeSetDoctor
{
    public partial class Ribbon1
    {
        private System.Windows.Forms.Form myForm = null;
        private void Ribbon1_Load(object sender, RibbonUIEventArgs e)
        {

        }

        private void cmdLoad_Click(object sender, RibbonControlEventArgs e)
        {
            if (myForm == null || myForm.IsDisposed)
            {
                myForm = new formMain();
                myForm.Show();
            }
            else
            {
                myForm.Show();
                myForm.BringToFront();
            }


            //using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            //{
            //    folderBrowserDialog.Description = "Select the Root Folder for NodeSet files";

            //    DialogResult result = folderBrowserDialog.ShowDialog();

            //    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
            //    {
            //        MessageBox.Show("You selected: " + folderBrowserDialog.SelectedPath);
            //    }
            //}
        }
    }
}
