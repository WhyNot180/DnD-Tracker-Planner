using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Neo4j.Driver;

namespace Dungeons_and_Dragons_Tracker_Planner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IDriver _driver;

        public void GraphDriver_Init(string uri)
        {
            _driver = GraphDatabase.Driver(uri);
        }

        public MainWindow()
        {
            InitializeComponent();
            GraphDriver_Init("bolt://localhost:7687");
        }

        public void Dispose()
        {
            _driver?.Dispose();
        }

        // Sidebar

        private int sidebar_state = 0;

        private async void Search_SessionReader(string query)
        {
            var results = await sessionRead(query, 2);

            for (var i = 0; i < results.ElementAt(0).Count; i++)
            {
                Grid grid = new Grid();
                Button button = new Button();
                Secondary_st_pnl.Children.Add(grid);
                grid.Children.Add(button);
                grid.Style = (Style)st_pnl.FindResource("sidebar_grids");
                button.Style = (Style)st_pnl.FindResource("sidebar_buttons");
                button.Content = results.ElementAt(0).ElementAt(i);
                if (results.ElementAt(1).ElementAt(i).Equals("Campaign"))
                {
                    button.Click += ResultCampaign_Click;
                }
                else if (results.ElementAt(1).ElementAt(i).Equals("Adventure"))
                {
                    button.Click += ResultAdventure_Click;
                }
                else if (results.ElementAt(1).ElementAt(i).Equals("NPC"))
                {
                    button.Click += ResultNPC_Click;
                }
                else if (results.ElementAt(1).ElementAt(i).Equals("Encounter"))
                {
                    button.Click += ResultEncounter_Click;
                }
            }
        }

        private async void SearchText_Changed(object sender, RoutedEventArgs e)
        {

            Secondary_st_pnl.Children.Clear();

            if (sidebar_nav_states.Count == 0)
            {
                if (SearchText.Text.Equals(""))
                {
                    Campaign_Btn_Grid.Visibility = Visibility.Visible;
                    Adventure_Btn_Grid.Visibility = Visibility.Visible;
                    NPCs_Btn_Grid.Visibility = Visibility.Visible;
                    Encounters_Btn_Grid.Visibility = Visibility.Visible;
                    return;
                }

                Campaign_Btn_Grid.Visibility = Visibility.Collapsed;
                Adventure_Btn_Grid.Visibility = Visibility.Collapsed;
                NPCs_Btn_Grid.Visibility = Visibility.Collapsed;
                Encounters_Btn_Grid.Visibility = Visibility.Collapsed;

                Search_SessionReader($"MATCH (n) WHERE n.name CONTAINS \"{SearchText.Text}\" RETURN n.name, labels(n)");
                return;
            }

            Campaign_Btn_Grid.Visibility = Visibility.Collapsed;
            Adventure_Btn_Grid.Visibility = Visibility.Collapsed;
            NPCs_Btn_Grid.Visibility = Visibility.Collapsed;
            Encounters_Btn_Grid.Visibility = Visibility.Collapsed;

            switch (sidebar_nav_states.Last())
            {
                case "Campaigns":

                    var campaigns = await sessionRead($"MATCH (c:Campaign) WHERE c.name CONTAINS \"{SearchText.Text}\" RETURN c.name", 1);

                    Create_Content_Btns(campaigns.ElementAt(0), new RoutedEventHandler(ResultCampaign_Click));

                    break;

                case "Adventures":

                    var adventures = await sessionRead($"MATCH (a:Adventure) WHERE a.name CONTAINS \"{SearchText.Text}\" RETURN a.name", 1);

                    Create_Content_Btns(adventures.ElementAt(0), new RoutedEventHandler(ResultAdventure_Click));

                    break;
                case "NPCs":

                    var npcs = await sessionRead($"MATCH (n:NPC) WHERE n.name CONTAINS \"{SearchText.Text}\" RETURN n.name", 1);

                    Create_Content_Btns(npcs.ElementAt(0), new RoutedEventHandler(ResultNPC_Click));

                    break;
                case "Encounters":

                    var encounters = await sessionRead($"MATCH (e:Encounter) WHERE e.name CONTAINS \"{SearchText.Text}\" RETURN e.name", 1);

                    Create_Content_Btns(encounters.ElementAt(0), new RoutedEventHandler(ResultEncounter_Click));

                    break;
                default:

                    switch (sidebar_nav_states.ElementAt(sidebar_nav_states.Count() - 2))
                    {
                        case "Campaigns":

                            if (SearchText.Text.Equals(""))
                            {
                                Adventure_Btn_Grid.Visibility = Visibility.Visible;
                                NPCs_Btn_Grid.Visibility = Visibility.Visible;
                                Encounters_Btn_Grid.Visibility = Visibility.Visible;
                                return;
                            }

                            Search_SessionReader("MATCH (n)-[:BELONGS_TO]->(a:Adventure)-[:BELONGS_TO]->(c:Campaign) " +
                                $"WHERE n.name CONTAINS \"{SearchText.Text}\" " +
                                "RETURN n.name, labels(n)");

                            Search_SessionReader("MATCH (n)-[:BELONGS_TO]->(c:Campaign) " +
                                $"WHERE n.name CONTAINS \"{SearchText.Text}\" " +
                                "RETURN n.name, labels(n)");

                            break;
                        case "Adventures":

                            if (SearchText.Text.Equals(""))
                            {
                                NPCs_Btn_Grid.Visibility = Visibility.Visible;
                                Encounters_Btn_Grid.Visibility = Visibility.Visible;
                                return;
                            }

                            Search_SessionReader("MATCH (n)-[:BELONGS_TO]->(a:Adventure)-[:BELONGS_TO]->(c:Campaign) " +
                                $"WHERE n.name CONTAINS \"{SearchText.Text}\" " +
                                "RETURN n.name, labels(n)");

                            break;
                    }

                    break;
            }
        }

        private List<string> sidebar_nav_states = new List<string>();

        private void Back_Btn_Clicked(object sender, RoutedEventArgs e)
        {

            SearchText.Text = "";

            sidebar_nav_states.RemoveAt(sidebar_nav_states.Count - 1);
            Secondary_st_pnl.Children.Clear();

            if (sidebar_nav_states.Count() == 0)
            {
                Campaign_Btn_Grid.Visibility = Visibility.Visible;
                Adventure_Btn_Grid.Visibility = Visibility.Visible;
                NPCs_Btn_Grid.Visibility = Visibility.Visible;
                Encounters_Btn_Grid.Visibility = Visibility.Visible;

                Back_Btn_Grid.Visibility = Visibility.Collapsed;
                return;
            }

            switch (sidebar_nav_states.Last())
            {
                case "Campaigns":
                    if (sidebar_state != 0) sidebar_state--;
                    Campaign_Btn_Click();

                    break;

                case "Adventures":
                    if (sidebar_state == 2) sidebar_state -= 2; else sidebar_state--;
                    Adventure_Btn_Click();

                    break;

                case "NPCs":

                    NPC_Btn_Click();

                    break;

                case "Encounters":

                    Encounter_Btn_Click();

                    break;

                default:

                    if (sidebar_nav_states.Count() - 2 < 0)
                    {
                        break;
                    }

                    switch (sidebar_nav_states.ElementAt(sidebar_nav_states.Count() - 2))
                    {
                        case "Campaigns":

                            Adventure_Btn_Grid.Visibility = Visibility.Visible;
                            NPCs_Btn_Grid.Visibility = Visibility.Visible;
                            Encounters_Btn_Grid.Visibility = Visibility.Visible;

                            Back_Btn_Grid.Visibility = Visibility.Visible;
                            break;
                        case "Adventures":

                            NPCs_Btn_Grid.Visibility = Visibility.Visible;
                            Encounters_Btn_Grid.Visibility = Visibility.Visible;

                            Back_Btn_Grid.Visibility = Visibility.Visible;
                            break;
                    }

                    break;
            }
        }

        private async Task<List<List<string>>> sessionRead(string query, int numberOfColumns)
        {
            using (var session = _driver.AsyncSession())
            {
                var queryResults = await session.ExecuteReadAsync(
                    async tx =>
                    {
                        var resultsList = new List<List<string>>();
                        
                        for (int i = 0; i < numberOfColumns; i++)
                        {
                            resultsList.Add(new List<string>());
                        }

                        var reader = await tx.RunAsync(
                            query);

                        while (await reader.FetchAsync())
                        {
                            for (int i = 0; i < numberOfColumns; i++)
                            {
                                if (reader.Current[i] != null)
                                {
                                    resultsList.ElementAt(i).Add(reader.Current[i].ToString());
                                }
                                else resultsList.ElementAt(i).Add(null);
                            }
                        }

                        return resultsList;
                    });
                return queryResults;
            }
        }

        private void Create_Content_Btns(List<string> names, RoutedEventHandler eventHandler)
        {
            foreach (var name in names)
            {
                Grid grid = new Grid();
                Button button = new Button();
                Secondary_st_pnl.Children.Add(grid);
                grid.Children.Add(button);
                grid.Style = (Style)st_pnl.FindResource("sidebar_grids");
                button.Style = (Style)st_pnl.FindResource("sidebar_buttons");
                button.Content = name;
                button.Click += eventHandler;
            }
        }

        private List<PathPair> pathGrids = new List<PathPair>();

        private void Create_Chart_Entries(List<string> names, List<string> secondary_names, List<string> relationships)
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
                grid.Name = name.Replace(" ", "_").Replace(":", "");
                grid.Children.Add(poly_view);
                grid.Children.Add(text_view);
                poly_view.Child = poly;
                text_view.Child = text;
                grid.Style = (Style) canvas.FindResource("chart_grid");
                poly_view.Style = (Style) canvas.FindResource("chart_poly_viewbox");
                text_view.Style = (Style) canvas.FindResource("chart_text_viewbox");
                poly.Style = (Style) canvas.FindResource("chart_poly");
                text.Style = (Style) canvas.FindResource("chart_text");

                text.Text = name;

                grid.SetValue(Canvas.LeftProperty, random.Next(-1000, 1000) + random.NextDouble());
                grid.SetValue(Canvas.TopProperty, random.Next(-1000, 1000) + random.NextDouble());

                Panel.SetZIndex(grid, 1);

            }

            for (var i = 0; i < names.Count; i++)
            {
                var grid = container_canvas.Children.OfType<Grid>().ElementAt(i);
                if (relationships?.ElementAtOrDefault(i) != null && secondary_names?.ElementAtOrDefault(i) != null)
                {

                    Path path = new Path();
                    LineGeometry line = new LineGeometry();
                    Grid secondaryGrid = container_canvas.Children.OfType<Grid>().ToList().Find(g => g.Name.Equals(secondary_names.ElementAt(i).Replace(" ", "_").Replace(":", "")));
                    
                    container_canvas.Children.Add(path);
                    line.StartPoint = new System.Windows.Point((double)grid.GetValue(Canvas.LeftProperty) + grid.Width/2, (double)grid.GetValue(Canvas.TopProperty) + grid.Height/2);
                    line.EndPoint = new System.Windows.Point((double)secondaryGrid.GetValue(Canvas.LeftProperty) + secondaryGrid.Width/2, (double)secondaryGrid.GetValue(Canvas.TopProperty) + secondaryGrid.Height/2);
                    path.Data = line;
                    path.Stroke = Brushes.Black;
                    path.StrokeThickness = 5;
                    Panel.SetZIndex(path, 0);
                    pathGrids.Add(new PathPair(path, grid, secondaryGrid));
                }
            }
        }

        private string current_campaign;
        private string current_adventure;

        private async void Campaign_Btn_Click()
        {
            Campaign_Btn_Grid.Visibility = Visibility.Collapsed;
            Adventure_Btn_Grid.Visibility = Visibility.Collapsed;
            NPCs_Btn_Grid.Visibility = Visibility.Collapsed;
            Encounters_Btn_Grid.Visibility = Visibility.Collapsed;

            Back_Btn_Grid.Visibility = Visibility.Visible;

            var campaigns = await sessionRead("MATCH (c:Campaign) RETURN c.name", 1);

            Create_Chart_Entries(campaigns.ElementAt(0), null, null);

            Create_Content_Btns(campaigns.ElementAt(0), new RoutedEventHandler(ResultCampaign_Click));
        }

        private void Campaign_Btn_Click(object sender, RoutedEventArgs e)
        {
            Campaign_Btn_Click();

            sidebar_nav_states.Add("Campaigns");

        }

        private void ResultCampaign_Click(object sender, RoutedEventArgs e)
        {
            sidebar_state++;

            Button button = e.Source as Button;

            current_campaign = button.Content.ToString();

            if (sidebar_nav_states.Count() == 0)
            {
                sidebar_nav_states.Add("Campaigns");
            }

            sidebar_nav_states.Add(current_campaign);

            Secondary_st_pnl.Children.Clear();
            Adventure_Btn_Grid.Visibility = Visibility.Visible;
            NPCs_Btn_Grid.Visibility = Visibility.Visible;
            Encounters_Btn_Grid.Visibility = Visibility.Visible;

        }

        private void ResultAdventure_Click(object sender, RoutedEventArgs e)
        {
            if (sidebar_state == 1) sidebar_state++; else sidebar_state += 2;

            Button button = e.Source as Button;

            current_adventure = button.Content.ToString();

            if (sidebar_nav_states.Count() == 0)
            {
                sidebar_nav_states.Add("Adventures");
            }

            sidebar_nav_states.Add(current_adventure);

            Secondary_st_pnl.Children.Clear();
            NPCs_Btn_Grid.Visibility = Visibility.Visible;
            Encounters_Btn_Grid.Visibility = Visibility.Visible;
        }

        private void ResultNPC_Click(object sender, RoutedEventArgs e)
        {
            Button button = e.Source as Button;

        }

        private void ResultEncounter_Click(object sender, RoutedEventArgs e)
        {
            Button button = e.Source as Button;

        }

        private async void Adventure_Btn_Click()
        {
            switch (sidebar_state)
            {
                case 0:
                    Campaign_Btn_Grid.Visibility = Visibility.Collapsed;
                    Adventure_Btn_Grid.Visibility = Visibility.Collapsed;
                    NPCs_Btn_Grid.Visibility = Visibility.Collapsed;
                    Encounters_Btn_Grid.Visibility = Visibility.Collapsed;

                    Back_Btn_Grid.Visibility = Visibility.Visible;

                    var adventures = await sessionRead("MATCH (a:Adventure) OPTIONAL MATCH (a)-[r]->(b:Adventure) RETURN a.name, b.name, type(r)", 3);

                    Create_Chart_Entries(adventures.ElementAt(0), adventures.ElementAt(1), adventures.ElementAt(2));

                    Create_Content_Btns(adventures.ElementAt(0), new RoutedEventHandler(ResultAdventure_Click));



                    break;

                case 1:
                    Adventure_Btn_Grid.Visibility = Visibility.Collapsed;
                    NPCs_Btn_Grid.Visibility = Visibility.Collapsed;
                    Encounters_Btn_Grid.Visibility = Visibility.Collapsed;

                    var campaign_adventures = await sessionRead("MATCH (a:Adventure)-[:BELONGS_TO]->(c:Campaign) " +
                                    $"WHERE c.name = \"{current_campaign}\" " +
                                    "OPTIONAL MATCH (a)-[r]->(b:Adventure) " +
                                    "RETURN a.name, b.name, type(r)", 3);

                    Create_Chart_Entries(campaign_adventures.ElementAt(0), campaign_adventures.ElementAt(1), campaign_adventures.ElementAt(2));

                    Create_Content_Btns(campaign_adventures.ElementAt(0), new RoutedEventHandler(ResultAdventure_Click));

                    break;

                default:
                    Console.WriteLine("Adventure defaulted!!!");
                    break;
            }
        }


        private void Adventure_Btn_Click(object sender, RoutedEventArgs e)
        {
            Adventure_Btn_Click();

            sidebar_nav_states.Add("Adventures");
        }

        private async void NPC_Btn_Click()
        {
            switch (sidebar_state)
            {
                case 0:
                    Campaign_Btn_Grid.Visibility = Visibility.Collapsed;
                    Adventure_Btn_Grid.Visibility = Visibility.Collapsed;
                    NPCs_Btn_Grid.Visibility = Visibility.Collapsed;
                    Encounters_Btn_Grid.Visibility = Visibility.Collapsed;

                    Back_Btn_Grid.Visibility = Visibility.Visible;

                    var npcs = await sessionRead("MATCH (n:NPC) OPTIONAL MATCH (n)-[r]->(b:NPC) RETURN n.name, b.name, type(r)", 3);

                    Create_Chart_Entries(npcs.ElementAt(0), npcs.ElementAt(1), npcs.ElementAt(2));

                    Create_Content_Btns(npcs.ElementAt(0), new RoutedEventHandler(ResultNPC_Click));

                    break;

                case 1:
                    Adventure_Btn_Grid.Visibility = Visibility.Collapsed;
                    NPCs_Btn_Grid.Visibility = Visibility.Collapsed;
                    Encounters_Btn_Grid.Visibility = Visibility.Collapsed;

                    var campaign_npcs = await sessionRead("MATCH (n:NPC)-[:BELONGS_TO]->(a:Adventure)-[:BELONGS_TO]->(c:Campaign) " +
                                    $"WHERE c.name = \"{current_campaign}\" " +
                                    "OPTIONAL MATCH (n)-[r]->(b:NPC) " +
                                    "RETURN n.name, b.name, type(r)", 3);

                    Create_Chart_Entries(campaign_npcs.ElementAt(0), campaign_npcs.ElementAt(1), campaign_npcs.ElementAt(2));

                    Create_Content_Btns(campaign_npcs.ElementAt(0), new RoutedEventHandler(ResultNPC_Click));

                    break;

                case 2:
                    NPCs_Btn_Grid.Visibility = Visibility.Collapsed;
                    Encounters_Btn_Grid.Visibility = Visibility.Collapsed;

                    var adventure_npcs = await sessionRead("MATCH (n:NPC)-[:BELONGS_TO]->(a:Adventure) " +
                                    $"WHERE a.name = \"{current_adventure}\" " +
                                    "OPTIONAL MATCH (n)-[r]->(b:NPC) " +
                                    "RETURN n.name, b.name, type(r)", 3);

                    Create_Chart_Entries(adventure_npcs.ElementAt(0), adventure_npcs.ElementAt(1), adventure_npcs.ElementAt(2));

                    Create_Content_Btns(adventure_npcs.ElementAt(0), new RoutedEventHandler(ResultNPC_Click));

                    break;

                default:
                    Console.WriteLine("NPC defaulted!!!");
                    break;
            }
        }

        private void NPC_Btn_Click(object sender, RoutedEventArgs e)
        {
            NPC_Btn_Click();

            sidebar_nav_states.Add("NPCs");

        }

        private async void Encounter_Btn_Click()
        {
            switch (sidebar_state)
            {
                case 0:
                    Campaign_Btn_Grid.Visibility = Visibility.Collapsed;
                    Adventure_Btn_Grid.Visibility = Visibility.Collapsed;
                    NPCs_Btn_Grid.Visibility = Visibility.Collapsed;
                    Encounters_Btn_Grid.Visibility = Visibility.Collapsed;

                    Back_Btn_Grid.Visibility = Visibility.Visible;

                    var encounters = await sessionRead("MATCH (e:Encounter) OPTIONAL MATCH (e)-[r]->(b:Encounter) RETURN e.name, b.name, type(r)", 3);

                    Create_Chart_Entries(encounters.ElementAt(0), encounters.ElementAt(1), encounters.ElementAt(2));

                    Create_Content_Btns(encounters.ElementAt(0), new RoutedEventHandler(ResultNPC_Click));

                    break;

                case 1:
                    Adventure_Btn_Grid.Visibility = Visibility.Collapsed;
                    NPCs_Btn_Grid.Visibility = Visibility.Collapsed;
                    Encounters_Btn_Grid.Visibility = Visibility.Collapsed;

                    var campaign_encounters = await sessionRead("MATCH (e:Encounter)-[:BELONGS_TO]->(a:Adventure)-[:BELONGS_TO]->(c:Campaign) " +
                                    $"WHERE c.name = \"{current_campaign}\" " +
                                    "OPTIONAL MATCH (e)-[r]->(b:Encounter) " +
                                    "RETURN e.name, b.name, type(r)", 3);

                    Create_Chart_Entries(campaign_encounters.ElementAt(0), campaign_encounters.ElementAt(1), campaign_encounters.ElementAt(2));

                    Create_Content_Btns(campaign_encounters.ElementAt(0), new RoutedEventHandler(ResultNPC_Click));

                    break;

                case 2:
                    NPCs_Btn_Grid.Visibility = Visibility.Collapsed;
                    Encounters_Btn_Grid.Visibility = Visibility.Collapsed;

                    var adventure_encounters = await sessionRead("MATCH (e:Encounter)-[:BELONGS_TO]->(a:Adventure) " +
                                    $"WHERE a.name = \"{current_adventure}\" " +
                                    "OPTIONAL MATCH (e)-[r]->(b:Encounter) " +
                                    "RETURN e.name, b.name, type(r)", 3);

                    Create_Chart_Entries(adventure_encounters.ElementAt(0), adventure_encounters.ElementAt(1), adventure_encounters.ElementAt(2));

                    Create_Content_Btns(adventure_encounters.ElementAt(0), new RoutedEventHandler(ResultNPC_Click));

                    break;

                default:
                    Console.WriteLine("Encounter defaulted!!!");
                    break;
            }
        }

        private void Encounter_Btn_Click(object sender, RoutedEventArgs e)
        {
            Encounter_Btn_Click();

            sidebar_nav_states.Add("Encounters");

        }
        
        // Flowchart

        private object movingObject;
        private List<double> firstXPos = new List<double>(), firstYPos = new List<double>();
        private List<LineGeometry> firstLine = new List<LineGeometry>();
        private Grid entityClicked;

        private void PreviewDown(object sender, MouseButtonEventArgs e)
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

        private void PreviewUp(object sender, MouseButtonEventArgs e)
        {
            movingObject = null;
            firstXPos.Clear();
            firstYPos.Clear();
            firstLine.Clear();
            entityClicked = null;
        }

        private void Zoom(object sender, MouseWheelEventArgs e)
        {
            
            var position = e.GetPosition(container_canvas);
            var scale = e.Delta >= 0 ? 1.1 : (1.0 / 1.1); // choose appropriate scaling factor

            var transform = (MatrixTransform)container_canvas.RenderTransform;
            var matrix = transform.Matrix;
            matrix.ScaleAtPrepend(scale, scale, position.X, position.Y);
            container_canvas.RenderTransform = new MatrixTransform(matrix);
            
            
        }

        private void MoveMouse(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && sender == movingObject)
            {
                
                if (entityClicked != null)
                {
                    double newLeft = e.GetPosition(container_canvas).X - firstXPos.ElementAt(container_canvas.Children.IndexOf(entityClicked)) - canvas.Margin.Left;

                    entityClicked.SetValue(Canvas.LeftProperty, newLeft);

                    double newTop = e.GetPosition(container_canvas).Y - firstYPos.ElementAt(container_canvas.Children.IndexOf(entityClicked)) - canvas.Margin.Top;

                    entityClicked.SetValue(Canvas.TopProperty, newTop);

                    foreach (PathPair pathGrid in pathGrids.Where(g => g.primaryGrid == entityClicked || g.secondaryGrid == entityClicked))
                    {
                        LineGeometry line = firstLine.ElementAt(container_canvas.Children.IndexOf(pathGrid.path) - container_canvas.Children.OfType<Grid>().Count());

                        double leftOffset =  !Double.IsNaN((double)pathGrid.path.GetValue(Canvas.LeftProperty)) ? (double)pathGrid.path.GetValue(Canvas.LeftProperty) : 0;
                        double topOffset = !Double.IsNaN((double)pathGrid.path.GetValue(Canvas.TopProperty)) ? (double)pathGrid.path.GetValue(Canvas.TopProperty) : 0;

                        if (pathGrid.primaryGrid == entityClicked)
                        {
                            line.StartPoint = new System.Windows.Point(newLeft + entityClicked.Width/2 - leftOffset, newTop + entityClicked.Height/2 - topOffset);
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
                    double newLeft = e.GetPosition(container_canvas).X - firstXPos.ElementAt(container_canvas.Children.IndexOf(chartEntity)) - canvas.Margin.Left;

                    chartEntity.SetValue(Canvas.LeftProperty, newLeft);

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
