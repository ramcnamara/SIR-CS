using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SIR_CS
{
    public partial class SIRSchemeForm
    {
        private SIRTreeNode draggedNode = null;
        private int overUnder;
        private SIRTreeNode lastOver;


        private class SIRTreeNode : System.Windows.Forms.TreeNode
        {
            public MarkType Mark { get; private set; }
            public MarkPanel Panel { get; private set; }
            
            public SIRTreeNode(MarkType newMark, string text, MarkPanel mp) : base(text)
            {
                Mark = newMark;
                Panel = mp;

                RefreshNodeDisplay();

                // wire up events
                if (Panel != null && !(Mark is CriterionType))
                {
                    Panel.TextChangeHandler += new EventHandler(OnChange);
                }
            }

            public void OnChange(object sender, EventArgs e)
            {
                System.Diagnostics.Debug.WriteLine($"OnChange handler: {Mark.Name}");
                RefreshNodeDisplay();

                // if the max mark changed, we need to propagate
                SIRTreeNode ancestor = Parent as SIRTreeNode;
                while (ancestor != null)
                {
                    ancestor.RefreshNodeDisplay();
                    ancestor = ancestor.Parent as SIRTreeNode;
                }
            }

            private void RefreshNodeDisplay()
            {
                bool bonus = false;
                bool penalty = false;
                if (Mark == null) return;
                string desc = "";
                if (Mark is CriterionType)
                {
                    Text = Mark.Name;
                    ImageIndex = 6;
                    SelectedImageIndex = 6;
                    return;
                }
                if (Mark is QualitativeType)
                    desc = "(qualitative) ";
                else if (Mark is NumericType)
                { 
                    desc = $"({((NumericType)Mark).GetTotalMaxMark()} marks) ";
                }


                desc += Panel.GetTaskName();

                // handle flags
                string flags = (Mark.groupSpecified && Mark.group) ? "(group" : ""; ;

                if (Mark is NumericType task)
                {

                    if (task.bonusSpecified && task.bonus)
                    {
                        if (flags == "")
                            flags = "(bonus";
                        else flags = "group, bonus";
                        bonus = true;
                    }

                    if (task.penaltySpecified && task.penalty)
                    {
                        if (flags == "")
                            flags = "(penalty";
                        else flags += ", penalty";
                        penalty = true;
                    }

                    if (flags != "")
                        flags += ")";
                }

                Text = $"{desc} {flags}";

                // Set the image icon.
                if (Mark is CriterionType)
                {
                    System.Diagnostics.Debug.WriteLine($"{Mark.Name} is a Criterion");
                    ImageIndex = 6;
                }
                else if (Mark.groupSpecified && Mark.group)
                {
                    if (bonus)
                        ImageIndex = 1;
                    else if (penalty)
                        ImageIndex = 2;
                    else
                        ImageIndex = 0;
                }
                else if (bonus)
                    ImageIndex = 4;
                else if (penalty)
                    ImageIndex = 5;
                else
                    ImageIndex = 3;

                SelectedImageIndex = ImageIndex;
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
        }

        private void TreeView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            // Drag on left button
            if (e.Button == MouseButtons.Left)
            {
                draggedNode = e.Item as SIRTreeNode;
                System.Diagnostics.Debug.Write("Dragging ");
                System.Diagnostics.Debug.WriteLine((draggedNode == null) ? "null" : draggedNode.Mark.Name);
                DoDragDrop(((SIRTreeNode)e.Item).Mark, DragDropEffects.Move | DragDropEffects.Scroll);
            }
        }

        private void TreeView_DragEnter(object sender, DragEventArgs e)
        {
            // TODO: draw indicator lines for sibling/parent insert.
            e.Effect = e.AllowedEffect;
        }

        private void TreeView_DragOver(object sender, DragEventArgs e)
        {
            // Find node that the mouse pointer is pointing at.
            Point targetPoint = treeView.PointToClient(new Point(e.X, e.Y));
            SIRTreeNode targetNode = treeView.GetNodeAt(targetPoint) as SIRTreeNode;

            bool dropOver = false;
            bool dropUnder = false;

            SIRTreeNode dest = targetNode;

            // Pointing at top or bottom of node?
            int offsetY = treeView.PointToClient(Cursor.Position).Y - targetNode.Bounds.Top;

            // Can't drop into Criteria, so over or under are the only choices.
            if (targetNode.Mark is CriterionType && draggedNode.Mark is CriterionType)
            {
                if (offsetY < targetNode.Bounds.Height / 2)
                    dropOver = true;
                else dropUnder = true;

                dest = (SIRTreeNode)targetNode.Parent;
            }

            // We are not over a Criterion.
            else if (offsetY < targetNode.Bounds.Height / 3)
            {
                dropOver = true;
                dest = (SIRTreeNode)targetNode.Parent;
            }
            else if (offsetY > (targetNode.Bounds.Height * 2) / 3)
            {
                dropUnder = true;
                dest = (SIRTreeNode)targetNode.Parent;
            }

            int newDrop = 0;
            if (dropOver) newDrop++;
            if (dropUnder) newDrop--;

            if (targetNode == lastOver && newDrop == overUnder)
                return;

            lastOver = targetNode;
            overUnder = newDrop;

            Refresh();


            // Draw the separator
            if (dropOver)
            {
                DrawLeafTopPlaceholders(targetNode, CanDropOn(draggedNode, dest));
            }

            else if (dropUnder)
            {
                DrawLeafBottomPlaceholders(targetNode, targetNode.Parent as SIRTreeNode, CanDropOn(draggedNode, dest));
            }

            else if (CanDropOn(draggedNode, targetNode))
            {
                DrawAddToFolderPlaceholder(targetNode, CanDropOn(draggedNode, dest));
            }
        }

        private void TreeView_DragDrop(object sender, DragEventArgs e)
        {
            // Retrieve the client coordinates of the drop location.
            Point targetPoint = treeView.PointToClient(new Point(e.X, e.Y));

            // Retrieve the node at the drop location.
            SIRTreeNode targetNode = treeView.GetNodeAt(targetPoint) as SIRTreeNode;


            // Confirm that the node at the drop location is not 
            // the dragged node or a descendant of the dragged node.
            if (!draggedNode.Equals(targetNode) && CanDropOn(draggedNode, targetNode))
            {
                // If it is a move operation, remove the node from its current 
                // location and add it to the node at the drop location.
                if ((e.Effect | DragDropEffects.Move) == DragDropEffects.Move)
                {

                    // The only way the dragged node's parent can be null is if we 
                    // dragged root, and that's not allowed.
                    if (!(draggedNode.Parent is SIRTreeNode oldParent))
                        return;

                    // Are we moving criteria?
                    if (draggedNode.Mark is CriterionType)
                    {
                        InsertIntoCriteria(draggedNode.Mark as CriterionType, targetNode.Mark, 0);
                        DeleteCriterion(draggedNode.Mark as CriterionType, oldParent.Mark);
                        // TODO: recreate old parent
                        // TODO: recreate targetNode
                        return;
                    }

                    // Are we dragging to the root?
                    if (oldParent == treeView.Nodes[0])
                    {
                        List<MarkType> tasks = formScheme.Tasks.ToList();
                        tasks.Insert(0, draggedNode.Mark);
                        // TODO: recreate entire tree
                        return;
                    }

                    // We are dragging a numeric or qualitative task to a subtask position.
                    InsertIntoSubtask(draggedNode.Mark, targetNode.Mark, 0);
                    DeleteSubtask(draggedNode.Mark, oldParent.Mark);

                    // TODO: recreate old parent
                    // TODO: recreate 
                }

            }
        }

        #region Methods for manipulating model classes to achieve drag and drop reordering
        private void DeleteCriterion(CriterionType c, dynamic mark)
        {
            List<CriterionType> criteria = mark.Criteria.ToList();
            criteria.Remove(c);
            mark.Criteria = criteria.ToArray();
        }

        private void InsertIntoCriteria(CriterionType c, dynamic mark, int pos)
        {
            List<CriterionType> criteria = mark.Criteria.ToList();
            criteria.Insert(pos, c);
            mark.Criteria = criteria.ToArray();
        }

        private void DeleteSubtask(MarkType m, dynamic mark)
        {
            List<MarkType> marks = mark.Subtasks.ToList();
            marks.Remove(m);
            mark.Subtasks = marks.ToArray();
        }

        private void InsertIntoSubtask(MarkType m, dynamic mark, int pos)
        {
            List<MarkType> marks = mark.Subtasks.ToList();
            marks.Insert(pos, m);
            mark.Tasks = marks.ToArray();
        }
        #endregion
   


        // Can node1 be dropped onto node2?
        private bool CanDropOn(SIRTreeNode node1, SIRTreeNode node2)
        {
          
            if (node1 == null || node2 == null)
            {
                return false;
            }

            // Only Criteria can't be dropped on the root
            if (node2 == treeView.Nodes[0]) 
                return !(node1.Mark is CriterionType);

            // Nothing can be dropped on Criteria
            if (node2.Mark is CriterionType)
                return false;

            // Otherwise, check for identity or  parent relationships
            if (node2.Parent == null) return true;
            if (node2.Parent.Equals(node1)) return false;

            // If the parent node is not null or equal to the first node, 
            // recurse.
            return CanDropOn(node1, node2.Parent as SIRTreeNode);
        }

        #region Methods for drawing images on treeView
        private void DrawLeafTopPlaceholders(SIRTreeNode NodeOver, bool possible)
        {
            Graphics g = this.treeView.CreateGraphics();

            Color color = possible ? Color.Black : Color.LightGray;
            int NodeOverImageWidth =  this.treeView.ImageList.Images[NodeOver.ImageIndex].Size.Width + 8;
            int LeftPos = NodeOver.Bounds.Left - NodeOverImageWidth;
            int RightPos = this.treeView.Width - 4;

            Point[] LeftTriangle = new Point[5]{
                                                   new Point(LeftPos, NodeOver.Bounds.Top - 4),
                                                   new Point(LeftPos, NodeOver.Bounds.Top + 4),
                                                   new Point(LeftPos + 4, NodeOver.Bounds.Y),
                                                   new Point(LeftPos + 4, NodeOver.Bounds.Top - 1),
                                                   new Point(LeftPos, NodeOver.Bounds.Top - 5)};

            Point[] RightTriangle = new Point[5]{
                                                    new Point(RightPos, NodeOver.Bounds.Top - 4),
                                                    new Point(RightPos, NodeOver.Bounds.Top + 4),
                                                    new Point(RightPos - 4, NodeOver.Bounds.Y),
                                                    new Point(RightPos - 4, NodeOver.Bounds.Top - 1),
                                                    new Point(RightPos, NodeOver.Bounds.Top - 5)};


            g.FillPolygon(Brushes.Black, LeftTriangle);
            g.FillPolygon(Brushes.Black, RightTriangle);
            g.DrawLine(new Pen(Color.Black, 2), new Point(LeftPos, NodeOver.Bounds.Top), new Point(RightPos, NodeOver.Bounds.Top));

        }//eom

        private void DrawLeafBottomPlaceholders(SIRTreeNode NodeOver, SIRTreeNode ParentDragDrop, bool possible)
        {
            Graphics g = this.treeView.CreateGraphics();

            int NodeOverImageWidth = this.treeView.ImageList.Images[NodeOver.ImageIndex].Size.Width + 8;
            // Once again, we are not dragging to node over, draw the placeholder using the ParentDragDrop bounds
            int LeftPos, RightPos;
            if (ParentDragDrop != null && ParentDragDrop.ImageIndex != -1)
                LeftPos = ParentDragDrop.Bounds.Left - (treeView.ImageList.Images[ParentDragDrop.ImageIndex].Size.Width + 8);
            else
                LeftPos = NodeOver.Bounds.Left - NodeOverImageWidth;
            RightPos = this.treeView.Width - 4;
            Color color = possible ? Color.Black : Color.Gray;
            Brush brush = new SolidBrush(color);
            Point[] LeftTriangle = new Point[5]{
                                                   new Point(LeftPos, NodeOver.Bounds.Bottom - 4),
                                                   new Point(LeftPos, NodeOver.Bounds.Bottom + 4),
                                                   new Point(LeftPos + 4, NodeOver.Bounds.Bottom),
                                                   new Point(LeftPos + 4, NodeOver.Bounds.Bottom - 1),
                                                   new Point(LeftPos, NodeOver.Bounds.Bottom - 5)};

            Point[] RightTriangle = new Point[5]{
                                                    new Point(RightPos, NodeOver.Bounds.Bottom - 4),
                                                    new Point(RightPos, NodeOver.Bounds.Bottom + 4),
                                                    new Point(RightPos - 4, NodeOver.Bounds.Bottom),
                                                    new Point(RightPos - 4, NodeOver.Bounds.Bottom - 1),
                                                    new Point(RightPos, NodeOver.Bounds.Bottom - 5)};


            g.FillPolygon(brush, LeftTriangle);
            g.FillPolygon(brush, RightTriangle);
            g.DrawLine(new Pen(color, 2), new Point(LeftPos, NodeOver.Bounds.Bottom), new Point(RightPos, NodeOver.Bounds.Bottom));
        }//eom

        private void DrawFolderTopPlaceholders(SIRTreeNode NodeOver, bool possible)
        {
            Graphics g = this.treeView.CreateGraphics();
            int NodeOverImageWidth = this.treeView.ImageList.Images[NodeOver.ImageIndex].Size.Width + 8;

            int LeftPos, RightPos;
            LeftPos = NodeOver.Bounds.Left - NodeOverImageWidth;
            RightPos = this.treeView.Width - 4;
            Color color = possible ? Color.Black : Color.Gray;
            Brush brush = new SolidBrush(color);

            Point[] LeftTriangle = new Point[5]{
                                                   new Point(LeftPos, NodeOver.Bounds.Top - 4),
                                                   new Point(LeftPos, NodeOver.Bounds.Top + 4),
                                                   new Point(LeftPos + 4, NodeOver.Bounds.Y),
                                                   new Point(LeftPos + 4, NodeOver.Bounds.Top - 1),
                                                   new Point(LeftPos, NodeOver.Bounds.Top - 5)};

            Point[] RightTriangle = new Point[5]{
                                                    new Point(RightPos, NodeOver.Bounds.Top - 4),
                                                    new Point(RightPos, NodeOver.Bounds.Top + 4),
                                                    new Point(RightPos - 4, NodeOver.Bounds.Y),
                                                    new Point(RightPos - 4, NodeOver.Bounds.Top - 1),
                                                    new Point(RightPos, NodeOver.Bounds.Top - 5)};


            g.FillPolygon(brush, LeftTriangle);
            g.FillPolygon(brush, RightTriangle);
            g.DrawLine(new Pen(Color.Black, 2), new Point(LeftPos, NodeOver.Bounds.Top), new Point(RightPos, NodeOver.Bounds.Top));

        }//eom
        private void DrawAddToFolderPlaceholder(SIRTreeNode NodeOver, bool possible)
        {
            Graphics g = treeView.CreateGraphics();
            int RightPos = NodeOver.Bounds.Right + 6;
            Color color = possible ? Color.Black : Color.Gray;
            Brush brush = new SolidBrush(color);
            Point[] RightTriangle = new Point[5]{
                                                    new Point(RightPos, NodeOver.Bounds.Y + (NodeOver.Bounds.Height / 2) + 4),
                                                    new Point(RightPos, NodeOver.Bounds.Y + (NodeOver.Bounds.Height / 2) + 4),
                                                    new Point(RightPos - 4, NodeOver.Bounds.Y + (NodeOver.Bounds.Height / 2)),
                                                    new Point(RightPos - 4, NodeOver.Bounds.Y + (NodeOver.Bounds.Height / 2) - 1),
                                                    new Point(RightPos, NodeOver.Bounds.Y + (NodeOver.Bounds.Height / 2) - 5)};


            g.FillPolygon(brush, RightTriangle);
        }//eom

        #endregion
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
            if (bonus == task.bonus && penalty == task.penalty)
                totalMaxMark += task.GetTotalMaxMark();
        }

        return totalMaxMark;
    }
}
