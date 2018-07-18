using System;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace SIR_CS
{
    public partial class SIRSchemeForm : Form
    {
        private Scheme formScheme;
        private string myFileName;
        private bool changedSinceSave = false;


        public SIRSchemeForm(Scheme newScheme)
        {

            // populate marking scheme metadata edit boxes
            this.formScheme = newScheme;
            InitializeComponent();
            unitCodeBox.DataBindings.Add(new Binding("Text", formScheme, "UnitCode"));
            activityNameBox.DataBindings.Add(new Binding("Text", formScheme, "ActivityName"));
            subtitleBox.DataBindings.Add(new Binding("Text", formScheme, "Subtitle"));
            preambleBox.DataBindings.Add(new Binding("Text", formScheme, "Preamble"));

            // watch for changes
            unitCodeBox.TextChanged += new EventHandler(SetDirty);
            activityNameBox.TextChanged += new EventHandler(SetDirty);
            subtitleBox.TextChanged += new EventHandler(SetDirty);
            preambleBox.TextChanged += new EventHandler(SetDirty);

            // Set up overlay for treeView drag/drop indicators.
            treeOverlay = treeView.CreateGraphics();

            // populate tree selector
            // WinForms TreeViews don't do databinding so this must be done by hand
            SIRTreeNode rootNode = new SIRTreeNode(null, formScheme.ActivityName, null)
            {
                ImageIndex = 7
            };
            treeView.Nodes.Add(rootNode);
            if (formScheme.Tasks != null)
                foreach (var task in formScheme.Tasks)
                {
                    CreateSubtree(rootNode, task);
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
        private void CreateSubtree(SIRTreeNode parent, dynamic mark)
        {
            MarkPanel mp = new MarkPanel(mark);
            SIRTreeNode newNode = new SIRTreeNode(mark, mark.Name, mp);
            parent.Nodes.Add(newNode);

            if (mark.Criteria != null)
            {
                foreach (CriterionType criterion in mark.Criteria)
                {
                    newNode.Nodes.Add(new SIRTreeNode(criterion, criterion.Name, mp));
                }
            }
            if (mark.Subtasks != null)
            {
                foreach (dynamic subtask in mark.Subtasks)
                {
                    CreateSubtree(newNode, subtask);
                }
            }
        }

        private SIRTreeNode FindNodeForMark(SIRTreeNode node, MarkType mark)
        {
            // Base case: not there.
            if (node == null)
                return null;

            // Base case: item found.
            if (node.Mark != null && node.Mark.Equals(mark))
            {
                return node;
            }

            // If found in a child node, pass reference up the call chain.
            foreach (SIRTreeNode child in node.Nodes)
            {
                SIRTreeNode foundNode = FindNodeForMark(child, mark);
                if (foundNode != null)
                    return foundNode;
            }

            // Mark was not found in any subtree.
            return null;
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
            serializer.Serialize(writer, formScheme);
            writer.Close();

            myFileName = newFileName;
            Text = newFileName;
            changedSinceSave = false;
        }

        private void SetDirty(object sender, EventArgs e) => changedSinceSave = true;

        private bool IsDirty()
        {
            if (changedSinceSave) return true;
 // TODO           if (treeView.IsDirty) return true;
            if (treeView == null || (treeView.Nodes == null))
                return false;

            return FindDirtyNodes((SIRTreeNode)treeView.Nodes[0]);
        }

        private void SetTreeToClean(SIRTreeNode node)
        {
            if (node == null) return;
            node.Panel.ChangedSinceSave = false;
            foreach(SIRTreeNode child in node.Nodes)
            {
                SetTreeToClean(child);
            }
        }
        private bool FindDirtyNodes(SIRTreeNode treeNode)
        {
            if (treeNode == null || treeNode.Panel == null)
                return false;

            if (treeNode.Panel.ChangedSinceSave)
                return true;

            foreach (SIRTreeNode node in treeNode.Nodes)
                if (FindDirtyNodes(node))
                    return true;

            return false;
        }

        public void CloseWithCheck()
        {
            if (IsDirty())
            {
                var choice = MessageBox.Show("You have unsaved changes.  Are you sure you want to close?",
                    "You have unsaved changes",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (choice == DialogResult.No)
                    return;
            }
            Close();
        }
    }
}


