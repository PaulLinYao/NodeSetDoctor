using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System;
using NodeSetDoctor;

namespace XmlScan
{
    public partial class formMain : Form
    {
        public formMain()
        {
            InitializeComponent();
            InitOwnerDraw();
        }

        private Brush brushForeground = new SolidBrush(System.Drawing.Color.AliceBlue);
        private Brush brushBackground = new SolidBrush(System.Drawing.Color.AliceBlue);
        private Font fntOwnerDrawFixed = new Font("Courier New", 12, FontStyle.Regular);
        private StringFormat stringFormat = new StringFormat();

        private void InitOwnerDraw()
        {
            Graphics g = this.CreateGraphics();

            brushForeground = new SolidBrush(this.ForeColor);
            brushBackground = new SolidBrush(this.BackColor);

            // Set up tabstops for our owner draw combo control
            stringFormat = new StringFormat();
            SizeF sfUrl = g.MeasureString("http://opcfoundation.org/UA/SomeSpace", fntOwnerDrawFixed);
            SizeF sfVer = g.MeasureString("V:11.05.03.22.44   ", fntOwnerDrawFixed);
            SizeF sfDate = g.MeasureString("Pub:2024-10-07T10:24:02Z", fntOwnerDrawFixed);
            float[] tabStops = { sfUrl.Width, sfVer.Width, sfDate.Width }; // Sets the tab stops at 100 pixels each
            stringFormat.SetTabStops(0, tabStops);

            // Set the height of the combo box to the height of the font
            this.comboNodeSets.ItemHeight = (int)sfUrl.Height;

            g.Dispose();
        }

        private void formMain_Load(object sender, EventArgs e)
        {
            // Initialize the status text
            FileLoadStatusShow(true, true, true, false);
            textFileLoadStatus.Text = "Select the root folder where the NodeSet files are located.";
            cmdLoadFiles.Enabled = false;
            cmdGenDoc.Enabled = false;

            // Center the form over Microsoft Word main window
            NodeSetDoctor.Utils.CenterWindowOverApp(this);
        }

        private void cmdSelectFolder_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                folderBrowserDialog.Description = "Select the Root Folder for NodeSet files";

                //// DEBUG HELPER -- Remove Before Shipping
                //// DEBUG HELPER -- Remove Before Shipping
                folderBrowserDialog.SelectedPath = @"C:\_aWork\2025\Opc_Ua\CCS_WELLS";

                DialogResult result = folderBrowserDialog.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
                {
                    textRootPath.Text = folderBrowserDialog.SelectedPath;

                    int cTotalFiles = NodeSet.CountAllFiles(folderBrowserDialog.SelectedPath);
                    if (cTotalFiles > 0)
                    {
                        textFileLoadStatus.Text = $"Found {cTotalFiles} XML files. Click Load NodeSet Files button to begin processing.";
                        cmdLoadFiles.Enabled = true;
                    }
                    else
                    {
                        textFileLoadStatus.Text = "No XML files found. Please select another root folder.";
                    }
                }
            }
        }

        private void cmdLoadFiles_Click(object sender, EventArgs e)
        {
            string strRootFolder = textRootPath.Text;

            if (string.IsNullOrEmpty(strRootFolder))
            {
                textFileLoadStatus.Text = "Please select a root folder for the NodeSet files.";
                return;
            }

            if (Directory.Exists(strRootFolder) == false)
            {
                textFileLoadStatus.Text = $"Error: root folder {strRootFolder} not found. Please select another root folder.";
                return;
            }

            cmdCancelFileLoad.Visible = true;
            cmdCancelFileLoad.Enabled = true;
            cmdCancelFileLoad.Refresh();

            bool bSuccess = NodeSet.LoadAllFiles(strRootFolder, textFileLoadStatus);

            int cModels = Model.g_AllModels.Count;
            int cMissing = Model.GetAllPlaceholderModels().Count;

            int cItems = 0;

            //// DEBUG HELPER -- Remove Before Shipping
            //// DEBUG HELPER -- Remove Before Shipping
            int iOpcUaNodeset = -1;

            if (cModels > 0)
            {
                comboNodeSets.Items.Clear();
                foreach (Model m in Model.g_AllModels.Values)
                {
                    var xx = new ComboBoxItem();
                    xx.Text = $"{m.ModelUri}\tV:{m.Version}\tPub:{m.PublicationDate}";
                    xx.Id = m.Id;
                    cItems++;
                    Utils.Debug_WriteLine(true, $"comboNodeSets.Items.Add\t{cItems}\t{xx.Text}");

                    int iAdded = comboNodeSets.Items.Add(xx);

                    //// DEBUG HELPER -- Remove Before Shipping
                    //// DEBUG HELPER -- Remove Before Shipping
                    if (m.ModelUri.Contains("opcfoundation.org"))
                    {
                        iOpcUaNodeset = iAdded;
                    }
                }

                //// DEBUG HELPER -- Remove Before Shipping
                //// DEBUG HELPER -- Remove Before Shipping
                comboNodeSets.SelectedIndex = iOpcUaNodeset;

                textFileLoadStatus.Text = $"Found {cModels} NodeSets. Select a NodeSet from the list and click Generate Documentation.";
                cmdGenDoc.Enabled = true;
            }
            else
            {
                textFileLoadStatus.Text = $"Error: No models found. Please select another folder to search.";
            }
        }

        private void cmdCancelFileLoad_Click(object sender, EventArgs e)
        {

        }

        private void comboNodeSets_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();

            if (e.Index >= 0 && comboNodeSets != null && comboNodeSets.Items != null)
            {
                string strText = comboNodeSets.Items[e.Index].ToString();

                if (strText != null)
                {
                    e.Graphics.FillRectangle(brushBackground, e.Bounds);

                    // Draw the item text
                    e.Graphics.DrawString(strText, fntOwnerDrawFixed, brushForeground, e.Bounds, stringFormat);
                }
            }
            e.DrawFocusRectangle();
        }


        private void cmdGenDocumentation_Click(object sender, EventArgs e)
        {
            bool bSuccess = false;
            var selected = comboNodeSets.SelectedItem;
            if (selected != null && selected is ComboBoxItem)
            {
                ComboBoxItem item = (ComboBoxItem)selected;
                if (item != null && item.Id != null) 
                {
                    Guid guid = (Guid)item.Id;

                    // Load up the data needed for the documentation.
                    bSuccess = NodeSet.LoadNodeSetData(guid);

                    if (Model.TryGetModel(guid, out Model m))
                    {
                        if (m != null && m.listOutputObjectDetails != null)
                        {
                            Word_Document.GenDocumentation(m);
                            bSuccess = true;
                        }
                    }
                }
            }

            if (!bSuccess)
            {
                textFileLoadStatus.Text = "Error: No NodeSet selected. Please select a NodeSet from the list.";
            }
        }


        private void FileLoadStatusShow(bool bShowParent, bool bShowText, bool bClearText, bool bShowButton)
        {
            if (bClearText)
            {
                textFileLoadStatus.Text = "";
            }

            textFileLoadStatus.Visible = bShowText;
            cmdCancelFileLoad.Visible = bShowButton;
        }

    } // class
    public class ComboBoxItem
    {
        public string Text { get; set; }
        public Guid Id { get; set; }

        // This override is used to display the text in the ComboBox
        public override string ToString()
        {
            return (Text != null ? Text : "");
        }
    }

} // namespace
