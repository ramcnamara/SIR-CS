using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SIR_CS
{
    public partial class SIRSchemeForm
    {
        Graphics treeOverlay;
        private Point point1;
        private Point point2;

        #region EventHandlers
        /// <summary>
        /// Event that fires when TreeNode drag commences.  Stores the SIRTreeNode that is
        /// being dragged into the current Form.
        /// 
        /// We would normally use e.Item for this, but although TreeNodes are 
        /// serializable in C#, they're not in the OSX version of mono.
        /// </summary>
        /// <param name="sender">A reference to the object that raised the event.</param>
        /// <param name="e">Event data.</param>
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
        /// <param name="sender">A reference to the object that raised the event.</param>
        /// <param name="e">Event data.</param>
        private void TreeView_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.AllowedEffect;
        }

        /// <summary>
        /// Event that fires while an item is dragged over the TreeView.
        /// </summary>
        /// <param name="sender">A reference to the object that raised the event.</param>
        /// <param name="e">Event data.</param>
        private void TreeView_DragOver(object sender, DragEventArgs e)
        {
            // Find node that the mouse pointer is pointing at.
            Point targetPoint = treeView.PointToClient(new Point(e.X, e.Y));
            SIRTreeNode targetNode = treeView.GetNodeAt(targetPoint) as SIRTreeNode;

            if (targetNode == null)
            {
                // We are not currently over a node, so we can't do much.
                return;
            }

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
            if (dropOver) newDrop--;
            if (dropUnder) newDrop++;

            if (targetNode == lastOver && newDrop == overUnder)
                return;

            Console.WriteLine(newDrop);

            lastOver = targetNode;
            overUnder = newDrop;


            // Draw the separator
            if (dropOver)
            {
                ShowDropLocOverNode(targetNode, CanDropOn(draggedNode, dest));
            }

            else if (dropUnder)
            {
                ShowDropLocUnderNode(targetNode, CanDropOn(draggedNode, dest));
            }

            else if (CanDropOn(draggedNode, targetNode))
            {
                DrawAddToFolderPlaceholder(targetNode, CanDropOn(draggedNode, dest));
            }
        }

        /// <summary>
        /// Event that fires when mouse is released after a drag.
        /// </summary>
        /// <param name="sender">A reference to the object that raised the event.</param>
        /// <param name="e">Event data.</param>
        private void TreeView_DragDrop(object sender, DragEventArgs e)
        {
            // Retrieve the client coordinates of the drop location.
            Point targetPoint = treeView.PointToClient(new Point(e.X, e.Y));

            // Retrieve the node at the drop location.
            SIRTreeNode targetNode = treeView.GetNodeAt(targetPoint) as SIRTreeNode;

            // Where should the drop go?
            SIRTreeNode dest = ComputeDropDestination(targetNode, draggedNode);
            MarkType predecessor = ComputeDropAfter(dest, targetNode, draggedNode);

            // Don't allow drops outside nodes
            if (dest == null)
                return;
 
            // Confirm that the node at the drop location is not 
            // the dragged node or a descendant of the dragged node.
            if (!draggedNode.Equals(targetNode) && CanDropOn(draggedNode, dest))
            {          
                if ((e.Effect | DragDropEffects.Move) == DragDropEffects.Move)
                {
                    SIRTreeNode oldParent = draggedNode.Parent as SIRTreeNode;

                    // Are we moving criteria?
                    if (draggedNode.Mark is CriterionType)
                    {
                        DeleteCriterion(draggedNode.Mark as CriterionType, oldParent.Mark);
                        if (dest.Mark is NumericType)
                        {
                            NumericType num = dest.Mark as NumericType;
                            num.Criteria = (InsertAfterMark(num.Criteria, draggedNode.Mark, predecessor).Cast<CriterionType>()).ToArray();
                        }
                        else if (dest.Mark is QualitativeType)
                        {
                            QualitativeType qual = dest.Mark as QualitativeType;
                            qual.Criteria = (InsertAfterMark(qual.Criteria, draggedNode.Mark, predecessor).Cast<CriterionType>()).ToArray();
                        }
                    }

                    // Are we dragging to the root?
                    else if (dest == treeView.Nodes[0])
                    {
                        DeleteSubtask(draggedNode.Mark, oldParent.Mark);
                        formScheme.Tasks = InsertAfterMark(formScheme.Tasks, draggedNode.Mark, predecessor).ToArray();
                    }

                    // We are dragging a numeric or qualitative task to a subtask position.
                    else
                    {
                        DeleteSubtask(draggedNode.Mark, oldParent.Mark);
                        if (dest.Mark is NumericType)
                        {
                            NumericType num = dest.Mark as NumericType;
                            if (num.Subtasks == null)
                                num.Subtasks = new MarkType[1] { draggedNode.Mark };
                            else
                                num.Subtasks = InsertAfterMark(num.Subtasks, draggedNode.Mark, predecessor).ToArray();
                        }
                        else if (dest.Mark is QualitativeType)
                        {
                            QualitativeType qual = dest.Mark as QualitativeType;
                            qual.Subtasks = (InsertAfterMark(qual.Subtasks, draggedNode.Mark, predecessor).Cast<QualitativeType>()).ToArray();
                        }
                    }
                    RebuildTree();
                    return;
                }

            }
        }
        #endregion


        private void RebuildTree()
        {
            treeView.Nodes[0].Nodes.Clear();
            if (formScheme.Tasks != null)
                foreach (var task in formScheme.Tasks)
                {
                    CreateSubtree((SIRTreeNode)treeView.Nodes[0], task);
                }
            treeView.ExpandAll();
            return;
        }

        private MarkType ComputeDropAfter(SIRTreeNode dest, SIRTreeNode targetNode, SIRTreeNode draggedNode)
        {
            if (targetNode == treeView.Nodes[0])
            {
                // We are dropping onto the root node.
                return formScheme.Tasks[formScheme.Tasks.Length - 1];
            }

            if (overUnder == 0 && !(targetNode.Mark is CriterionType))
                // Dropping onto a Task
                return null;

            if (overUnder < 0)
                if (targetNode.Index == 0 || ((targetNode.PrevNode as SIRTreeNode).Mark is CriterionType && !(targetNode.Mark is CriterionType)))
                    // Dropping before first item, or first non-Criterion under Task
                    return null;
                else
                    return (targetNode.PrevNode as SIRTreeNode).Mark;

            return targetNode.Mark;
        }

        private SIRTreeNode ComputeDropDestination(SIRTreeNode targetNode, SIRTreeNode draggedNode)
        {
            if (targetNode == null)
                return null;

            // If not dropping over or under current destination, we are dropping into the node itself.
            if (overUnder == 0 && !(targetNode.Mark is CriterionType))
                return targetNode;

            // Dropping over or under the root node drops onto it
            if (targetNode.Parent == null)
                return targetNode;

            // Dropping before or after an ordinary node drops into its parent
            return targetNode.Parent as SIRTreeNode;
        }

        #region Methods for manipulating model classes to achieve drag and drop reordering
        private void DeleteCriterion(CriterionType c, dynamic mark)
        {
            if (mark.Criteria == null) return;
            List<CriterionType> criteria = new List<CriterionType>(mark.Criteria);
            criteria.Remove(c);
            mark.Criteria = criteria.ToArray();
        }


        private List<MarkType> InsertAfterMark(MarkType[] m, MarkType mark, MarkType predecessor)
        {
            if (m == null)
                return null;

            List<MarkType> markList = new List<MarkType>(m);
            if (predecessor == null || markList.IndexOf(predecessor) == -1)
            {
                markList.Insert(0, mark);
            }
            else
                markList.Insert(markList.IndexOf(predecessor) + 1, mark);
            return markList;
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

        private void ShowDropLocUnderNode(SIRTreeNode target, bool highlight)
        {
            DrawLine(target, target.Bounds.Bottom, highlight);
        }

        private void ShowDropLocOverNode(SIRTreeNode target, bool highlight)
        {
            DrawLine(target, target.Bounds.Top, highlight);
        }

        private void ShowDropLocIntoNode(SIRTreeNode target, bool highlight)
        {
            DrawAddToFolderPlaceholder(target, highlight);
        }

        private void DrawLine(SIRTreeNode target, int yOffset, bool highlight)
        {
            Color color = highlight ? Color.Blue : Color.LightGray;

            int targetImageWidth = this.treeView.ImageList.Images[target.ImageIndex].Size.Width + 8;
            int leftOffset = target.Bounds.Left - targetImageWidth;
            int rightOffset = this.treeView.Width - 4;

            if (point1 != null)
            {
                treeOverlay.DrawLine(new Pen(treeView.BackColor, 2), point1, point2);
            }
            point1 = new Point(leftOffset, yOffset);
            point2 = new Point(rightOffset, yOffset);
            treeOverlay.DrawLine(new Pen(color, 2), point1, point2);
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