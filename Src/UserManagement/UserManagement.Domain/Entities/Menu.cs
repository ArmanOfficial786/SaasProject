namespace UserManagement.Domain.Entities;

public class Menu
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    [MaxLength(100)]
    public string MenuText { get; private set; }
    [MaxLength(250)]
    public string ToolTip { get; private set; }
    public int OrderNo { get; private set; }
    [MaxLength(256)]
    public string? Url { get; private set; }
    public Guid? ParentId { get; private set; }
    public Menu? Parent { get; private set; }
    [MaxLength(100)]
    public string? Icon { get; private set; }
    [MaxLength(20)]
    public string? Color { get; private set; }
    public bool Active { get; private set; }
    public List<Menu>? Children { get; private set; }

    public Menu(
        Guid id,
        string menuText,
        string toolTip,
        int orderNo,
        string? url,
        Guid? parentId,
        string? icon,
        string? color,
        bool active = true
)
    {
        Id = id;
        MenuText = menuText;
        ToolTip = toolTip;
        OrderNo = orderNo;
        Url = url;
        ParentId = parentId;
        Icon = icon;
        Color = color;
        Active = active;
    }

#pragma warning disable CS8618
    private Menu() { }

}
