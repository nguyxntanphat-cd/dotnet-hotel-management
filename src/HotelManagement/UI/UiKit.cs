using System.Text;

namespace HotelManagement.UI;

public enum BtnKind { Primary, Secondary, Danger, Success }

public record Col(string Prop, string Header, int Width = 110, string? Format = null, bool Fill = false, bool Right = false, bool Status = false);

/// <summary>Các hàm tạo control dùng chung để mọi màn hình có cùng phong cách.</summary>
public static class UiKit
{
    public static Button Btn(string text, BtnKind kind = BtnKind.Primary, int width = 0)
    {
        var (bg, fg, hover) = kind switch
        {
            BtnKind.Primary => (Theme.Primary, Color.White, Theme.PrimaryHover),
            BtnKind.Danger => (Theme.Danger, Color.White, Color.FromArgb(185, 28, 28)),
            BtnKind.Success => (Theme.Success, Color.White, Color.FromArgb(21, 128, 61)),
            _ => (Color.White, Theme.Text, Color.FromArgb(241, 245, 249)),
        };
        var b = new Button
        {
            Text = text,
            Font = Theme.Bold,
            BackColor = bg,
            ForeColor = fg,
            FlatStyle = FlatStyle.Flat,
            Height = 34,
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 8, 0),
            UseVisualStyleBackColor = false,
        };
        b.FlatAppearance.BorderSize = kind == BtnKind.Secondary ? 1 : 0;
        b.FlatAppearance.BorderColor = Theme.Border;
        b.FlatAppearance.MouseOverBackColor = hover;
        b.Width = width > 0 ? width : TextRenderer.MeasureText(text, Theme.Bold).Width + 32;
        return b;
    }

    public static Label Lbl(string text, Font? font = null, Color? color = null) => new()
    {
        Text = text,
        Font = font ?? Theme.Base,
        ForeColor = color ?? Theme.Text,
        AutoSize = true,
        BackColor = Color.Transparent,
    };

    public static TextBox Search(string placeholder, int width = 260) => new()
    {
        PlaceholderText = "🔍  " + placeholder,
        Font = Theme.Base,
        Width = width,
        Margin = new Padding(0, 4, 8, 0),
    };

    public static ComboBox Combo(IEnumerable<string> items, int width = 170, string? selected = null)
    {
        var c = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = Theme.Base, Width = width, Margin = new Padding(0, 4, 8, 0) };
        c.Items.AddRange(items.Cast<object>().ToArray());
        if (selected != null) c.SelectedItem = selected;
        else if (c.Items.Count > 0) c.SelectedIndex = 0;
        return c;
    }

    public static DateTimePicker Date(DateTime value, int width = 130) => new()
    {
        Format = DateTimePickerFormat.Custom,
        CustomFormat = "dd/MM/yyyy",
        Value = value,
        Font = Theme.Base,
        Width = width,
        Margin = new Padding(0, 4, 8, 0),
    };

    /// <summary>Khung trắng có viền và tiêu đề, dùng để nhóm nội dung.</summary>
    public static Panel Card(string? title = null, Padding? padding = null)
    {
        var p = new BorderPanel { BackColor = Theme.Surface, Padding = padding ?? new Padding(16, title == null ? 12 : 44, 16, 12) };
        if (title != null)
        {
            var l = Lbl(title, Theme.H2);
            l.Location = new Point(16, 12);
            p.Controls.Add(l);
        }
        return p;
    }

    public static DataGridView Grid()
    {
        var g = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoGenerateColumns = false,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            MultiSelect = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            RowHeadersVisible = false,
            BackgroundColor = Theme.Surface,
            BorderStyle = BorderStyle.None,
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            GridColor = Theme.Border,
            EnableHeadersVisualStyles = false,
            ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
            ColumnHeadersHeight = 38,
            Font = Theme.Base,
        };
        g.RowTemplate.Height = 34;
        g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
        g.ColumnHeadersDefaultCellStyle.ForeColor = Theme.Muted;
        g.ColumnHeadersDefaultCellStyle.Font = Theme.Bold;
        g.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
        g.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(248, 250, 252);
        g.DefaultCellStyle.SelectionBackColor = Theme.PrimarySoft;
        g.DefaultCellStyle.SelectionForeColor = Theme.Text;
        g.DefaultCellStyle.ForeColor = Theme.Text;
        g.DefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
        g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 251, 253);
        return g;
    }

    public static DataGridView Columns(this DataGridView g, params Col[] cols)
    {
        foreach (var c in cols)
        {
            var col = new DataGridViewTextBoxColumn
            {
                Name = c.Prop,
                DataPropertyName = c.Prop,
                HeaderText = c.Header,
                Width = c.Width,
                AutoSizeMode = c.Fill ? DataGridViewAutoSizeColumnMode.Fill : DataGridViewAutoSizeColumnMode.None,
                SortMode = DataGridViewColumnSortMode.NotSortable,
            };
            if (c.Format != null) col.DefaultCellStyle.Format = c.Format;
            if (c.Right) col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            if (c.Status) col.Tag = "status";
            g.Columns.Add(col);
        }
        g.CellFormatting -= ColorStatus;
        g.CellFormatting += ColorStatus;
        return g;
    }

    private static void ColorStatus(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        var g = (DataGridView)sender!;
        if (e.ColumnIndex < 0 || e.RowIndex < 0 || (string?)g.Columns[e.ColumnIndex].Tag != "status") return;
        var c = Theme.StatusColor(e.Value as string);
        e.CellStyle!.ForeColor = c;
        e.CellStyle.SelectionForeColor = c;
        e.CellStyle.Font = Theme.Bold;
    }

    /// <summary>Gán dữ liệu cho lưới (tạo bản sao danh sách để lưới luôn làm mới).</summary>
    public static void Bind<T>(this DataGridView g, IEnumerable<T> rows) => g.DataSource = rows.ToList();

    public static T? Selected<T>(this DataGridView g) where T : class => g.CurrentRow?.DataBoundItem as T;

    public static bool Confirm(string message, string title = "Xác nhận") =>
        MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

    public static void Info(string message, string title = "Thông báo") =>
        MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);

    public static void Warn(string message, string title = "Không hợp lệ") =>
        MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);

    /// <summary>6.5 Xuất báo cáo: ghi một hoặc nhiều lưới ra CSV (UTF-8 có BOM để Excel đọc đúng tiếng Việt).</summary>
    public static void ExportCsv(string tenFile, string tieuDe, params DataGridView[] grids)
    {
        using var dlg = new SaveFileDialog { Filter = "CSV (Excel)|*.csv", FileName = tenFile + ".csv" };
        if (dlg.ShowDialog() != DialogResult.OK) return;
        var sb = new StringBuilder();
        sb.AppendLine(Csv(tieuDe));
        sb.AppendLine(Csv($"Xuất lúc {DateTime.Now:dd/MM/yyyy HH:mm}"));
        foreach (var g in grids)
        {
            sb.AppendLine();
            var cols = g.Columns.Cast<DataGridViewColumn>().Where(c => c.Visible).OrderBy(c => c.DisplayIndex).ToList();
            sb.AppendLine(string.Join(",", cols.Select(c => Csv(c.HeaderText))));
            foreach (DataGridViewRow r in g.Rows)
                sb.AppendLine(string.Join(",", cols.Select(c => Csv(r.Cells[c.Index].FormattedValue?.ToString() ?? ""))));
        }
        File.WriteAllText(dlg.FileName, sb.ToString(), new UTF8Encoding(true));
        Info($"Đã xuất báo cáo ra file:\n{dlg.FileName}");
    }

    private static string Csv(string s) => s.Contains(',') || s.Contains('"') || s.Contains('\n') ? "\"" + s.Replace("\"", "\"\"") + "\"" : s;

    public static FlowLayoutPanel Row(params Control[] controls)
    {
        var f = new FlowLayoutPanel { AutoSize = true, WrapContents = false, BackColor = Color.Transparent, Margin = new Padding(0) };
        f.Controls.AddRange(controls);
        return f;
    }
}

/// <summary>Panel có viền mảnh 1px.</summary>
public class BorderPanel : Panel
{
    public Color BorderColor { get; set; } = Theme.Border;

    public BorderPanel()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using var pen = new Pen(BorderColor);
        e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
    }
}
