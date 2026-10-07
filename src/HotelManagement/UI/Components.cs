using System.Drawing.Drawing2D;

namespace HotelManagement.UI;

/// <summary>
/// Khung chung cho mọi màn hình chức năng: tiêu đề, dòng mô tả, thanh công cụ và vùng nội dung.
/// </summary>
public class BaseView : UserControl
{
    protected FlowLayoutPanel Toolbar { get; }
    protected Panel Body { get; }

    public BaseView(string title, string subtitle)
    {
        Dock = DockStyle.Fill;
        BackColor = Theme.Background;
        Padding = new Padding(24, 16, 24, 20);
        Font = Theme.Base;

        Body = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Background };
        Toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 46,
            WrapContents = false,
            BackColor = Theme.Background,
            Padding = new Padding(0, 0, 0, 8),
        };
        var header = new Panel { Dock = DockStyle.Top, Height = 62, BackColor = Theme.Background };
        var t = UiKit.Lbl(title, Theme.H1);
        t.Location = new Point(0, 0);
        var s = UiKit.Lbl(subtitle, Theme.Base, Theme.Muted);
        s.Location = new Point(2, 36);
        header.Controls.AddRange(new Control[] { t, s });

        // Thứ tự Add quyết định thứ tự Dock: control thêm sau được xếp trước.
        Controls.Add(Body);
        Controls.Add(Toolbar);
        Controls.Add(header);
    }

    /// <summary>Gọi lại khi màn hình được mở lại để làm mới dữ liệu.</summary>
    public virtual void Reload() { }

    protected void Spacer(int width = 16) => Toolbar.Controls.Add(new Panel { Width = width, Height = 1 });
}

/// <summary>Hộp thoại nhập liệu dựng theo danh sách trường, dùng cho các chức năng thêm/sửa.</summary>
public class FormDialog : Form
{
    private int _y = 20;
    private const int LabelW = 150, FieldW = 300, RowH = 42;
    public Func<string?>? Validator { get; set; }

    public FormDialog(string title)
    {
        Text = title;
        Font = Theme.Base;
        BackColor = Theme.Surface;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(LabelW + FieldW + 56, 100);
    }

    public T Field<T>(string label, T control, int height = 0) where T : Control
    {
        var l = UiKit.Lbl(label, Theme.Base, Theme.Muted);
        l.Location = new Point(24, _y + 6);
        control.Location = new Point(24 + LabelW, _y);
        control.Width = FieldW;
        if (height > 0) control.Height = height;
        Controls.Add(l);
        Controls.Add(control);
        _y += Math.Max(RowH, control.Height + 10);
        return control;
    }

    public TextBox TextField(string label, string value = "", bool password = false) =>
        Field(label, new TextBox { Text = value, Font = Theme.Base, UseSystemPasswordChar = password });

    public ComboBox ComboField(string label, IEnumerable<string> items, string? selected = null) =>
        Field(label, UiKit.Combo(items, FieldW, selected));

    public DateTimePicker DateField(string label, DateTime value) => Field(label, UiKit.Date(value));

    public NumericUpDown NumberField(string label, decimal value, decimal min = 0, decimal max = 1_000_000_000, decimal step = 1) =>
        Field(label, new NumericUpDown { Minimum = min, Maximum = max, Value = Math.Clamp(value, min, max), Increment = step, ThousandsSeparator = true, Font = Theme.Base });

    public Label Note(string text, Color? color = null)
    {
        var l = UiKit.Lbl(text, Theme.Base, color ?? Theme.Muted);
        l.MaximumSize = new Size(FieldW, 0);
        l.Location = new Point(24 + LabelW, _y);
        Controls.Add(l);
        _y += 30;
        return l;
    }

    protected override void OnLoad(EventArgs e)
    {
        var ok = UiKit.Btn("Lưu", BtnKind.Primary, 100);
        var cancel = UiKit.Btn("Hủy", BtnKind.Secondary, 90);
        _y += 10;
        cancel.Location = new Point(ClientSize.Width - 24 - cancel.Width, _y);
        ok.Location = new Point(cancel.Left - 8 - ok.Width, _y);
        cancel.DialogResult = DialogResult.Cancel;
        ok.Click += (_, _) =>
        {
            var err = Validator?.Invoke();
            if (err != null) { UiKit.Warn(err); return; }
            DialogResult = DialogResult.OK;
        };
        Controls.Add(ok);
        Controls.Add(cancel);
        AcceptButton = ok;
        CancelButton = cancel;
        ClientSize = new Size(ClientSize.Width, _y + 54);
        base.OnLoad(e);
    }
}

/// <summary>Thẻ số liệu nhỏ trên bảng điều khiển và trang thống kê.</summary>
public class StatCard : BorderPanel
{
    private readonly Label _value;
    private readonly Label _note;

    public StatCard(string title, string value, Color accent, string note = "")
    {
        BackColor = Theme.Surface;
        Size = new Size(200, 104);
        Margin = new Padding(0, 0, 14, 14);
        var bar = new Panel { BackColor = accent, Dock = DockStyle.Left, Width = 4 };
        var t = UiKit.Lbl(title, Theme.Base, Theme.Muted);
        t.Location = new Point(18, 14);
        _value = UiKit.Lbl(value, Theme.Big, Theme.Text);
        _value.Location = new Point(16, 36);
        _note = UiKit.Lbl(note, Theme.Small, accent);
        _note.Location = new Point(18, 76);
        Controls.AddRange(new Control[] { bar, t, _value, _note });
    }

    public void Set(string value, string? note = null)
    {
        _value.Text = value;
        if (note != null) _note.Text = note;
    }
}

/// <summary>Biểu đồ cột (có thể xếp chồng nhiều chuỗi), vẽ bằng GDI+ để không phụ thuộc thư viện ngoài.</summary>
public class BarChart : Control
{
    public List<(string Label, double[] Values)> Data { get; set; } = new();
    public string[] SeriesNames { get; set; } = Array.Empty<string>();
    public Color[] SeriesColors { get; set; } = { Theme.Primary, Theme.Info, Theme.Warning };
    public Func<double, string> FormatValue { get; set; } = v => v.ToString("N0");
    public bool ShowTotals { get; set; } = true;
    /// <summary>Mỗi cột một màu (dùng cho biểu đồ một chuỗi như số phòng theo trạng thái).</summary>
    public bool ColorPerBar { get; set; }

    public BarChart()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = Theme.Surface;
        Font = Theme.Small;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(BackColor);
        if (Data.Count == 0) return;

        int legendH = SeriesNames.Length > 1 ? 24 : 0;
        var area = new Rectangle(64, 10 + legendH, Width - 80, Height - 44 - legendH);
        if (area.Width < 20 || area.Height < 20) return;

        double max = Math.Max(1, Data.Max(d => d.Values.Sum()));
        double step = NiceStep(max / 4);
        max = Math.Ceiling(max / step) * step;

        using var gridPen = new Pen(Theme.Border);
        using var textBrush = new SolidBrush(Theme.Muted);
        for (double v = 0; v <= max + 0.001; v += step)
        {
            int y = area.Bottom - (int)(v / max * area.Height);
            g.DrawLine(gridPen, area.Left, y, area.Right, y);
            var s = Short(v);
            var sz = g.MeasureString(s, Font);
            g.DrawString(s, Font, textBrush, area.Left - sz.Width - 6, y - sz.Height / 2);
        }

        float slot = (float)area.Width / Data.Count;
        float barW = Math.Max(4, Math.Min(46, slot * 0.6f));
        int labelEvery = Math.Max(1, (int)Math.Ceiling(Data.Count / (area.Width / 56.0)));
        for (int i = 0; i < Data.Count; i++)
        {
            var (label, values) = Data[i];
            float x = area.Left + slot * i + (slot - barW) / 2;
            float yTop = area.Bottom;
            for (int s = 0; s < values.Length; s++)
            {
                float h = (float)(values[s] / max * area.Height);
                using var b = new SolidBrush(SeriesColors[(ColorPerBar ? i : s) % SeriesColors.Length]);
                g.FillRectangle(b, x, yTop - h, barW, h);
                yTop -= h;
            }
            if (ShowTotals && Data.Count <= 14 && values.Sum() > 0)
            {
                var tv = Short(values.Sum());
                var tsz = g.MeasureString(tv, Font);
                g.DrawString(tv, Font, textBrush, x + barW / 2 - tsz.Width / 2, yTop - tsz.Height - 2);
            }
            if (i % labelEvery == 0)
            {
                var lsz = g.MeasureString(label, Font);
                g.DrawString(label, Font, textBrush, x + barW / 2 - lsz.Width / 2, area.Bottom + 6);
            }
        }

        float lx = area.Left;
        for (int s = 0; s < SeriesNames.Length && legendH > 0; s++)
        {
            using var b = new SolidBrush(SeriesColors[s % SeriesColors.Length]);
            g.FillRectangle(b, lx, 6, 12, 12);
            g.DrawString(SeriesNames[s], Font, textBrush, lx + 16, 4);
            lx += g.MeasureString(SeriesNames[s], Font).Width + 36;
        }
    }

    private static double NiceStep(double raw)
    {
        if (raw <= 0) return 1;
        double mag = Math.Pow(10, Math.Floor(Math.Log10(raw)));
        double n = raw / mag;
        return (n <= 1 ? 1 : n <= 2 ? 2 : n <= 5 ? 5 : 10) * mag;
    }

    private static string Short(double v) =>
        v >= 1_000_000_000 ? $"{v / 1_000_000_000:0.#} tỷ" :
        v >= 1_000_000 ? $"{v / 1_000_000:0.#}tr" :
        v >= 1_000 ? $"{v / 1_000:0.#}k" : v.ToString("0");
}
