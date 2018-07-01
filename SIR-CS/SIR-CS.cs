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
            System.IO.FileStream infile = null;
            try
            {
                infile = new System.IO.FileStream(myFile, System.IO.FileMode.Open);
            }
            catch (System.IO.IOException ee)
            {
                MessageBox.Show("There was a problem reading from that file.  " + 
                    "Check that it is not being used by another program, and that " +
                    "you have permission to open it.  \n\nTechnical details: " + ee.Message, 
                    "IO exception on load",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
            XmlReader reader = XmlReader.Create(infile);

            Scheme newScheme;
            newScheme = (Scheme)serializer.Deserialize(reader);
            // TODO: check that deserialization worked
            reader.Close();

            // create MDI child window holding opened marking scheme
            SIRSchemeForm newChild = new SIRSchemeForm(newScheme, myFile)
            {
                MdiParent = this
            };
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

        private void saveToolStripMenuItem_Click(object sender, EventArgs e) => ((SIRSchemeForm)ActiveMdiChild).Save();

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Filter = "MADAM marking schemes (*.xml)|*.xml|All files (*.*)|*.*";
            dlg.FilterIndex = 1;
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                ((SIRSchemeForm)ActiveMdiChild).SaveAs(dlg.FileName);
            }
        }
    }
}
