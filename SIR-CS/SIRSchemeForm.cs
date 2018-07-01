using System;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace SIR_CS
{
    public partial class SIRSchemeForm : Form
    {
        private Scheme myScheme;
        private string myFileName;
        private bool changedSinceSave = false;


        public SIRSchemeForm(Scheme newScheme)
        {

            // populate marking scheme metadata edit boxes
            this.myScheme = newScheme;
            InitializeComponent();
            unitCodeBox.DataBindings.Add(new Binding("Text", myScheme, "UnitCode"));
            activityNameBox.DataBindings.Add(new Binding("Text", myScheme, "ActivityName"));
            subtitleBox.DataBindings.Add(new Binding("Text", myScheme, "Subtitle"));
            preambleBox.DataBindings.Add(new Binding("Text", myScheme, "Preamble"));

            // populate tree selector
            // WinForms TreeViews don't do databinding so this must be done by hand
            SIRTreeNode rootNode = new SIRTreeNode(null, myScheme.ActivityName, null);
            treeView.Nodes.Add(rootNode);
            if (myScheme.Tasks != null)
                foreach (var task in myScheme.Tasks)
                {
                    Traverse(rootNode, task);
                }

            // Enable drag/drop reorder
            treeView.ItemDrag += new ItemDragEventHandler(TreeView_ItemDrag);
            treeView.DragEnter += new DragEventHandler(TreeView_DragEnter);
            treeView.DragDrop += new DragEventHandler(TreeView_DragDrop);
            treeView.DragOver += new DragEventHandler(TreeView_DragOver);

            treeView.ExpandAll();
        }

        // <summary>
        // Recurse down the tree of tasks and subtasks.  Criteria are handled
        // separately despite being descendants of MarkType because they never have
        // subtasks of their own.
        // </summary>
        private void Traverse(SIRTreeNode parent, dynamic mark)
        {
            MarkPanel mp = new MarkPanel(mark);
            SIRTreeNode newNode = new SIRTreeNode(mark, mark.Name, mp);
            parent.Nodes.Add(newNode);

            if (mark.Criteria != null)
            {
                foreach (CriterionType criterion in mark.Criteria)
                {
                    parent.Nodes.Add(new SIRTreeNode(criterion, criterion.Name, mp));
                }
            }
            if (mark.Subtasks != null)
            {
                foreach (dynamic subtask in mark.Subtasks)
                {
                    Traverse(parent, subtask);
                }
            }
        }



        public SIRSchemeForm(Scheme newScheme, string fileName) : this(newScheme)
        {
            this.myFileName = fileName;
            Text = fileName;
        }

        internal void Save()
        {
            SaveAs(myFileName);
        }

        internal void SaveAs(string newFileName)
        {
            // TODO: check that file open succeeded
            XmlSerializer serializer = new XmlSerializer(typeof(Scheme));
            XmlWriter writer;
            try
            {
                writer = XmlWriter.Create(newFileName);
            }
            catch (System.IO.IOException e)
            {
                MessageBox.Show("There was a problem writing to that file.  " +
                   "Check that it is not being used by another program, and that " +
                   "you have permission to write to it.  \n\nTechnical details: " + e.Message,
                    "IO exception on save",
                   MessageBoxButtons.OK,
                   MessageBoxIcon.Error);
                return;
            }
            serializer.Serialize(writer, myScheme);
            writer.Close();

            myFileName = newFileName;
            Text = newFileName;
        }
    }
}


