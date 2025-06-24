using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace GraphVisualizer
{
    public partial class Form1 : Form
    {
        private Dictionary<string, List<(string, int)>> GraphInfo = new Dictionary<string, List<(string, int)>>();
        private bool isDirected;
        private bool isWeighted;
        private string selectedNode = null;
        private bool deleteMode = false;
        private int nodeCounter = 1;
        private string movingNode = null;
        private PointF mouseOffset;
        private Dictionary<string, PointF> NodePositions = new Dictionary<string, PointF>();

        public Form1()
        {
            InitializeComponent();
            InitializeGraphTypeSelection();

            InitializeDeleteButton();
            InitializeDFSButton();
            InitializeBFSButton();
        }

        private void InitializeDFSButton()
        {
            int Wy = ClientRectangle.Height;
            var dfsButton = new Button
            {
                Text = "DFS",
                Location = new Point(140, 20),
                Size = new Size(70, 20)
            };
            dfsButton.Click += (s, e) => VisualizeDFS();
            this.Controls.Add(dfsButton);
        }
        private void InitializeBFSButton()
        {
            var bfsButton = new Button
            {
                Text = "BFS",
                Location = new Point(230, 20),
                Size = new Size(70, 20)
            };
            bfsButton.Click += (s, e) => VisualizeBFS();
            this.Controls.Add(bfsButton);
        }
        private void InitializeDeleteButton()
        {
            var DeleteButton = new Button
            {
                Text = "Удалить вершину",
                Location = new Point(20, 20),
                Size = new Size(100, 20)
            };
            DeleteButton.Click += (s, e) => deleteMode = deleteMode? false : true;
            this.Controls.Add(DeleteButton);
        }

        private void VisualizeDFS()
        {
            if (GraphInfo.Count == 0)
            {
                MessageBox.Show("Граф пуст. Добавьте вершины и ребра для выполнения DFS.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string startNode = SelectStartNode();
            if (string.IsNullOrEmpty(startNode))
            {
                return; // Если пользователь закрыл окно или ничего не выбрал
            }

            HashSet<string> visited = new HashSet<string>();
            Stack<string> stack = new Stack<string>();

            stack.Push(startNode);
            while (stack.Count > 0)
            {
                var current = stack.Pop();

                if (!visited.Contains(current))
                {
                    visited.Add(current);
                    HighlightNode(current); // Подсвечиваем текущую вершину
                    System.Threading.Thread.Sleep(500); // Пауза для визуализации
                    Invalidate(); // Обновляем отрисовку

                    foreach (var neighbor in GraphInfo[current])
                    {
                        if (!visited.Contains(neighbor.Item1))
                        {
                            stack.Push(neighbor.Item1);
                        }
                    }
                }
            }
        }
        private void VisualizeBFS()
        {
            if (GraphInfo.Count == 0)
            {
                MessageBox.Show("Граф пуст. Добавьте вершины и ребра для выполнения BFS.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string startNode = SelectStartNode();
            if (string.IsNullOrEmpty(startNode))
            {
                return; // Если пользователь закрыл окно или ничего не выбрал
            }

            HashSet<string> visited = new HashSet<string>();
            Queue<string> queue = new Queue<string>();

            queue.Enqueue(startNode);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                if (!visited.Contains(current))
                {
                    visited.Add(current);
                    HighlightNode(current); // Подсвечиваем текущую вершину
                    System.Threading.Thread.Sleep(500); // Пауза для визуализации
                    Invalidate(); // Обновляем отрисовку

                    foreach (var neighbor in GraphInfo[current])
                    {
                        if (!visited.Contains(neighbor.Item1))
                        {
                            queue.Enqueue(neighbor.Item1);
                        }
                    }
                }
            }
        }

        private void HighlightNode(string nodeName)
        {
            var font = new Font("Arial", 10);
            if (NodePositions.ContainsKey(nodeName))
            {
                using (Graphics g = this.CreateGraphics())
                {
                    var position = NodePositions[nodeName];
                    g.FillEllipse(Brushes.HotPink, position.X - 15, position.Y - 15, 30, 30);
                    g.DrawString(nodeName, font, Brushes.Black, position.X - 8, position.Y - 8);
                }
            }
        }

        private string SelectStartNode()
        {
            var dialog = new Form
            {
                Text = "Выбор начальной вершины",
                Size = new Size(300, 200),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent
            };

            var label = new Label
            {
                Text = "Выберите начальную вершину:",
                Location = new Point(10, 20),
                AutoSize = true
            };

            var comboBox = new ComboBox
            {
                Location = new Point(10, 50),
                Size = new Size(260, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            foreach (var node in GraphInfo.Keys)
            {
                comboBox.Items.Add(node);
            }

            var okButton = new Button
            {
                Text = "OK",
                Location = new Point(10, 100),
                DialogResult = DialogResult.OK
            };

            var cancelButton = new Button
            {
                Text = "Отмена",
                Location = new Point(100, 100),
                DialogResult = DialogResult.Cancel
            };

            dialog.Controls.Add(label);
            dialog.Controls.Add(comboBox);
            dialog.Controls.Add(okButton);
            dialog.Controls.Add(cancelButton);

            dialog.AcceptButton = okButton;
            dialog.CancelButton = cancelButton;

            if (dialog.ShowDialog() == DialogResult.OK && comboBox.SelectedItem != null)
            {
                return comboBox.SelectedItem.ToString();
            }

            return null; // Если пользователь ничего не выбрал или нажал "Отмена"
        }

        private void InitializeGraphTypeSelection()
        {
            var dialog = new Form
            {
                Text = "Выберите тип графа",
                Size = new Size(300, 200),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterScreen
            };

            var directedLabel = new Label { Text = "Ориентированный", Location = new Point(110, 25) };
            var directedCheck = new CheckBox { Location = new Point(90, 20) };
            var weightedLabel = new Label { Text = "Взвешенный", Location = new Point(110, 65) };
            var weightedCheck = new CheckBox { Location = new Point(90, 60) };
            var okButton = new Button { Text = "Ок", Location = new Point(100, 120) };

            okButton.Click += (s, e) =>
            {
                isDirected = directedCheck.Checked;
                isWeighted = weightedCheck.Checked;
                dialog.Close();
            };

            dialog.Controls.Add(directedLabel);
            dialog.Controls.Add(directedCheck);
            dialog.Controls.Add(weightedLabel);
            dialog.Controls.Add(weightedCheck);
            dialog.Controls.Add(okButton);

            dialog.ShowDialog();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            DrawGraph(e.Graphics);
        }
       
        private void DrawGraph(Graphics g)
        {
            var pen = new Pen(Color.DeepPink, 2);
            var arrowPen = new Pen(Color.DeepPink, 2) { CustomEndCap = new System.Drawing.Drawing2D.AdjustableArrowCap(6, 6) };
            var font = new Font("Arial", 10);

            foreach (var node in GraphInfo)
            {
                foreach (var edge in node.Value)
                {
                    var sourcePosition = NodePositions[node.Key];
                    var targetPosition = NodePositions[edge.Item1];

                    if (node.Key == edge.Item1)
                    {
                        if (isDirected)
                        {
                            DrawLoop(g, arrowPen, font, sourcePosition, edge.Item2);
                        }
                        
                    }
                    else if (isDirected && GraphInfo.ContainsKey(edge.Item1) && GraphInfo[edge.Item1].Any(e => e.Item1 == node.Key))
                    {
                        DrawCurvedEdge(g, arrowPen, font, sourcePosition, targetPosition, edge.Item2);
                    }
                    else
                    {
                        DrawStraightEdge(g, isDirected ? arrowPen : pen, font, sourcePosition, targetPosition, edge.Item2);
                    }
                }
            }

            foreach (var node in NodePositions)
            {
                var nodePosition = node.Value;
                g.FillEllipse(Brushes.LightPink, nodePosition.X - 15, nodePosition.Y - 15, 30, 30);
                g.DrawEllipse(pen, nodePosition.X - 15, nodePosition.Y - 15, 30, 30);
                g.DrawString(node.Key, font, Brushes.Black, nodePosition.X - 8, nodePosition.Y - 8);
            }

            if (selectedNode != null)
            {
                var selectedPosition = NodePositions[selectedNode];
                g.FillEllipse(Brushes.HotPink, selectedPosition.X - 15, selectedPosition.Y - 15, 30, 30);
                g.DrawString(selectedNode, font, Brushes.Black, selectedPosition.X - 8, selectedPosition.Y - 8);
            }
        }

        private void DrawStraightEdge(Graphics g, Pen pen, Font font, PointF source, PointF target, int weight)
        {
            var direction = new PointF(target.X - source.X, target.Y - source.Y);
            var distance = Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y);

            var offsetX = (float)(15 * direction.X / distance);
            var offsetY = (float)(15 * direction.Y / distance);

            var startPoint = new PointF(source.X + offsetX, source.Y + offsetY);
            var endPoint = new PointF(target.X - offsetX, target.Y - offsetY);

            g.DrawLine(pen, startPoint, endPoint);

            if (isWeighted)
            {
                var midPoint = new PointF((startPoint.X + endPoint.X) / 2, (startPoint.Y + endPoint.Y) / 2);
                g.DrawString(weight.ToString(), font, Brushes.Black, midPoint);
            }
        }

        private void DrawCurvedEdge(Graphics g, Pen pen, Font font, PointF source, PointF target, int weight)
        {
            var midPoint = new PointF((source.X + target.X) / 2, (source.Y + target.Y) / 2);
            var curveOffset = new PointF((source.Y - target.Y) / 4, (target.X - source.X) / 4);
            var controlPoint = new PointF(midPoint.X + curveOffset.X, midPoint.Y + curveOffset.Y);

            var path = new System.Drawing.Drawing2D.GraphicsPath();

            var direction = new PointF(target.X - source.X, target.Y - source.Y);
            var distance = Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y);

            var offsetX = (float)(15 * direction.X / distance);
            var offsetY = (float)(15 * direction.Y / distance);

            var startPoint = new PointF(source.X + offsetX, source.Y + offsetY);
            var endPoint = new PointF(target.X - offsetX, target.Y - offsetY);

            path.AddBezier(startPoint, controlPoint, controlPoint, endPoint);

            g.DrawPath(pen, path);

            if (isWeighted)
            {
                var textPosition = new PointF(controlPoint.X - 10, controlPoint.Y - 10);
                g.DrawString(weight.ToString(), font, Brushes.Black, textPosition);
            }
        }
        
        private void DrawLoop(Graphics g, Pen pen, Font font, PointF source, int weight)
        {
            float offset = 80;
            var controlPoint1 = new PointF(source.X + offset, source.Y + offset); // Первое контрольное
            var controlPoint2 = new PointF(source.X - offset, source.Y + offset); // Второе контрольное
            var path = new System.Drawing.Drawing2D.GraphicsPath();

            g.DrawBezier(pen, source, controlPoint1, controlPoint2, source);

            if (isWeighted)
            {
                var textPosition = new PointF(source.X + 30 , source.Y + 30);
                g.DrawString(weight.ToString(), font, Brushes.Black, textPosition);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
            {
                string clickedNode = GetClickedNode(e.Location);

                if (clickedNode != null)
                {
                    movingNode = clickedNode;
                    mouseOffset = new PointF(e.Location.X - NodePositions[clickedNode].X, e.Location.Y - NodePositions[clickedNode].Y);
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (movingNode != null)
            {
                NodePositions[movingNode] = new PointF(e.Location.X - mouseOffset.X, e.Location.Y - mouseOffset.Y);
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button == MouseButtons.Left)
            {
                movingNode = null;
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (movingNode != null)
                return;

            string clickedNode = GetClickedNode(e.Location);

            if (clickedNode != null)
            {
                if (clickedNode == selectedNode && deleteMode)
                {
                    RemoveNode(clickedNode);
                    selectedNode = null;
                }
                else if (selectedNode == null)
                {
                    selectedNode = clickedNode;
                }
                else
                {
                    HandleEdgeInteraction(selectedNode, clickedNode);
                    selectedNode = null;
                }
            }
            else
            {
                if (selectedNode != null)
                {
                    selectedNode = null;
                }
                else
                {
                    AddNode(e.Location);
                }
            }

            Invalidate();
        }

        private void AddNode(Point location)
        {
            string nodeName = nodeCounter++.ToString();
            GraphInfo[nodeName] = new List<(string, int)>();
            NodePositions[nodeName] = new PointF(location.X, location.Y);
        }

        private void HandleEdgeInteraction(string from, string to)
        {
            var existingEdge = GraphInfo[from].FirstOrDefault(edge => edge.Item1 == to);

            if (existingEdge != default)
            {
                var dialog = new Form
                {
                    Text = isWeighted ? "Изменить или Удалить ребро?" : "Удалить",
                    Size = isWeighted ? new Size(330, 150) : new Size(270, 150),
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    StartPosition = FormStartPosition.CenterScreen
                };

                var question = new Label
                {
                    Text = isWeighted ? "Вы хотите Удалить или Изменить ребро?" : "Вы хотите Удалить ребро?",
                    Location = new Point(50, 20),
                    Size = new Size(260, 30)
                };

                var removeButton = new Button { Text = "Удалить", Location = new Point(20, 70), Width = 80 };
                removeButton.Click += (s, e) =>
                {
                    RemoveEdge(from, to);
                    dialog.Close();
                };

                var cancelButton = new Button { Text = "Отменить", Location = new Point(140, 70), Width = 80 };
                cancelButton.Click += (s, e) =>
                {
                    dialog.Close();
                };

                dialog.Controls.Add(question);
                dialog.Controls.Add(removeButton);

                if (isWeighted)
                {
                    var modifyButton = new Button { Text = "Изменить", Location = new Point(120, 70), Width = 80 };
                    modifyButton.Click += (s, e) =>
                    {
                        ModifyEdgeWeight(from, to);
                        dialog.Close();
                    };
                    dialog.Controls.Add(modifyButton);

                    cancelButton = new Button { Text = "Отменить", Location = new Point(220, 70), Width = 80 };
                    cancelButton.Click += (s, e) =>
                    {
                        dialog.Close();
                    };
                }

                dialog.Controls.Add(cancelButton);

                dialog.ShowDialog();
            }
            else
            {
                AddEdge(from, to);
            }
        }

        private void AddEdge(string from, string to)
        {
            int weight = 1;

            if (isWeighted)
            {
                var weightDialog = new Form
                {
                    Text = "Вес ребра",
                    Size = new Size(300, 150),
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    StartPosition = FormStartPosition.CenterScreen
                };

                var weightLabel = new Label { Text = "Введите вес ребра:", Location = new Point(90, 10), Width = 200 };
                var input = new TextBox { Location = new Point(100, 30), Width = 100 };
                var okButton = new Button { Text = "Ок", Location = new Point(110, 70) };

                okButton.Click += (s, e) =>
                {
                    if (int.TryParse(input.Text, out int parsedWeight))
                    {
                        weight = parsedWeight;
                        weightDialog.Close();
                    }
                };

                weightDialog.Controls.Add(input);
                weightDialog.Controls.Add(okButton);
                weightDialog.Controls.Add(weightLabel);
                weightDialog.ShowDialog();
            }
            if (from == to && !isDirected) MessageBox.Show("Ошибка! Граф является неориентированным. Вы не можете добавить петлю", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else GraphInfo[from].Add((to, weight));
            if (!isDirected)
            {
                GraphInfo[to].Add((from, weight));
            }
        }

        private void RemoveEdge(string from, string to)
        {
            GraphInfo[from].RemoveAll(edge => edge.Item1 == to);
            if (!isDirected)
            {
                GraphInfo[to].RemoveAll(edge => edge.Item1 == from);
            }
        }

        private int ModifyEdgeWeight(string from, string to)
        {
            int parsedWeight = 0;
            var weightDialog = new Form
            {
                Text = "Изменить вес ребра",
                Size = new Size(200, 150),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterScreen
            };

            var input = new TextBox { Location = new Point(50, 30), Width = 100 };
            var okButton = new Button { Text = "Ок", Location = new Point(60, 70) };

            okButton.Click += (s, e) =>
            {
                if (int.TryParse(input.Text, out parsedWeight))
                {
                    for (int i = 0; i < GraphInfo[from].Count; i++)
                    {
                        if (GraphInfo[from][i].Item1 == to)
                        {
                            GraphInfo[from][i] = (to, parsedWeight);
                            break;
                        }
                    }

                    if (!isDirected)
                    {
                        for (int i = 0; i < GraphInfo[to].Count; i++)
                        {
                            if (GraphInfo[to][i].Item1 == from)
                            {
                                GraphInfo[to][i] = (from, parsedWeight);
                                break;
                            }
                        }
                    }

                    weightDialog.Close();
                }
            };

            weightDialog.Controls.Add(input);
            weightDialog.Controls.Add(okButton);
            weightDialog.ShowDialog();

            return parsedWeight;
        }

        private void RemoveNode(string node)
        {
            GraphInfo.Remove(node);
            NodePositions.Remove(node);

            foreach (var edges in GraphInfo.Values)
            {
                edges.RemoveAll(edge => edge.Item1 == node);
            }
        }

        private string GetClickedNode(Point location)
        {
            foreach (var node in NodePositions)
            {
                var nodePosition = node.Value;
                var distance = Math.Sqrt(Math.Pow(location.X - nodePosition.X, 2) + Math.Pow(location.Y - nodePosition.Y, 2));
                if (distance <= 15)
                {
                    return node.Key;
                }
            }

            return null;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            Refresh();
        }
    }
}
