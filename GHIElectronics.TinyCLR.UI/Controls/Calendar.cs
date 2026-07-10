using System;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Media;

namespace GHIElectronics.TinyCLR.UI.Controls {
    /// <summary>A month calendar: a header with prev/next month arrows, a day-of-week row, and a grid of day cells.
    /// Tapping the arrows changes the month; tapping a day selects it.</summary>
    public class Calendar : Control {
        private static readonly string[] MonthNames = {
            "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
        };
        private static readonly string[] DayInitials = { "S", "M", "T", "W", "T", "F", "S" };

        private int _year;
        private int _month;      // 1-12
        private int _selectedDay; // 0 = none
        private int _rowHeight = 22;

        /// <summary>Colour of the day / header text.</summary>
        public Color ForeColor { get; set; } = Colors.Black;
        /// <summary>Fill behind the selected day.</summary>
        public Color SelectionColor { get; set; } = Color.FromRgb(0x35, 0x8B, 0xF6);

        /// <summary>Creates a calendar showing the current month.</summary>
        public Calendar() {
            var now = DateTime.Now;
            this._year = now.Year;
            this._month = now.Month;
            this._selectedDay = now.Day;
        }

        /// <summary>The displayed year.</summary>
        public int Year {
            get => this._year;
            set { this._year = value; this.Invalidate(); }
        }

        /// <summary>The displayed month (1-12).</summary>
        public int Month {
            get => this._month;
            set { if (value >= 1 && value <= 12) { this._month = value; this.Invalidate(); } }
        }

        /// <summary>The selected day of month (0 = none).</summary>
        public int SelectedDay {
            get => this._selectedDay;
            set { this._selectedDay = value; this.Invalidate(); }
        }

        private static int DaysInMonth(int year, int month) {
            switch (month) {
                case 2: return (year % 4 == 0 && (year % 100 != 0 || year % 400 == 0)) ? 29 : 28;
                case 4:
                case 6:
                case 9:
                case 11: return 30;
                default: return 31;
            }
        }

        private int FirstDayOfWeek() => (int)new DateTime(this._year, this._month, 1).DayOfWeek; // 0 = Sunday

        private void PrevMonth() {
            if (--this._month < 1) {
                this._month = 12;
                this._year--;
            }

            this._selectedDay = 0;
            this.Invalidate();
        }

        private void NextMonth() {
            if (++this._month > 12) {
                this._month = 1;
                this._year++;
            }

            this._selectedDay = 0;
            this.Invalidate();
        }

        /// <summary>Reports the full grid height (header + day-of-week row + 6 week rows).</summary>
        protected override void MeasureOverride(int availableWidth, int availableHeight, out int desiredWidth, out int desiredHeight) {
            desiredWidth = availableWidth < Media.Constants.MaxExtent ? availableWidth : 7 * 20;
            desiredHeight = 8 * this._rowHeight;
        }

        /// <summary>Handles taps on the prev/next arrows and the day cells.</summary>
        protected override void OnTouchUp(TouchEventArgs e) {
            if (!this.IsEnabled) {
                return;
            }

            e.GetPosition(this, 0, out var x, out var y);
            var rh = this._rowHeight;

            if (y >= 0 && y < rh) {
                if (x < rh) {
                    this.PrevMonth();
                }
                else if (x > this._renderWidth - rh) {
                    this.NextMonth();
                }

                e.Handled = true;
                return;
            }

            // Day grid starts at row 2 (after header + day-of-week row).
            var gridRow = (y / rh) - 2;
            if (gridRow >= 0 && gridRow < 6) {
                var cellW = this._renderWidth / 7;
                if (cellW > 0) {
                    var col = x / cellW;
                    if (col >= 0 && col < 7) {
                        var cellIndex = gridRow * 7 + col;
                        var day = cellIndex - this.FirstDayOfWeek() + 1;
                        if (day >= 1 && day <= DaysInMonth(this._year, this._month)) {
                            this._selectedDay = day;
                            this.Invalidate();
                            e.Handled = true;
                        }
                    }
                }
            }
        }

        /// <summary>Draws the header, weekday initials, and the day grid.</summary>
        public override void OnRender(DrawingContext dc) {
            base.OnRender(dc);

            if (this.Font == null) {
                return;
            }

            var w = this._renderWidth;
            var rh = this._rowHeight;
            var cellW = w / 7;
            if (cellW < 1) {
                cellW = 1;
            }

            var pen = new Media.Pen(this.ForeColor, 1);
            var fh = this.Font.Height;

            // Header: month + year, centred, with prev/next arrows.
            var title = MonthNames[this._month - 1] + " " + this._year.ToString();
            dc.DrawText(ref title, this.Font, this.ForeColor, rh, (rh - fh) / 2, w - 2 * rh, fh, TextAlignment.Center, TextTrimming.CharacterEllipsis);
            dc.DrawLine(pen, rh / 2 + 2, rh / 2, rh / 2 - 2, rh / 2 - 4);   // prev arrow
            dc.DrawLine(pen, rh / 2 + 2, rh / 2, rh / 2 - 2, rh / 2 + 4);
            dc.DrawLine(pen, w - rh / 2 - 2, rh / 2, w - rh / 2 + 2, rh / 2 - 4); // next arrow
            dc.DrawLine(pen, w - rh / 2 - 2, rh / 2, w - rh / 2 + 2, rh / 2 + 4);

            // Weekday initials.
            for (var c = 0; c < 7; c++) {
                var dow = DayInitials[c];
                dc.DrawText(ref dow, this.Font, this.ForeColor, c * cellW, rh + (rh - fh) / 2, cellW, fh, TextAlignment.Center, TextTrimming.None);
            }

            // Days.
            var first = this.FirstDayOfWeek();
            var days = DaysInMonth(this._year, this._month);
            for (var day = 1; day <= days; day++) {
                var cellIndex = first + day - 1;
                var gridRow = cellIndex / 7;
                var col = cellIndex % 7;
                var x = col * cellW;
                var y = (2 + gridRow) * rh;

                if (day == this._selectedDay) {
                    dc.DrawRectangle(new SolidColorBrush(this.SelectionColor), null, x + 1, y + 1, cellW - 2, rh - 2);
                }

                var txt = day.ToString();
                dc.DrawText(ref txt, this.Font, this.ForeColor, x, y + (rh - fh) / 2, cellW, fh, TextAlignment.Center, TextTrimming.None);
            }
        }
    }
}
