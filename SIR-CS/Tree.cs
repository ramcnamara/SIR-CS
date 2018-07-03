using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SIR_CS
{
    public partial class SIRSchemeForm
    {

        private class SIRTreeNode : System.Windows.Forms.TreeNode
        {
            public MarkType Mark { get; private set; }
            public MarkPanel Panel { get; private set; }

            public SIRTreeNode(MarkType newMark, string text, MarkPanel mp) : base(text)
            {
                Mark = newMark;
                Panel = mp;

                Recompute();

                // wire up events
                if (Panel != null && !(Mark is CriterionType))
                {
                    Panel.TextChangeHandler += new EventHandler(OnChange);
                }
            }

            public void OnChange(object sender, EventArgs e)
            {
                System.Diagnostics.Debug.WriteLine($"OnChange handler: {Mark.Name}");
                Recompute();

                // if the max mark changed, we need to propagate
                SIRTreeNode ancestor = Parent as SIRTreeNode;
                while (ancestor != null)
                {
                    ancestor.Recompute();
                    ancestor = ancestor.Parent as SIRTreeNode;
                }
            }

            private void Recompute()
            {
                if (Mark == null) return;
                string desc = "";
                if (Mark is CriterionType)
                {
                    Text = Mark.Name;
                    return;
                }
                if (Mark is QualitativeType)
                    desc = "(qualitative) ";
                else if (Mark is NumericType)
                {
                    // TODO: scrape from 
                    desc = $"({((NumericType)Mark).GetTotalMaxMark()} marks) ";
                }

                desc += Panel.GetTaskName();

                // handle flags
                string flags = Mark.group ? "(group" : ""; ;

                if (Mark is NumericType task)
                {

                    if (task.bonusSpecified && task.bonus)
                    {
                        if (flags == "")
                            flags = "(bonus";
                        else flags = "group, bonus";
                    }

                    if (task.penaltySpecified && task.penalty)
                    {
                        if (flags == "")
                            flags = "(penalty";
                        else flags += ", penalty";
                    }

                    if (flags != "")
                        flags += ")";
                }

                Text = $"{desc} {flags}";
            }
        }

        private void TreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (treeView.SelectedNode == null)
            {
                return;
            }
            if (!(treeView.SelectedNode is SIRTreeNode node))
            {
                return;
            };

            MarkPanel mp = node.Panel;
            if (mp == null) return;

            cardPanel.Controls.Clear();
            cardPanel.Controls.Add(mp);
            Refresh();


        }

        private void TreeView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            // Drag on left button
            if (e.Button == MouseButtons.Left)
                DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void TreeView_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.AllowedEffect;
        }

        private void TreeView_DragOver(object sender, DragEventArgs e)
        {
            // Retrieve the client coordinates of the mouse position.
            Point targetPoint = treeView.PointToClient(new Point(e.X, e.Y));

            // Highlight the node at the mouse position, if a suitable drop target.
            SIRTreeNode dest = treeView.GetNodeAt(targetPoint) as SIRTreeNode;
            SIRTreeNode payload = (SIRTreeNode)e.Data.GetData(typeof(SIRTreeNode));
            if (CanDropOn(payload, dest))
            {
                dest.BackColor = SystemColors.Highlight;
                dest.ForeColor = SystemColors.HighlightText;
            }

            // Unhighlight any node we just left
            if (dest.PrevVisibleNode != null)
            {
                dest.PrevVisibleNode.BackColor = SystemColors.ControlLightLight;
                dest.PrevVisibleNode.ForeColor = SystemColors.ControlText;
            }

            if (dest.NextVisibleNode != null)
            {
                dest.NextVisibleNode.BackColor = SystemColors.ControlLightLight;
                dest.NextVisibleNode.ForeColor = SystemColors.ControlText;
            }
        }

        private void TreeView_DragDrop(object sender, DragEventArgs e)
        {
            // Retrieve the client coordinates of the drop location.
            Point targetPoint = treeView.PointToClient(new Point(e.X, e.Y));

            // Retrieve the node at the drop location.
            SIRTreeNode targetNode = treeView.GetNodeAt(targetPoint) as SIRTreeNode;

            // Retrieve the node that was dragged.
            SIRTreeNode draggedNode = (SIRTreeNode)e.Data.GetData(typeof(SIRTreeNode));

            // Confirm that the node at the drop location is not 
            // the dragged node or a descendant of the dragged node.
            if (!draggedNode.Equals(targetNode) && CanDropOn(draggedNode, targetNode))
            {
                // If it is a move operation, remove the node from its current 
                // location and add it to the node at the drop location.
                if (e.Effect == DragDropEffects.Move)
                {
                    draggedNode.Remove();
                    targetNode.Nodes.Add(draggedNode);
                }

                // If it is a copy operation, clone the dragged node 
                // and add it to the node at the drop location.
                else if (e.Effect == DragDropEffects.Copy)
                {
                    targetNode.Nodes.Add((TreeNode)draggedNode.Clone());
                }

                // Expand the node at the location 
                // to show the dropped node.
                targetNode.Expand();
            }
        }

        // Can node1 be dropped onto node2?
        private bool CanDropOn(SIRTreeNode node1, SIRTreeNode node2)
        {
            // Nothing can be dropped on Criteria
            if (node2.Mark is CriterionType)
                return false;

            // Only Criteria can't be dropped on the root
            if (node2 == treeView.Nodes[0])
                return !(node1.Mark is CriterionType);

            // Otherwise, check for identity or  parent relationships
            if (node2.Parent == null) return true;
            if (node2.Parent.Equals(node1)) return false;

            // If the parent node is not null or equal to the first node, 
            // recurse.
            return CanDropOn(node1, node2.Parent as SIRTreeNode);
        }
    }
}

public partial class NumericType
{
    public decimal GetTotalMaxMark()
    {
        if (subtasksField == null || subtasksField.Length == 0)
            return maxMark;

        decimal totalMaxMark = 0;
        foreach (NumericType task in subtasksField.Where(st => st is NumericType))
        {
            totalMaxMark += task.GetTotalMaxMark();
        }

        return totalMaxMark;
    }
}
