using System;
using System.Drawing;
using System.Windows.Forms;

namespace KoryukovVA_Lab3
{
    public partial class ParametersForm : Form
    {
        private Button pointColorButton; //Цвет точек
        private Button curveColorButton; //Цвет прямой
        private Color pointColor;
        private Color curveColor;

        public Color PointColor
        {
            get { return pointColor; }
            set { pointColor = value; }
        }
        public Color CurveColor
        {
            get { return curveColor; }
            set { curveColor = value; }
        }
        public ParametersForm(Color initialPointColor, Color initialCurveColor)
        {
            Text = "Параметры";
            Size = new Size(200, 150);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            pointColor = initialPointColor;
            curveColor = initialCurveColor;
            CreateButtons();
        }
        private void CreateButtons() //Создаем кнопки
        {
            //Кнопка выбора цвета точек
            pointColorButton = new Button();
            pointColorButton.Text = "Цвет точек";
            pointColorButton.Location = new Point(20, 20);
            pointColorButton.Size = new Size(100, 30);
            pointColorButton.Click += PointColorButton_Click;
            Controls.Add(pointColorButton);

            //Кнопка выбора цвета кривой
            curveColorButton = new Button();
            curveColorButton.Text = "Цвет кривой";
            curveColorButton.Location = new Point(20, 70);
            curveColorButton.Size = new Size(100, 30);
            curveColorButton.Click += CurveColorButton_Click;
            Controls.Add(curveColorButton);
        }
        private void PointColorButton_Click(object sender, EventArgs e) //Выбор цвета точек
        {
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.Color = pointColor;
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                pointColor = colorDialog.Color;
                Invalidate();
            }
        }
        private void CurveColorButton_Click(object sender, EventArgs e) //Выбор цвета кривой
        {
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.Color = curveColor;
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                curveColor = colorDialog.Color;
                Invalidate();
            }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (Brush brush = new SolidBrush(pointColor))
            {
                e.Graphics.FillRectangle(brush, new Rectangle(150, 20, 30, 30));
            }
            using (Brush brush = new SolidBrush(curveColor))
            {
                e.Graphics.FillRectangle(brush, new Rectangle(150, 70, 30, 30));
            }
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (e.CloseReason == CloseReason.UserClosing) DialogResult = DialogResult.OK;
        }
    }
}