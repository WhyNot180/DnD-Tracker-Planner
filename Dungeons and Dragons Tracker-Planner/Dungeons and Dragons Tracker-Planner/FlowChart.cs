using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using System.Windows.Input;

namespace Dungeons_and_Dragons_Tracker_Planner
{
    internal class FlowChart
    {

        MainWindow targetWindow = Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow;

        private Canvas canvas, container_canvas;

        private List<PathPair> pathGrids = new List<PathPair>();

        // The container object (container_canvas)
        private object movingObject;

        // Initial x and y of chartEntries
        private List<double> firstXPos = new List<double>(), firstYPos = new List<double>();

        // Initial line properties of chartPaths
        private List<LineGeometry> firstLine = new List<LineGeometry>();

        private Grid entityClicked;

        public FlowChart() 
        {
            canvas = targetWindow.canvas;
            container_canvas = targetWindow.container_canvas;

        }

        internal void Create_Chart_Entries(List<string> names)
        {
            container_canvas.Children.Clear();
            pathGrids.Clear();

            Random random = new Random();
            foreach (var name in names)
            {
                Grid grid = new Grid();
                Viewbox poly_view = new Viewbox();
                Viewbox text_view = new Viewbox();
                Polygon poly = new Polygon();
                TextBlock text = new TextBlock();

                container_canvas.Children.Add(grid);

                // Name doesn't accept spaces or colons. Might need to be updated for all special characters
                grid.Name = name.Replace(" ", "_").Replace(":", "");
                grid.Children.Add(poly_view);
                grid.Children.Add(text_view);

                poly_view.Child = poly;

                text_view.Child = text;

                grid.Style = (Style)canvas.FindResource("chart_grid");

                poly_view.Style = (Style)canvas.FindResource("chart_poly_viewbox");

                text_view.Style = (Style)canvas.FindResource("chart_text_viewbox");

                poly.Style = (Style)canvas.FindResource("chart_poly");

                text.Style = (Style)canvas.FindResource("chart_text");

                text.Text = name;
                
                // Temp positioning. Nodes should have X,Y property to save location data.
                grid.SetValue(Canvas.LeftProperty, random.Next(-1000, 1000) + random.NextDouble());
                grid.SetValue(Canvas.TopProperty, random.Next(-1000, 1000) + random.NextDouble());

                Panel.SetZIndex(grid, 1);

            }


        }

        internal void Create_Chart_Lines(List<string> secondary_names, List<string> relationships)
        {
            pathGrids.Clear();

            for (var i = 0; i < container_canvas.Children.OfType<Grid>().Count(); i++)
            {
                var grid = container_canvas.Children.OfType<Grid>().ElementAt(i);
                if (relationships.ElementAtOrDefault(i) != null && secondary_names.ElementAtOrDefault(i) != null)
                {

                    Path path = new Path();
                    LineGeometry line = new LineGeometry();
                    Grid secondaryGrid = container_canvas.Children.OfType<Grid>().ToList().Find(g => g.Name.Equals(secondary_names.ElementAt(i).Replace(" ", "_").Replace(":", "")));

                    container_canvas.Children.Add(path);

                    line.StartPoint = new Point((double)grid.GetValue(Canvas.LeftProperty) + grid.Width / 2, (double)grid.GetValue(Canvas.TopProperty) + grid.Height / 2);
                    line.EndPoint = new Point((double)secondaryGrid.GetValue(Canvas.LeftProperty) + secondaryGrid.Width / 2, (double)secondaryGrid.GetValue(Canvas.TopProperty) + secondaryGrid.Height / 2);
                    
                    path.Data = line;

                    path.Stroke = Brushes.Black;
                    path.StrokeThickness = 5;

                    Panel.SetZIndex(path, 0);

                    pathGrids.Add(new PathPair(path, grid, secondaryGrid));
                }
            }
        }

        internal void PreviewDown(object sender, MouseButtonEventArgs e)
        {
            foreach (Grid chartEntity in container_canvas.Children.OfType<Grid>())
            {
                if (chartEntity.IsMouseOver) entityClicked = chartEntity;
                firstXPos.Add(e.GetPosition(chartEntity).X);
                firstYPos.Add(e.GetPosition(chartEntity).Y);
            }

            foreach (Path chartPath in container_canvas.Children.OfType<Path>())
            {
                firstXPos.Add(e.GetPosition(chartPath).X);
                firstYPos.Add(e.GetPosition(chartPath).Y);
                firstLine.Add((LineGeometry)chartPath.Data);
            }

            movingObject = sender;
        }

        internal void PreviewUp()
        {
            movingObject = null;
            firstXPos.Clear();
            firstYPos.Clear();
            firstLine.Clear();
            entityClicked = null;
        }

        internal void Zoom(double delta, Point position)
        {
            var scale = delta >= 0 ? 1.1 : (1.0 / 1.1); // Scales by 1.1 each time scrolling is detected

            var transform = (MatrixTransform)container_canvas.RenderTransform;
            var matrix = transform.Matrix;
            matrix.ScaleAtPrepend(scale, scale, position.X, position.Y); // Scales at mouse position
            container_canvas.RenderTransform = new MatrixTransform(matrix);
        }

        internal void MoveMouse(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && sender == movingObject)
            {

                // If entity is being dragged or everything is
                if (entityClicked != null)
                {
                    double newLeft = e.GetPosition(container_canvas).X - firstXPos.ElementAt(container_canvas.Children.IndexOf(entityClicked)) - canvas.Margin.Left;

                    entityClicked.SetValue(Canvas.LeftProperty, newLeft);

                    double newTop = e.GetPosition(container_canvas).Y - firstYPos.ElementAt(container_canvas.Children.IndexOf(entityClicked)) - canvas.Margin.Top;

                    entityClicked.SetValue(Canvas.TopProperty, newTop);

                    foreach (PathPair pathGrid in pathGrids.Where(g => g.primaryGrid == entityClicked || g.secondaryGrid == entityClicked))
                    {
                        LineGeometry line = firstLine.ElementAt(container_canvas.Children.IndexOf(pathGrid.path) - container_canvas.Children.OfType<Grid>().Count());

                        // Offset is required, as origin of path changes when they are moved
                        // Left and Top properties return NaN when initialized
                        double leftOffset = !Double.IsNaN((double)pathGrid.path.GetValue(Canvas.LeftProperty)) ? (double)pathGrid.path.GetValue(Canvas.LeftProperty) : 0;
                        double topOffset = !Double.IsNaN((double)pathGrid.path.GetValue(Canvas.TopProperty)) ? (double)pathGrid.path.GetValue(Canvas.TopProperty) : 0;

                        // Places start/end point at same position as chartEntry
                        if (pathGrid.primaryGrid == entityClicked)
                        {
                            line.StartPoint = new System.Windows.Point(newLeft + entityClicked.Width / 2 - leftOffset, newTop + entityClicked.Height / 2 - topOffset);
                        }

                        if (pathGrid.secondaryGrid == entityClicked)
                        {
                            line.EndPoint = new System.Windows.Point(newLeft + entityClicked.Width / 2 - leftOffset, newTop + entityClicked.Height / 2 - topOffset);
                        }

                        pathGrid.path.Data = line;
                    }

                    return;
                }


                foreach (Grid chartEntity in container_canvas.Children.OfType<Grid>())
                {
                    // X pos of cursor - initial pos of cursor on element
                    double newLeft = e.GetPosition(container_canvas).X - firstXPos.ElementAt(container_canvas.Children.IndexOf(chartEntity)) - canvas.Margin.Left;

                    chartEntity.SetValue(Canvas.LeftProperty, newLeft);
                    
                    // Y pos of cursor - initial pos of cursor on element
                    double newTop = e.GetPosition(container_canvas).Y - firstYPos.ElementAt(container_canvas.Children.IndexOf(chartEntity)) - canvas.Margin.Top;

                    chartEntity.SetValue(Canvas.TopProperty, newTop);
                }

                foreach (Path chartPath in container_canvas.Children.OfType<Path>())
                {
                    double newLeft = e.GetPosition(container_canvas).X - firstXPos.ElementAt(container_canvas.Children.IndexOf(chartPath)) - canvas.Margin.Left;

                    chartPath.SetValue(Canvas.LeftProperty, newLeft);

                    double newTop = e.GetPosition(container_canvas).Y - firstYPos.ElementAt(container_canvas.Children.IndexOf(chartPath)) - canvas.Margin.Top;

                    chartPath.SetValue(Canvas.TopProperty, newTop);

                }
            }
        }

    }
}
