using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace KoryukovVA_Lab3
{
    public partial class GraphicalForm : Form
    {
        private Button pointsButton; //"Точки"
        private Button parametersButton; //"Параметры"
        private Button curveButton; //"Кривая"
        private Button polylineButton; //"Ломаная"
        private Button bezierButton; //"Безье"
        private Button filledButton; //"Заполненная"
        private Button movementButton; //"Движение"
        private Button clearButton; //"Очистить"
        private Button drawingButton; //"Рисование"

        private bool isMovementMode = false; //Флаг режима движения
        private bool isDrawingMode = false; //Флаг режима рисования
        private bool isPointsMode = false; //Флаг режима добавления точек

        private enum DrawingMode //Текущий режим
        {
            None,
            Parameters,
            Curve,
            Polyline,
            Bezier,
            Filled
        }
        private DrawingMode currentDrawingMode = DrawingMode.None;
        private Color baseColor = Color.LightGray;
        private Color currentColor = Color.Green;
        private Pen linePen = new Pen(Color.Red, 3);
        private Pen drawingPen = new Pen(Color.Yellow, 2);
        private Brush fillBrush = Brushes.Green;
        private Color pointColor = Color.Black; // Цвет точек по умолчанию
        private Color lineColor = Color.Red; // Цвет кривой по умолчанию

        private List<Point> points = new List<Point>(); //Список точек

        private List<PointMotion> pointMotions = new List<PointMotion>();
        private List<Point> drawingPath = new List<Point>();
        private PointMotion drawingPathMotion;

        private bool isRightMouseDown = false;

        private struct PointMotion
        {
            public PointF Velocity; //Вектор скорости
            public float Angle; //Угол направления
        }
        
        private Timer timer = new Timer();
        private Random random = new Random();

        private float baseSpeed = 3.0f; //Базовая скорость
        private float speedIncrement = 1.0f; //Шаг изменения скорости
        private float minSpeed = 1.0f; //Минимальная скорость
        private float maxSpeed = 100.0f; //Максимальная скорость
        private int moveStep = 5; //Шаг смещения фигуры

        private bool isDragging = false;
        private int draggedPointIndex = -1; //Индекс перемещаемой точки
        private int hitAreaRadius = 10;

        public GraphicalForm()
        {
            DoubleBuffered = true;
            Text = "Graphical Form";
            Size = new Size(800, 600);
            StartPosition = FormStartPosition.CenterScreen;
            Rectangle screenBounds = Screen.PrimaryScreen.Bounds;
            MinimumSize = Size;
            MaximumSize = new Size(screenBounds.Width, screenBounds.Height);
            BackColor = Color.Aquamarine;
            KeyPreview = true;
            CreateButtons();
            SetButtonColors();

            MouseClick += GraphicalForm_MouseClick;
            Paint += GraphicalForm_Paint;

            MouseDown += GraphicalForm_MouseDown;
            MouseMove += GraphicalForm_MouseMove;
            MouseUp += GraphicalForm_MouseUp;

            timer = new Timer();
            timer.Interval = 30;
            timer.Tick += Timer_Tick;
            KeyDown += GraphicalForm_KeyDown;

            linePen.Color = lineColor;
        }
        private void CreateButtons() //Создание кнопок
        {
            int buttonWidth = 100;
            int buttonHeight = 50;
            int buttonSpacing = 20;
            int startX = 10;
            int startY = 10;

            // 1. "Точки"
            pointsButton = new Button();
            pointsButton.Text = "Точки";
            pointsButton.BackColor = baseColor;
            pointsButton.Size = new Size(buttonWidth, buttonHeight);
            pointsButton.Location = new Point(startX, startY);
            pointsButton.Click += PointsButton_Click;
            Controls.Add(pointsButton);

            // 2. "Параметры"
            parametersButton = new Button();
            parametersButton.Text = "Параметры";
            parametersButton.Size = new Size(buttonWidth, buttonHeight);
            parametersButton.Location = new Point(startX, startY + buttonHeight + buttonSpacing);
            parametersButton.Click += ParametersButton_Click;
            Controls.Add(parametersButton);

            // 3. "Кривая"
            curveButton = new Button();
            curveButton.Text = "Кривая";
            curveButton.Size = new Size(buttonWidth, buttonHeight);
            curveButton.Location = new Point(startX, startY + 2 * (buttonHeight + buttonSpacing));
            curveButton.Click += CurveButton_Click;
            Controls.Add(curveButton);

            // 4. "Ломаная"
            polylineButton = new Button();
            polylineButton.Text = "Ломаная";
            polylineButton.Size = new Size(buttonWidth, buttonHeight);
            polylineButton.Location = new Point(startX, startY + 3 * (buttonHeight + buttonSpacing));
            polylineButton.Click += PolylineButton_Click;
            Controls.Add(polylineButton);

            // 5. "Безье"
            bezierButton = new Button();
            bezierButton.Text = "Безье";
            bezierButton.Size = new Size(buttonWidth, buttonHeight);
            bezierButton.Location = new Point(startX, startY + 4 * (buttonHeight + buttonSpacing));
            bezierButton.Click += BezierButton_Click;
            Controls.Add(bezierButton);

            // 6. "Заполненная"
            filledButton = new Button();
            filledButton.Text = "Заполненная";
            filledButton.Size = new Size(buttonWidth, buttonHeight);
            filledButton.Location = new Point(startX, startY + 5 * (buttonHeight + buttonSpacing));
            filledButton.Click += FilledButton_Click;
            Controls.Add(filledButton);

            // 7. "Движение"
            movementButton = new Button();
            movementButton.Text = "Движение";
            movementButton.BackColor = baseColor;
            movementButton.Size = new Size(buttonWidth, buttonHeight);
            movementButton.Location = new Point(startX, startY + 6 * (buttonHeight + buttonSpacing));
            movementButton.Click += MovementButton_Click;
            Controls.Add(movementButton);

            // 8. "Очистить"
            clearButton = new Button();
            clearButton.Text = "Очистить";
            clearButton.Size = new Size(buttonWidth, buttonHeight);
            clearButton.Location = new Point(startX, startY + 7 * (buttonHeight + buttonSpacing));
            clearButton.Click += ClearButton_Click;
            Controls.Add(clearButton);

            // 9. "Рисование"
            drawingButton = new Button();
            drawingButton.Text = "Рисование";
            drawingButton.BackColor = baseColor;
            drawingButton.Size = new Size(buttonWidth, buttonHeight);
            drawingButton.Location = new Point(ClientSize.Width - buttonWidth - startX, ClientSize.Height - buttonHeight - startY);
            drawingButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            drawingButton.Click += DrawingButton_Click;
            Controls.Add(drawingButton);
        }
        private void SetButtonColors() //устанавливаем одинаковый цвет для всех кнопок, кроме "Движение", "Рисование" и "Точки", они обрабатываются по другой логике
        {
            foreach (var control in Controls)
            {
                if (control is Button button)
                {
                    if (button.Text != "Движение" && button.Text != "Рисование" && button.Text != "Точки") button.BackColor = baseColor;
                }
            }
        }
        private void PointsButton_Click(object sender, EventArgs e) //"Точки"
        {
            isPointsMode = !isPointsMode;
            pointsButton.BackColor = isPointsMode ? currentColor : baseColor;
            Invalidate();
        }
        private void ParametersButton_Click(object sender, EventArgs e) //"Параметры"
        {
            SetDrawingMode(DrawingMode.Parameters);
            ShowParametersForm();
        }
        private void ShowParametersForm() //Показываем форму с параметрами
        {
            ParametersForm parametersForm = new ParametersForm(pointColor, lineColor);
            if (parametersForm.ShowDialog() == DialogResult.OK)
            {
                pointColor = parametersForm.PointColor;
                lineColor = parametersForm.CurveColor;

                linePen.Color = lineColor;
                parametersButton.BackColor = Color.LightGray;
                Invalidate();
            }
            parametersForm.Dispose();
        }
        private void CurveButton_Click(object sender, EventArgs e) //"Кривая"
        {
            SetDrawingMode(DrawingMode.Curve);
            Invalidate();
        }
        private void PolylineButton_Click(object sender, EventArgs e) //"Ломаная"
        {
            SetDrawingMode(DrawingMode.Polyline);
            Invalidate();
        }
        private void BezierButton_Click(object sender, EventArgs e) //"Безье"
        {
            SetDrawingMode(DrawingMode.Bezier);
            Invalidate();
        }
        private void FilledButton_Click(object sender, EventArgs e) //"Заполненная"
        {
            SetDrawingMode(DrawingMode.Filled);
            Invalidate();
        }
        private void MovementButton_Click(object sender, EventArgs e) //"Движение"
        {
            if (!isDrawingMode && (points.Count > 0 || drawingPath.Count > 0)) ToggleMovementMode();
        }
        private void ToggleMovementMode() //Переключаем режим движения
        {
            isMovementMode = !isMovementMode;
            movementButton.BackColor = isMovementMode ? currentColor : baseColor;
            if (isMovementMode)
            {
                InitializePointMotions();
                timer.Start();
            }
            else timer.Stop();
            Invalidate();
        }
        private void DrawingButton_Click(object sender, EventArgs e) //"Рисование"
        {
            if (!isMovementMode) ToggleDrawingMode();
        }
        private void ToggleDrawingMode() //Переключаем режим рисования
        {
            isDrawingMode = !isDrawingMode;
            drawingButton.BackColor = isDrawingMode ? currentColor : baseColor;
            Invalidate();
        }
        private void ClearButton_Click(object sender, EventArgs e) //"Очистить"
        {
            if (isPointsMode)
            {
                isPointsMode = false;
                pointsButton.BackColor = baseColor;
            }
            if (isMovementMode)
            {
                isMovementMode = false;
                movementButton.BackColor = baseColor;
                timer.Stop();
            }
            if (isDrawingMode)
            {
                isDrawingMode = false;
                drawingButton.BackColor = baseColor;
                timer.Stop();
            }
            drawingPath.Clear();
            points.Clear();
            SetButtonColors();
            currentDrawingMode = DrawingMode.None;
            Invalidate();
        }
        private void SetDrawingMode(DrawingMode mode) //Устанавливаем текущий режим
        {
            if (mode != DrawingMode.Parameters)
            {
                SetButtonColors();
                currentDrawingMode = mode;
            }
            switch (mode)
            {
                case DrawingMode.Parameters:
                    parametersButton.BackColor = currentColor;
                    break;
                case DrawingMode.Curve:
                    curveButton.BackColor = currentColor;
                    break;
                case DrawingMode.Polyline:
                    polylineButton.BackColor = currentColor;
                    break;
                case DrawingMode.Bezier:
                    bezierButton.BackColor = currentColor;
                    break;
                case DrawingMode.Filled:
                    filledButton.BackColor = currentColor;
                    break;
            }
        }
        private void GraphicalForm_MouseClick(object sender, MouseEventArgs e)
        {
            if (!isMovementMode && !isDrawingMode && isPointsMode)
            {
                points.Add(e.Location);
                Invalidate();
            }
        }
        private void GraphicalForm_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            using (Brush brush = new SolidBrush(pointColor))
            {
                foreach (Point point in points)
                {
                    g.FillEllipse(brush, point.X - 5, point.Y - 5, 10, 10);
                }
            }            
            switch (currentDrawingMode)
            {
                case DrawingMode.Curve:
                    if (points.Count > 2) g.DrawClosedCurve(linePen, points.ToArray());
                    break;
                case DrawingMode.Polyline:
                    if (points.Count > 1) g.DrawPolygon(linePen, points.ToArray());                    
                    break;
                case DrawingMode.Bezier:
                    if (points.Count >= 4 && points.Count % 3 == 1) g.DrawBeziers(linePen, points.ToArray());
                    break;
                case DrawingMode.Filled:  
                    if (points.Count > 2) g.FillClosedCurve(fillBrush, points.ToArray());
                    break;
            }
            if (drawingPath.Count > 1) g.DrawLines(drawingPen, drawingPath.ToArray());
        }
        private void InitializePointMotions() //Инициализируем вектор скорости и направление
        {
            pointMotions.Clear();
            foreach (Point point in points)
            {
                PointMotion motion = GeneratePointMotion();
                pointMotions.Add(motion);
            }
            if (drawingPath.Count > 0) drawingPathMotion = GeneratePointMotion();
        }
        private PointMotion GeneratePointMotion() //Генерируем значения
        {
            double angle = random.NextDouble() * 2 * Math.PI;
            float speed = baseSpeed * (float)(random.NextDouble() * 0.5 + random.NextDouble() * 3);
            PointF velocity = new PointF((float)(speed * Math.Cos(angle)), (float)(speed * Math.Sin(angle)));
            return new PointMotion { Velocity = velocity, Angle = (float)angle };
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < points.Count; i++) MovePoint(i, radius: 5);
            if (drawingPath.Count > 0 && isMovementMode)
            {
                for (int i = 0; i < drawingPath.Count; i++)
                {
                    MovePathPoint(i, radius: 5);
                }
            }
            Invalidate();
        }
        private void MovePoint(int index, float radius) //Движения точек
        {
            PointF newLocation = new PointF(points[index].X + pointMotions[index].Velocity.X, points[index].Y + pointMotions[index].Velocity.Y);
            if (newLocation.X - radius < 0 || newLocation.X + radius > ClientSize.Width)
            {
                pointMotions[index] = new PointMotion
                {
                    Velocity = new PointF(-pointMotions[index].Velocity.X, pointMotions[index].Velocity.Y),
                    Angle = (float)(Math.PI - pointMotions[index].Angle)
                };
                newLocation.X = Clamp(newLocation.X, radius, ClientSize.Width - radius);
            }
            if (newLocation.Y - radius < 0 || newLocation.Y + radius > ClientSize.Height)
            {
                pointMotions[index] = new PointMotion
                {
                    Velocity = new PointF(pointMotions[index].Velocity.X, -pointMotions[index].Velocity.Y),
                    Angle = (-1) * pointMotions[index].Angle
                };
                newLocation.Y = Clamp(newLocation.Y, radius, ClientSize.Height - radius);
            }
            points[index] = new Point((int)newLocation.X, (int)newLocation.Y);
        }
        private void MovePathPoint(int index, float radius) //Движение нарисованного пути
        {
            PointF newLocation = new PointF(drawingPath[index].X + drawingPathMotion.Velocity.X, drawingPath[index].Y + drawingPathMotion.Velocity.Y);
            if (newLocation.X - radius < 0 || newLocation.X + radius > ClientSize.Width)
            {
                drawingPathMotion = new PointMotion
                {
                    Velocity = new PointF(-drawingPathMotion.Velocity.X, drawingPathMotion.Velocity.Y),
                    Angle = (float)(Math.PI - drawingPathMotion.Angle)
                };
                newLocation.X = Clamp(newLocation.X, radius, ClientSize.Width - radius);
            }
            if (newLocation.Y - radius < 0 || newLocation.Y + radius > ClientSize.Height)
            {
                drawingPathMotion = new PointMotion
                {
                    Velocity = new PointF(drawingPathMotion.Velocity.X, -drawingPathMotion.Velocity.Y),
                    Angle = (-1) * drawingPathMotion.Angle
                };
                newLocation.Y = Clamp(newLocation.Y, radius, ClientSize.Height - radius);
            }
            drawingPath[index] = new Point((int)newLocation.X, (int)newLocation.Y);
        }        
        private float Clamp(float value, float min, float max) //Вспомогательная функция
        {
            return (value < min) ? min : (value > max) ? max : value;
        }
        private void GraphicalForm_KeyDown(object sender, KeyEventArgs e) //Обработка клавиш
        {
            switch (e.KeyCode)
            {
                case Keys.Space:
                    MovementButton_Click(sender, EventArgs.Empty);
                    e.Handled = true;
                    break;
                case Keys.Add:
                    ChangeSpeed(speedIncrement);
                    e.Handled = true;
                    break;
                case Keys.Subtract:
                    ChangeSpeed(-speedIncrement);
                    e.Handled = true;
                    break;
                case Keys.Escape:
                    ClearButton_Click(sender, EventArgs.Empty);
                    e.Handled = true;
                    break;
                default:
                    e.Handled = false;
                    break;
            }
        }
        private void ChangeSpeed(float speedChange) //Изменяем скорость
        {
            if (points.Count > 0)
            {
                for (int i = 0; i < points.Count; i++)
                {
                    float newSpeed = Clamp((float)Math.Sqrt(pointMotions[i].Velocity.X * pointMotions[i].Velocity.X + pointMotions[i].Velocity.Y * pointMotions[i].Velocity.Y) + speedChange, minSpeed, maxSpeed);
                    pointMotions[i] = new PointMotion
                    {
                        Velocity = new PointF((float)(newSpeed * Math.Cos(pointMotions[i].Angle)), (float)(newSpeed * Math.Sin(pointMotions[i].Angle))),
                        Angle = pointMotions[i].Angle
                    };
                }
            }
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) //Обработка стрелок
        {
            switch (keyData)
            {
                case Keys.Up:
                    if (CanMoveFigure(0, -moveStep)) MoveFigure(0, -moveStep); // Смещение вверх
                    return true;
                case Keys.Down:
                    if (CanMoveFigure(0, moveStep)) MoveFigure(0, moveStep); // Смещение вниз
                    return true;
                case Keys.Left:
                    if (CanMoveFigure(-moveStep, 0)) MoveFigure(-moveStep, 0); // Смещение влево
                    return true;
                case Keys.Right:
                    if (CanMoveFigure(moveStep, 0)) MoveFigure(moveStep, 0); // Смещение вправо
                    return true;
                default:
                    return base.ProcessCmdKey(ref msg, keyData);
            }
        }
        private bool CanMoveFigure(int dx, int dy) //Проверяем,не упирается ли в границы
        {
            foreach (Point point in points)
            {
                int newX = point.X + dx;
                int newY = point.Y + dy;
                if (newX < 0 || newX >= ClientSize.Width || newY < 0 || newY >= ClientSize.Height) return false;
            }
            return true;
        }
        private void MoveFigure(int dx, int dy) //Перемещаем фигуру
        {
            if (!isMovementMode)
            {
                for (int i = 0; i < points.Count; i++)
                {
                    points[i] = new Point(points[i].X + dx, points[i].Y + dy);
                }
                Invalidate();
            }
        }                
        private void GraphicalForm_MouseDown(object sender, MouseEventArgs e) //Обрабатываем нажатия на мышку
        {
            if (e.Button == MouseButtons.Left && !isMovementMode)
            {                
                for (int i = 0; i < points.Count; i++)
                {
                    int dx = e.X - points[i].X;
                    int dy = e.Y - points[i].Y;
                    if (dx * dx + dy * dy <= hitAreaRadius * hitAreaRadius)
                    {
                        isDragging = true;
                        draggedPointIndex = i;
                        break;
                    }
                }
            }
            else if (e.Button == MouseButtons.Right && isDrawingMode)
            {
                isRightMouseDown = true;
                drawingPath.Clear();
                drawingPath.Add(e.Location);
            }
        }
        private void GraphicalForm_MouseMove(object sender, MouseEventArgs e) //Ведем с зажатой мышкой
        {
            if (isDragging && draggedPointIndex != -1 && !isMovementMode)
            {
                points[draggedPointIndex] = new Point(e.X, e.Y);
                Invalidate();
            }
            else if (isRightMouseDown && isDrawingMode)
            {
                drawingPath.Add(e.Location);
                Invalidate();
            }
        }
        private void GraphicalForm_MouseUp(object sender, MouseEventArgs e) //Отпускаем кнопку мышки
        {
            isDragging = false;
            draggedPointIndex = -1;
            isRightMouseDown = false;
        }
    }
}
