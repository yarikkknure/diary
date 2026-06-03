using System;
using System.Drawing;
using System.Windows.Forms;

namespace Diary
{
    /// <summary>
    /// Форма додавання / редагування заходу щоденника.
    /// </summary>
    public class EventForm : Form
    {
        // ── Public properties ─────────────────────────────────────────────
        public string EventTitle => txtTitle.Text.Trim();

        public DateTime EventStart
        {
            get
            {
                var d = dtpDate.Value.Date;
                var t = dtpTime.Value.TimeOfDay;
                return d + t;
            }
        }

        public int EventDuration
        {
            get
            {
                return int.TryParse(txtDuration.Text.Trim(), out int v) && v > 0 ? v : 60;
            }
        }

        public string EventLocation => txtLocation.Text.Trim();
        public string EventNotes    => txtNotes.Text.Trim();

        // ── Controls ──────────────────────────────────────────────────────
        private TextBox      txtTitle    = null!;
        private DateTimePicker dtpDate   = null!;
        private DateTimePicker dtpTime   = null!;
        private TextBox      txtDuration = null!;
        private TextBox      txtLocation = null!;
        private TextBox      txtNotes    = null!;
        private Button       btnSave     = null!;
        private Button       btnCancel   = null!;

        // ── Constructor (new) ─────────────────────────────────────────────
        public EventForm()
        {
            BuildUI();
            Text = "Новий захід";
            dtpDate.Value = DateTime.Today;
            // Set time to next full hour
            int nextHour = DateTime.Now.Hour + 1;
            if (nextHour >= 24) nextHour = 8;
            dtpTime.Value = new DateTime(2000, 1, 1, nextHour, 0, 0);
            txtDuration.Text = "60";
        }

        // ── Constructor (edit) ────────────────────────────────────────────
        public EventForm(DiaryEvent ev) : this()
        {
            Text = "Редагування заходу";
            txtTitle.Text    = ev.Title;
            dtpDate.Value    = ev.StartDateTime.Date;
            dtpTime.Value    = new DateTime(2000, 1, 1,
                                   ev.StartDateTime.Hour,
                                   ev.StartDateTime.Minute, 0);
            txtDuration.Text = ev.DurationMinutes.ToString();
            txtLocation.Text = ev.Location;
            txtNotes.Text    = ev.Notes;
        }

        // ── UI builder ────────────────────────────────────────────────────
        private void BuildUI()
        {
            SuspendLayout();
            Text            = "Захід";
            Size            = new Size(400, 380);
            MinimumSize     = new Size(360, 340);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            BackColor       = SystemColors.Control;
            Font            = new Font("Segoe UI", 9.5f);

            // ── Labels column (left, fixed 95px) ───────────────────────
            int lx = 14, cx = 116, w = 258, row = 14, step = 32;

            Label MkLbl(string text, int y) => new Label
            {
                Text      = text,
                Location  = new Point(lx, y + 3),
                Size      = new Size(95, 22),
                TextAlign = ContentAlignment.MiddleRight,
            };

            // Row 0 – Title
            Controls.Add(MkLbl("Назва *:", row));
            txtTitle = new TextBox { Location = new Point(cx, row), Width = w, MaxLength = 120 };
            Controls.Add(txtTitle);
            row += step;

            // Row 1 – Date
            Controls.Add(MkLbl("Дата:", row));
            dtpDate = new DateTimePicker
            {
                Location = new Point(cx, row), Width = w,
                Format = DateTimePickerFormat.Short,
            };
            Controls.Add(dtpDate);
            row += step;

            // Row 2 – Time
            Controls.Add(MkLbl("Час початку:", row));
            dtpTime = new DateTimePicker
            {
                Location = new Point(cx, row), Width = w,
                Format = DateTimePickerFormat.Time,
                ShowUpDown = true,
            };
            Controls.Add(dtpTime);
            row += step;

            // Row 3 – Duration
            Controls.Add(MkLbl("Тривалість, хв:", row));
            txtDuration = new TextBox { Location = new Point(cx, row), Width = 70 };
            Controls.Add(txtDuration);
            row += step;

            // Row 4 – Location
            Controls.Add(MkLbl("Місце:", row));
            txtLocation = new TextBox { Location = new Point(cx, row), Width = w, MaxLength = 200 };
            Controls.Add(txtLocation);
            row += step;

            // Row 5 – Notes label
            Controls.Add(MkLbl("Нотатки:", row));
            txtNotes = new TextBox
            {
                Location    = new Point(cx, row),
                Size        = new Size(w, 70),
                Multiline   = true,
                ScrollBars  = ScrollBars.Vertical,
            };
            Controls.Add(txtNotes);
            row += 78;

            // Buttons
            btnSave = new Button
            {
                Text     = "Зберегти",
                Location = new Point(cx, row),
                Size     = new Size(90, 28),
                TabIndex = 10,
            };
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button
            {
                Text     = "Скасувати",
                Location = new Point(cx + 98, row),
                Size     = new Size(90, 28),
                TabIndex = 11,
            };
            btnCancel.Click += (_, __) => { DialogResult = DialogResult.Cancel; Close(); };

            Controls.Add(btnSave);
            Controls.Add(btnCancel);

            // Adjust form height
            ClientSize = new Size(390, row + 46);

            AcceptButton = btnSave;
            CancelButton = btnCancel;
            ResumeLayout(false);
        }

        // ── Validation ────────────────────────────────────────────────────
        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("Введіть назву заходу.", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTitle.Focus();
                return;
            }
            if (!int.TryParse(txtDuration.Text.Trim(), out int dur) || dur <= 0)
            {
                MessageBox.Show("Тривалість має бути цілим числом більше 0 (у хвилинах).",
                    "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDuration.Focus();
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
