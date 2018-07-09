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
    }
}
