using System.Windows.Forms;
using InverseBinding;
using System;

public partial class MarkPanel: UserControl
{
    private readonly MarkType mark;
    internal bool ChangedSinceSave { get; set; }
    private CriterionType[] criteriaSource;
    BindingSource source = new BindingSource();

    public event EventHandler TextChangeHandler;

    public MarkPanel(MarkType newMark)
    {
        mark = newMark;
        InitializeComponent();
        criterionTable.AutoGenerateColumns = false;

        // bind simple components to underlying XML-derived classes
        taskNameBox.DataBindings.Add(new Binding("Text", mark, "Name"));
        taskDescBox.DataBindings.Add(new Binding("Text", mark, "Description"));
        rbGroup.DataBindings.Add(new Binding("Checked", mark, "group"));
        rbIndividual.DataBindings.Add(InvertedBinding.Create(rbGroup, "Checked"));

        // handle metadata that are only present for numeric tasks

        if (mark is NumericType nt)
        {
            cbBonus.DataBindings.Add(new Binding("Checked", nt, "bonus"));
            cbPenalty.DataBindings.Add(new Binding("Checked", nt, "penalty"));

            decimal computedMaxMark = 0;
            bool hasNumericSubtasks = false;

            if (nt.Subtasks != null)
            {

                foreach (var st in nt.Subtasks)
                    if (st is NumericType)
                    {
                        hasNumericSubtasks = true;
                        computedMaxMark += ((NumericType)st).GetTotalMaxMark();
                    }
            }
            // set value of max mark box
            if (hasNumericSubtasks)
            {
                maxMarkBox.Text = computedMaxMark.ToString("0.0");
                maxMarkBox.Enabled = false;
                maxMarkLabel.Enabled = false;
            }
            else
            {
                maxMarkBox.DataBindings.Add(new Binding("Text", nt, "maxMark"));
                maxMarkBox.Enabled = true;
                maxMarkLabel.Enabled = true;
                maxMarkBox.TextChanged += new EventHandler(OnTextChanged);
            }
            // bind table of criteria
            criteriaSource = nt.Criteria;
        }
        else
        {
            cbBonus.Enabled = false;
            cbPenalty.Enabled = false;
            if (newMark is QualitativeType qt)
            {
                criteriaSource = qt.Criteria;
            }
        }
        if (criteriaSource != null)
        {
            source.DataSource = criteriaSource;
            DataGridViewColumn nameCol = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Name",
                Name = "Criterion name"
            };
            criterionTable.Columns.Add(nameCol);
            criterionTable.DataSource = source;
        }
        else
            criterionTable.Enabled = false;

        // wire up events to propagate text changes to the tree
        taskNameBox.TextChanged += new EventHandler(OnTextChanged);
        rbGroup.CheckedChanged += new EventHandler(OnTextChanged);
        cbBonus.CheckedChanged += new EventHandler(OnTextChanged);
        cbPenalty.CheckedChanged += new EventHandler(OnTextChanged);

        // dirty detection
        ChangedSinceSave = false;
    }

    private void OnTextChanged(object sender, EventArgs e)
    {
        ChangedSinceSave = true;
        TextChangeHandler?.Invoke(this, e);
    }

    public string GetXmlName()
    {
        return mark.Name;
    }

    internal string GetTaskName() => (taskNameBox.Text == ""? mark.Name : taskNameBox.Text);

}
