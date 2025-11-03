using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace TSP_AntColony
{
    public partial class MainForm : Form
    {
        private List<City> cities = new List<City>();
        private List<City> bestTour = new List<City>();
        private double bestDistance = double.MaxValue;
        private AntColonyOptimizer aco;
        private bool isRunning = false;
        private System.Windows.Forms.Timer animationTimer;
        private int currentIteration = 0;

        public MainForm()
        {
            InitializeComponent();
            animationTimer = new System.Windows.Forms.Timer();
            animationTimer.Interval = 100;
            animationTimer.Tick += AnimationTimer_Tick;
        }

        private void InitializeComponent()
        {
            this.Text = "TSP - Ant Colony Optimization";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Drawing Panel
            var drawPanel = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(800, 700),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            drawPanel.Paint += DrawPanel_Paint;
            drawPanel.MouseClick += DrawPanel_MouseClick;

            // Controls Panel
            var controlPanel = new Panel
            {
                Location = new Point(820, 10),
                Size = new Size(360, 700),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Labels and Inputs
            var lblCities = new Label { Text = "Cities (click to add):", Location = new Point(10, 10), AutoSize = true };
            var lblNumAnts = new Label { Text = "Number of Ants:", Location = new Point(10, 40), AutoSize = true };
            var numAnts = new NumericUpDown { Location = new Point(150, 38), Value = 20, Minimum = 5, Maximum = 100, Width = 100 };
            
            var lblIterations = new Label { Text = "Max Iterations:", Location = new Point(10, 70), AutoSize = true };
            var numIterations = new NumericUpDown { Location = new Point(150, 68), Value = 100, Minimum = 10, Maximum = 1000, Width = 100 };

            var lblAlpha = new Label { Text = "Alpha (pheromone):", Location = new Point(10, 100), AutoSize = true };
            var numAlpha = new NumericUpDown { Location = new Point(150, 98), Value = 1, Minimum = 0, Maximum = 5, DecimalPlaces = 1, Increment = 0.1M, Width = 100 };

            var lblBeta = new Label { Text = "Beta (distance):", Location = new Point(10, 130), AutoSize = true };
            var numBeta = new NumericUpDown { Location = new Point(150, 128), Value = 2, Minimum = 0, Maximum = 5, DecimalPlaces = 1, Increment = 0.1M, Width = 100 };

            var lblEvaporation = new Label { Text = "Evaporation Rate:", Location = new Point(10, 160), AutoSize = true };
            var numEvaporation = new NumericUpDown { Location = new Point(150, 158), Value = 0.5M, Minimum = 0, Maximum = 1, DecimalPlaces = 2, Increment = 0.05M, Width = 100 };

            // Buttons
            var btnStart = new Button { Text = "Start", Location = new Point(10, 200), Size = new Size(100, 30) };
            var btnPause = new Button { Text = "Pause", Location = new Point(120, 200), Size = new Size(100, 30), Enabled = false };
            var btnReset = new Button { Text = "Reset", Location = new Point(230, 200), Size = new Size(100, 30) };
            var btnRandom = new Button { Text = "Add Random Cities", Location = new Point(10, 240), Size = new Size(150, 30) };
            var btnClear = new Button { Text = "Clear All", Location = new Point(170, 240), Size = new Size(100, 30) };

            // Info Display
            var lblInfo = new Label 
            { 
                Location = new Point(10, 290), 
                Size = new Size(340, 400),
                Text = "Click on the canvas to add cities.\n\nAnt Colony Optimization uses:\n- Pheromone trails\n- Probabilistic decisions\n- Evaporation",
                BackColor = Color.LightYellow
            };

            // Event Handlers
            btnStart.Click += (s, e) =>
            {
                if (cities.Count < 3)
                {
                    MessageBox.Show("Please add at least 3 cities!");
                    return;
                }

                if (!isRunning)
                {
                    aco = new AntColonyOptimizer(
                        cities,
                        (int)numAnts.Value,
                        (int)numIterations.Value,
                        (double)numAlpha.Value,
                        (double)numBeta.Value,
                        (double)numEvaporation.Value
                    );
                    currentIteration = 0;
                    isRunning = true;
                    animationTimer.Start();
                    btnStart.Enabled = false;
                    btnPause.Enabled = true;
                }
            };

            btnPause.Click += (s, e) =>
            {
                if (isRunning)
                {
                    animationTimer.Stop();
                    isRunning = false;
                    btnStart.Enabled = true;
                    btnPause.Enabled = false;
                    btnStart.Text = "Resume";
                }
            };

            btnReset.Click += (s, e) =>
            {
                animationTimer.Stop();
                isRunning = false;
                currentIteration = 0;
                bestTour.Clear();
                bestDistance = double.MaxValue;
                btnStart.Enabled = true;
                btnPause.Enabled = false;
                btnStart.Text = "Start";
                drawPanel.Invalidate();
                lblInfo.Text = "Reset complete. Click Start to begin optimization.";
            };

            btnRandom.Click += (s, e) =>
            {
                var rand = new Random();
                for (int i = 0; i < 10; i++)
                {
                    cities.Add(new City(rand.Next(50, 750), rand.Next(50, 650), $"City {cities.Count + 1}"));
                }
                drawPanel.Invalidate();
            };

            btnClear.Click += (s, e) =>
            {
                cities.Clear();
                bestTour.Clear();
                bestDistance = double.MaxValue;
                animationTimer.Stop();
                isRunning = false;
                btnStart.Text = "Start";
                btnStart.Enabled = true;
                btnPause.Enabled = false;
                drawPanel.Invalidate();
                lblInfo.Text = "All cleared. Add cities to begin.";
            };

            // Add controls
            controlPanel.Controls.AddRange(new Control[] 
            { 
                lblCities, lblNumAnts, numAnts, lblIterations, numIterations,
                lblAlpha, numAlpha, lblBeta, numBeta, lblEvaporation, numEvaporation,
                btnStart, btnPause, btnReset, btnRandom, btnClear, lblInfo
            });

            this.Controls.Add(drawPanel);
            this.Controls.Add(controlPanel);

            this.Tag = new { drawPanel, lblInfo };
        }

        private void DrawPanel_MouseClick(object sender, MouseEventArgs e)
        {
            if (!isRunning)
            {
                cities.Add(new City(e.X, e.Y, $"City {cities.Count + 1}"));
                ((Panel)sender).Invalidate();
            }
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            if (currentIteration < aco.MaxIterations)
            {
                aco.RunIteration();
                currentIteration++;

                if (aco.BestDistance < bestDistance)
                {
                    bestDistance = aco.BestDistance;
                    bestTour = new List<City>(aco.BestTour);
                }

                var lblInfo = (Label)((dynamic)this.Tag).lblInfo;
                lblInfo.Text = $"Iteration: {currentIteration}/{aco.MaxIterations}\n" +
                              $"Best Distance: {bestDistance:F2}\n" +
                              $"Cities: {cities.Count}\n\n" +
                              $"Current Tour:\n{string.Join(" → ", bestTour.Select(c => c.Name))}";

                ((Panel)((dynamic)this.Tag).drawPanel).Invalidate();
            }
            else
            {
                animationTimer.Stop();
                isRunning = false;
                MessageBox.Show($"Optimization Complete!\nBest Distance: {bestDistance:F2}");
            }
        }

        private void DrawPanel_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Draw best tour
            if (bestTour.Count > 1)
            {
                using (var pen = new Pen(Color.Blue, 2))
                {
                    for (int i = 0; i < bestTour.Count - 1; i++)
                    {
                        g.DrawLine(pen, bestTour[i].X, bestTour[i].Y, bestTour[i + 1].X, bestTour[i + 1].Y);
                    }
                    g.DrawLine(pen, bestTour[bestTour.Count - 1].X, bestTour[bestTour.Count - 1].Y, 
                              bestTour[0].X, bestTour[0].Y);
                }
            }

            // Draw cities
            foreach (var city in cities)
            {
                g.FillEllipse(Brushes.Red, city.X - 5, city.Y - 5, 10, 10);
                g.DrawString(city.Name, new Font("Arial", 8), Brushes.Black, city.X + 8, city.Y - 8);
            }
        }
    }

    public class City
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Name { get; set; }

        public City(int x, int y, string name)
        {
            X = x;
            Y = y;
            Name = name;
        }

        public double DistanceTo(City other)
        {
            return Math.Sqrt(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2));
        }
    }

    public class AntColonyOptimizer
    {
        private List<City> cities;
        private double[,] pheromones;
        private double[,] distances;
        private int numAnts;
        public int MaxIterations { get; }
        private double alpha;
        private double beta;
        private double evaporationRate;
        private Random rand = new Random();

        public List<City> BestTour { get; private set; }
        public double BestDistance { get; private set; }

        public AntColonyOptimizer(List<City> cities, int numAnts, int maxIterations, 
                                  double alpha, double beta, double evaporationRate)
        {
            this.cities = cities;
            this.numAnts = numAnts;
            this.MaxIterations = maxIterations;
            this.alpha = alpha;
            this.beta = beta;
            this.evaporationRate = evaporationRate;
            this.BestDistance = double.MaxValue;

            int n = cities.Count;
            pheromones = new double[n, n];
            distances = new double[n, n];

            // Initialize distances and pheromones
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    distances[i, j] = cities[i].DistanceTo(cities[j]);
                    pheromones[i, j] = 1.0;
                }
            }
        }

        public void RunIteration()
        {
            var tours = new List<List<int>>();
            var tourDistances = new List<double>();

            // Each ant constructs a tour
            for (int ant = 0; ant < numAnts; ant++)
            {
                var tour = ConstructTour();
                var distance = CalculateTourDistance(tour);
                tours.Add(tour);
                tourDistances.Add(distance);

                if (distance < BestDistance)
                {
                    BestDistance = distance;
                    BestTour = tour.Select(i => cities[i]).ToList();
                }
            }

            // Evaporate pheromones
            for (int i = 0; i < cities.Count; i++)
            {
                for (int j = 0; j < cities.Count; j++)
                {
                    pheromones[i, j] *= (1 - evaporationRate);
                }
            }

            // Deposit pheromones
            for (int ant = 0; ant < numAnts; ant++)
            {
                var deposit = 1.0 / tourDistances[ant];
                for (int i = 0; i < tours[ant].Count - 1; i++)
                {
                    int from = tours[ant][i];
                    int to = tours[ant][i + 1];
                    pheromones[from, to] += deposit;
                    pheromones[to, from] += deposit;
                }
                // Close the tour
                int last = tours[ant][tours[ant].Count - 1];
                int first = tours[ant][0];
                pheromones[last, first] += deposit;
                pheromones[first, last] += deposit;
            }
        }

        private List<int> ConstructTour()
        {
            var tour = new List<int>();
            var visited = new bool[cities.Count];
            int current = rand.Next(cities.Count);
            tour.Add(current);
            visited[current] = true;

            while (tour.Count < cities.Count)
            {
                int next = SelectNextCity(current, visited);
                tour.Add(next);
                visited[next] = true;
                current = next;
            }

            return tour;
        }

        private int SelectNextCity(int current, bool[] visited)
        {
            var probabilities = new double[cities.Count];
            double sum = 0;

            for (int i = 0; i < cities.Count; i++)
            {
                if (!visited[i])
                {
                    double pheromone = Math.Pow(pheromones[current, i], alpha);
                    double distance = Math.Pow(1.0 / distances[current, i], beta);
                    probabilities[i] = pheromone * distance;
                    sum += probabilities[i];
                }
            }

            double randomValue = rand.NextDouble() * sum;
            double cumulative = 0;

            for (int i = 0; i < cities.Count; i++)
            {
                if (!visited[i])
                {
                    cumulative += probabilities[i];
                    if (cumulative >= randomValue)
                        return i;
                }
            }

            // Fallback
            for (int i = 0; i < cities.Count; i++)
            {
                if (!visited[i])
                    return i;
            }

            return -1;
        }

        private double CalculateTourDistance(List<int> tour)
        {
            double distance = 0;
            for (int i = 0; i < tour.Count - 1; i++)
            {
                distance += distances[tour[i], tour[i + 1]];
            }
            distance += distances[tour[tour.Count - 1], tour[0]];
            return distance;
        }
    }
}