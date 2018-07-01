using System.Windows.Forms;
using InverseBinding;
using System;

public partial class MarkPanel: UserControl
{
    private readonly MarkType mark;
    internal bool ChangedSinceSave { get; set; }

    public event EventHandler TextChangeHandler;

    public MarkPanel(MarkType newMark)
    {
        mark = newMark;
        InitializeComponent();

        taskNameBox.DataBindings.Add(new Binding("Text", mark, "Name"));
        taskDescBox.DataBindings.Add(new Binding("Text", mark, "Description"));
        rbGroup.DataBindings.Add(new Binding("Checked", mark, "group"));
        rbIndividual.DataBindings.Add(InvertedBinding.Create(rbGroup, "Checked"));

        // wire up events to propagate text changes to the tree
        taskNameBox.TextChanged += new EventHandler(OnTextChanged);
        rbGroup.CheckedChanged += new EventHandler(OnTextChanged);

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
