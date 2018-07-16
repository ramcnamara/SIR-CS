using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SIR_CS
{
    public partial class SIRSchemeForm
    {
        /// <summary>
        /// Event that fires when drag commences.  Stores the SIRTreeNode that is being 
        /// dragged into the current Form -- we would normally use e.Item for this, but
        /// although TreeNodes are serializable in C#, they're not in the OSX version of mono.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreeView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            // Drag on left button
            if (e.Button == MouseButtons.Left)
            {
                draggedNode = e.Item as SIRTreeNode;
                DoDragDrop(((SIRTreeNode)e.Item).Mark, DragDropEffects.Move);
            }
        }

        /// <summary>
        /// Event that fires when mouse enters a region while dragging.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreeView_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.AllowedEffect;
        }

        /// <summary>
        /// Event that fires while an item is dragged over the TreeView.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                if (targetNode.Mark is CriterionType)
                    DrawLeafTopPlaceholders(targetNode, CanDropOn(draggedNode, targetNode));
                else
                    DrawFolderTopPlaceholders(targetNode, CanDropOn(draggedNode, dest));
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

        /// <summary>
        /// Event that fires when mouse is released after a drag.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreeView_DragDrop(object sender, DragEventArgs e)
        {
            // Retrieve the client coordinates of the drop location.
            Point targetPoint = treeView.PointToClient(new Point(e.X, e.Y));

            // Retrieve the node at the drop location.
            SIRTreeNode targetNode = treeView.GetNodeAt(targetPoint) as SIRTreeNode;

            // Where should the drop go?
            SIRTreeNode dest = targetNode;
            int pos = 0;

            // Check for dropping over/under the node.
            if (targetNode != treeView.Nodes[0])
            {
                if (overUnder != 0)
                {
                    dest = dest.Parent as SIRTreeNode;
                    pos = targetNode.Index;
                    if (!(draggedNode.Mark is CriterionType) && dest.Mark != null)
                        pos -= dest.Mark.CriterionCount();
                    if (overUnder < 0)
                        pos--;
                }


                // correct for reordering siblings
                if (targetNode.Parent == draggedNode.Parent)
                {
                    int originalPos = draggedNode.Index;
                    if (!(draggedNode.Mark is CriterionType))
                        originalPos -= draggedNode.Mark.CriterionCount();
                    if (pos > originalPos)
                        pos++;
                }

            }
            else
            {
                dest = treeView.Nodes[0] as SIRTreeNode;
                pos = formScheme.Tasks.Length;
            }


            // Confirm that the node at the drop location is not 
            // the dragged node or a descendant of the dragged node.
            if (!draggedNode.Equals(targetNode) && CanDropOn(draggedNode, dest))
            {          
                if ((e.Effect | DragDropEffects.Move) == DragDropEffects.Move)
                {
                    // The only way the dragged node's parent can be null is if we 
                    // dragged root, and that's not allowed.
                    if (!(draggedNode.Parent is SIRTreeNode oldParent))
                    {
                        return;
                    }

                    // Are we moving criteria?
                    if (draggedNode.Mark is CriterionType)
                    {
                        DeleteCriterion(draggedNode.Mark as CriterionType, oldParent.Mark);
                        InsertIntoCriteria(draggedNode.Mark as CriterionType, dest.Mark, pos);
                        
                        // rebuild tree
                        treeView.Nodes[0].Nodes.Clear();
                        if (formScheme.Tasks != null)
                            foreach (var task in formScheme.Tasks)
                            {
                                CreateSubtree((SIRTreeNode)treeView.Nodes[0], task);
                            }
                        treeView.ExpandAll();
                        return;
                    }

                    // Are we dragging to the root?
                    if (dest == treeView.Nodes[0])
                    {
                        // Correct for dragging a top-level Task lower in the order.
                        if (oldParent == dest)
                            if (pos > draggedNode.Index)
                                pos--;
                        DeleteSubtask(draggedNode.Mark, oldParent.Mark);
                        List<MarkType> tasks = formScheme.Tasks.ToList();
                        tasks.Insert(pos, draggedNode.Mark);
                        formScheme.Tasks = tasks.ToArray<MarkType>();

                        // rebuild tree
                        treeView.Nodes[0].Nodes.Clear();
                        if (formScheme.Tasks != null)
                            foreach (var task in formScheme.Tasks)
                            {
                                CreateSubtree((SIRTreeNode)treeView.Nodes[0], task);
                            }
                        treeView.ExpandAll();
                        return;
                    }

                    // We are dragging a numeric or qualitative task to a subtask position.
                    InsertIntoSubtask(draggedNode.Mark, dest.Mark, pos);
                    DeleteSubtask(draggedNode.Mark, oldParent.Mark);

                    // rebuild tree
                    treeView.Nodes[0].Nodes.Clear();
                    if (formScheme.Tasks != null)
                        foreach (var task in formScheme.Tasks)
                        {
                            CreateSubtree((SIRTreeNode)treeView.Nodes[0], task);
                        }
                    treeView.ExpandAll();
                    return;
                }

            }
        }

        #region Methods for manipulating model classes to achieve drag and drop reordering
        private void DeleteCriterion(CriterionType c, dynamic mark)
        {
            if (mark.Criteria == null) return;
            List<CriterionType> criteria = new List<CriterionType>(mark.Criteria);
            criteria.Remove(c);
            mark.Criteria = criteria.ToArray();
        }

        private void InsertIntoCriteria(CriterionType c, dynamic mark, int pos)
        {
            if (mark.Criteria == null)
                mark.Criteria = new CriterionType[] { c };
            List<CriterionType> criteria = new List<CriterionType>(mark.Criteria);
            criteria.Insert(pos, c);
            mark.Criteria = criteria.ToArray();
        }

        private void DeleteSubtask(MarkType nodeToDelete, dynamic formerParent)
        {
            List<MarkType> marks;
          
            if (formerParent == null)
            {
                // We are deleting a Task rather than a Subtask.
                marks = new List<MarkType>(formScheme.Tasks);
                marks.Remove(nodeToDelete);
                formScheme.Tasks = marks.ToArray();
                return;
            }
            if (formerParent.Subtasks != null)
            {
                marks = new List<MarkType>(formerParent.Subtasks);
                marks.Remove(nodeToDelete);
                formerParent.Subtasks = marks.ToArray();
            }
        }

        private void InsertIntoSubtask(MarkType m, dynamic mark, int pos)
        {
            if (mark?.Subtasks == null)
            {
                mark.Subtasks = new MarkType[] { m };
            }
            else
            {
                List<MarkType> marks = new List<MarkType>(mark.Subtasks);
                marks.Insert(pos, m);
                mark.Subtasks = marks.ToArray();
            }
        }
        #endregion



        /// <summary>
        /// Returns true if node1 can be dropped onto node2.
        /// </summary>
        /// <param name="node1">SIRTreeNode being dragged</param>
        /// <param name="node2">Candidate target SIRTreeNode</param>
        /// <returns>true if node1 would be valid as a child of node2, false otherwise.</returns>
        private bool CanDropOn(SIRTreeNode node1, SIRTreeNode node2)
        {

            if (node1 == null || node2 == null)
            {
                return false;
            }

            // Only Criteria can't be dropped on the root
            if (node2 == treeView.Nodes[0])
                return !(node1.Mark is CriterionType);

            // Criteria can be dropped anywhere else
            if (node1.Mark is CriterionType)
                return true;

            // Only Criteria can be dropped between Criteria
            if (node2.Mark is CriterionType)
                return node1.Mark is CriterionType;

            // Can't drop numeric tasks on qualitative tasks.
            if (node1.Mark is NumericType && node2.Mark is QualitativeType)
                return false;

            // Otherwise, check for identity or parent relationships
            if (node2.Parent == null) return true;
            if (node2.Parent.Equals(node1)) return false;

            // If the parent node is not null or equal to the first node, 
            // recurse.
            return CanDropOn(node1, node2.Parent as SIRTreeNode);
        }

        #region Methods for drawing images on treeView
        /// <summary>
        /// Draw guidelines to let the user know what the effects of dropping in the current location would be.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="possible"></param>
        private void DrawLeafTopPlaceholders(SIRTreeNode target, bool possible)
        {
            Graphics g = this.treeView.CreateGraphics();
            Color color = possible ? Color.Blue : Color.LightGray;

            int targetImageWidth = this.treeView.ImageList.Images[target.ImageIndex].Size.Width + 8;
            int leftOffset = target.Bounds.Left - targetImageWidth;
            int rightOffset = this.treeView.Width - 4;
            Brush brush = new SolidBrush(color);

            //DrawGuideArrows(leftOffset, rightOffset, target.Bounds.Top, brush, g);
            g.DrawLine(new Pen(color, 2), new Point(leftOffset, target.Bounds.Top), new Point(rightOffset, target.Bounds.Top));
        }


        private void DrawLeafBottomPlaceholders(SIRTreeNode target, SIRTreeNode parent, bool possible)
        {
            Graphics g = this.treeView.CreateGraphics();

            int targetImageWidth = this.treeView.ImageList.Images[target.ImageIndex].Size.Width + 8;

            int leftOffset, rightOffset;
            if (parent != null && parent.ImageIndex != -1)
                leftOffset = parent.Bounds.Left - (treeView.ImageList.Images[parent.ImageIndex].Size.Width + 8);
            else
                leftOffset = target.Bounds.Left - targetImageWidth;
            rightOffset = this.treeView.Width - 4;
            Color color = possible ? Color.Blue : Color.LightGray;
            Brush brush = new SolidBrush(color);

            //DrawGuideArrows(leftOffset, rightOffset, target.Bounds.Bottom, brush, g);
            g.DrawLine(new Pen(color, 2), new Point(leftOffset, target.Bounds.Bottom), new Point(rightOffset, target.Bounds.Bottom));
        }


        private void DrawFolderTopPlaceholders(SIRTreeNode target, bool possible)
        {
            Graphics g = this.treeView.CreateGraphics();
            int targetImageWidth = this.treeView.ImageList.Images[target.ImageIndex].Size.Width + 8;

            int leftOffset, rightOffset;
            leftOffset = target.Bounds.Left - targetImageWidth;
            rightOffset = this.treeView.Width - 4;
            Color color = possible ? Color.Blue : Color.LightGray;
            Brush brush = new SolidBrush(color);

            //DrawGuideArrows(leftOffset, rightOffset, target.Bounds.Top, brush, g);
            g.DrawLine(new Pen(color, 2), new Point(leftOffset, target.Bounds.Top), new Point(rightOffset, target.Bounds.Top));
        }

        private void DrawGuideArrows(int x1, int x2, int y, Brush brush, Graphics g)
        {
            Point[] leftPointer = new Point[5]{
                                                   new Point(x1, y - 4),
                                                   new Point(x1, y + 4),
                                                   new Point(x1 + 4, y),
                                                   new Point(x1 + 4, y - 1),
                                                   new Point(x1, y - 5)};

            Point[] rightPointer = new Point[5]{
                                                    new Point(x2, y - 4),
                                                    new Point(x2, y + 4),
                                                    new Point(x2 - 4, y),
                                                    new Point(x2 - 4, y - 1),
                                                    new Point(x2, y - 5)};


            g.FillPolygon(brush, leftPointer);
            g.FillPolygon(brush, rightPointer);
        }


        private void DrawAddToFolderPlaceholder(SIRTreeNode target, bool possible)
        {
            Graphics g = treeView.CreateGraphics();
            int RightPos = target.Bounds.Right + 6;
            Color color = possible ? Color.Black : Color.LightGray;
            Brush brush = new SolidBrush(color);
            Point[] pointer = new Point[5]{
                                                    new Point(RightPos, target.Bounds.Y + (target.Bounds.Height / 2) + 4),
                                                    new Point(RightPos, target.Bounds.Y + (target.Bounds.Height / 2) + 4),
                                                    new Point(RightPos - 4, target.Bounds.Y + (target.Bounds.Height / 2)),
                                                    new Point(RightPos - 4, target.Bounds.Y + (target.Bounds.Height / 2) - 1),
                                                    new Point(RightPos, target.Bounds.Y + (target.Bounds.Height / 2) - 5)};


            g.FillPolygon(brush, pointer);
        }

        #endregion
    }
}

public abstract partial class MarkType
{
    public abstract int CriterionCount();
}

public partial class NumericType
{
    public override int CriterionCount() => Criteria.Length;
}

public partial class QualitativeType
{
    public override int CriterionCount() => Criteria.Length;
}

public partial class CriterionType
{
    public override int CriterionCount() => 0;
}