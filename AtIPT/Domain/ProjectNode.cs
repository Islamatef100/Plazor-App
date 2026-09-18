namespace AtIPT.Domain;

public sealed class ProjectNode
{
    public string Name { get; set; } = "";

    /// <summary>
    /// Mirrors the old TreeNode.Tag values used by RibbonForm1: "Inputs", "Solutions",
    /// "1", "2", "3" (case number) or a file-system path for the project root.
    /// </summary>
    public string Tag { get; set; } = "";

    public ProjectNode? Parent { get; set; }
    public List<ProjectNode> Children { get; } = new();

    public ProjectNode() { }

    public ProjectNode(string name, string tag = "")
    {
        Name = name;
        Tag = tag;
    }

    public void AddChild(ProjectNode child)
    {
        child.Parent = this;
        Children.Add(child);
    }
}