using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace SIR_CS
{
    public partial class SIRMainForm : Form
    {
        public SIRMainForm()
        {
            InitializeComponent();
        }

        private void SIRMainForm_Load(object sender, EventArgs e)
        {
            IsMdiContainer = true;
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openMSdlg.FileName = "";
            openMSdlg.Filter = "MADAM marking schemes|*.xml|All files|*.*";
            openMSdlg.ShowDialog();
            string myFile = openMSdlg.FileName;

            // TODO: check that file open succeeded
            XmlSerializer serializer = new XmlSerializer(typeof(Scheme));
            System.IO.FileStream infile = new System.IO.FileStream(myFile, System.IO.FileMode.Open);
            XmlReader reader = XmlReader.Create(infile);

            Scheme newScheme;
            newScheme = (Scheme)serializer.Deserialize(reader);
            // TODO: check that deserialization worked
            infile.Close();

            // create MDI child window holding opened marking scheme
            SIRSchemeForm newChild = new SIRSchemeForm(newScheme, myFile)
            {
                MdiParent = this
            };
            newChild.Text = myFile;
            newChild.Show();
        }

        private void NewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Scheme newScheme = new Scheme();
            SIRSchemeForm newChild = new SIRSchemeForm(newScheme)
            {
                MdiParent = this
            };
            newChild.Text = "<New>";
            newChild.Show();
        }

        private void CascadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.LayoutMdi(MdiLayout.Cascade);
        }

        private void TileWindowsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.LayoutMdi(MdiLayout.TileHorizontal);
        }

        private void TileVerticallyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.LayoutMdi(MdiLayout.TileVertical);
        }
    }
}
