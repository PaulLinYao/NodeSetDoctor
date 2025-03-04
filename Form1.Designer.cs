using System.Drawing;
using System.Windows.Forms;

namespace XmlScan
{
    partial class formMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            cmdSelectFolder = new Button();
            cmdLoadFiles = new Button();
            textRootPath = new TextBox();
            cmdGenDoc = new Button();
            tableLayoutPanel1 = new TableLayoutPanel();
            comboNodeSets = new ComboBox();
            tableLayoutPanel2 = new TableLayoutPanel();
            cmdCancelFileLoad = new Button();
            textFileLoadStatus = new TextBox();
            label1 = new Label();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            SuspendLayout();
            // 
            // cmdSelectFolder
            // 
            cmdSelectFolder.Anchor = AnchorStyles.Right;
            cmdSelectFolder.Location = new Point(71, 15);
            cmdSelectFolder.Name = "cmdSelectFolder";
            cmdSelectFolder.Size = new Size(107, 34);
            cmdSelectFolder.TabIndex = 0;
            cmdSelectFolder.Text = "Select Folder";
            cmdSelectFolder.UseVisualStyleBackColor = true;
            cmdSelectFolder.Click += cmdSelectFolder_Click;
            // 
            // cmdLoadFiles
            // 
            cmdLoadFiles.Anchor = AnchorStyles.None;
            cmdLoadFiles.Location = new Point(15, 79);
            cmdLoadFiles.Name = "cmdLoadFiles";
            cmdLoadFiles.Size = new Size(151, 34);
            cmdLoadFiles.TabIndex = 1;
            cmdLoadFiles.Text = "Load NodeSet Files";
            cmdLoadFiles.UseVisualStyleBackColor = true;
            cmdLoadFiles.Click += cmdLoadFiles_Click;
            // 
            // textRootPath
            // 
            textRootPath.Anchor = AnchorStyles.Left;
            textRootPath.Location = new Point(3, 301);
            textRootPath.Name = "textRootPath";
            textRootPath.Size = new Size(172, 27);
            textRootPath.TabIndex = 2;
            // 
            // cmdGenDoc
            // 
            cmdGenDoc.Anchor = AnchorStyles.None;
            cmdGenDoc.Location = new Point(647, 207);
            cmdGenDoc.Name = "cmdGenDoc";
            cmdGenDoc.Size = new Size(216, 34);
            cmdGenDoc.TabIndex = 4;
            cmdGenDoc.Text = "Generate Documentation";
            cmdGenDoc.UseVisualStyleBackColor = true;
            cmdGenDoc.Click += cmdGenDocumentation_Click;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 13.6090221F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 86.390976F));
            tableLayoutPanel1.Controls.Add(cmdSelectFolder, 0, 0);
            tableLayoutPanel1.Controls.Add(cmdLoadFiles, 0, 1);
            tableLayoutPanel1.Controls.Add(cmdGenDoc, 1, 3);
            tableLayoutPanel1.Controls.Add(comboNodeSets, 1, 2);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 1, 1);
            tableLayoutPanel1.Controls.Add(label1, 0, 2);
            tableLayoutPanel1.Location = new Point(12, 12);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 5;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 128F));
            tableLayoutPanel1.Size = new Size(1330, 324);
            tableLayoutPanel1.TabIndex = 5;
            // 
            // comboNodeSets
            // 
            comboNodeSets.Anchor = AnchorStyles.Left;
            comboNodeSets.DrawMode = DrawMode.OwnerDrawFixed;
            comboNodeSets.DropDownStyle = ComboBoxStyle.DropDownList;
            comboNodeSets.FormattingEnabled = true;
            comboNodeSets.Location = new Point(184, 146);
            comboNodeSets.Name = "comboNodeSets";
            comboNodeSets.Size = new Size(1083, 28);
            comboNodeSets.TabIndex = 5;
            comboNodeSets.DrawItem += comboNodeSets_DrawItem;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.Anchor = AnchorStyles.None;
            tableLayoutPanel2.ColumnCount = 2;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 83.31108F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.68892F));
            tableLayoutPanel2.Controls.Add(cmdCancelFileLoad, 1, 0);
            tableLayoutPanel2.Controls.Add(textFileLoadStatus, 0, 0);
            tableLayoutPanel2.Location = new Point(211, 75);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 1;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel2.Size = new Size(1088, 41);
            tableLayoutPanel2.TabIndex = 6;
            // 
            // cmdCancelFileLoad
            // 
            cmdCancelFileLoad.Anchor = AnchorStyles.None;
            cmdCancelFileLoad.Location = new Point(947, 3);
            cmdCancelFileLoad.Name = "cmdCancelFileLoad";
            cmdCancelFileLoad.Size = new Size(100, 34);
            cmdCancelFileLoad.TabIndex = 0;
            cmdCancelFileLoad.Text = "Cancel";
            cmdCancelFileLoad.UseVisualStyleBackColor = true;
            cmdCancelFileLoad.Click += cmdCancelFileLoad_Click;
            // 
            // textFileLoadStatus
            // 
            textFileLoadStatus.Anchor = AnchorStyles.None;
            textFileLoadStatus.BorderStyle = BorderStyle.FixedSingle;
            textFileLoadStatus.Location = new Point(14, 7);
            textFileLoadStatus.Name = "textFileLoadStatus";
            textFileLoadStatus.ReadOnly = true;
            textFileLoadStatus.Size = new Size(877, 27);
            textFileLoadStatus.TabIndex = 1;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Right;
            label1.AutoSize = true;
            label1.Location = new Point(36, 150);
            label1.Name = "label1";
            label1.Size = new Size(142, 20);
            label1.TabIndex = 7;
            label1.Text = "Available NodeSets:";
            // 
            // formMain
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1390, 392);
            Controls.Add(tableLayoutPanel1);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "formMain";
            Text = "NodeSet Doctor - Select Folder with NodeSet Files";
            Load += formMain_Load;
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Button cmdSelectFolder;
        private Button cmdLoadFiles;
        private TextBox textRootPath;
        private Button cmdGenDoc;
        private TableLayoutPanel tableLayoutPanel1;
        private ComboBox comboNodeSets;
        private TextBox textFileLoadStatus;
        private Button cmdCancelFileLoad;
        private TableLayoutPanel tableLayoutPanel2;
        private Label label1;
    }
}
