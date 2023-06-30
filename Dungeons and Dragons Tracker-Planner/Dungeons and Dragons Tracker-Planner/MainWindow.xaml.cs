using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Neo4j.Driver;

namespace Dungeons_and_Dragons_Tracker_Planner
{
    // Interaction logic for MainWindow.xaml
    public partial class MainWindow : Window
    {

        // Neo4j .net driver
        private IDriver _driver;

        private Sidebar sidebar;

        private FlowChart flowChart;

        public void GraphDriver_Init(string uri)
        {
            _driver = GraphDatabase.Driver(uri);
        }

        public MainWindow()
        {
            InitializeComponent();
            GraphDriver_Init("bolt://localhost:7687");
            flowChart = new FlowChart();
            sidebar = new Sidebar(_driver, flowChart);
        }

        public void Dispose()
        {
            _driver?.Dispose();
        }

        private void SearchText_Changed(object sender, RoutedEventArgs e)
        {
            sidebar.SearchText_Changed();
        }

        private void Back_Btn_Clicked(object sender, RoutedEventArgs e)
        {
            sidebar.Back_Btn_Clicked();
        }

        private void Campaign_Btn_Click(object sender, RoutedEventArgs e)
        {
            sidebar.Campaign_Btn_Click();

            sidebar.sidebar_nav_states.Add("Campaigns");

        }

        internal void ResultCampaign_Click(object sender, RoutedEventArgs e)
        {
            sidebar.sidebar_state++;

            Button button = e.Source as Button;

            sidebar.current_campaign = button.Content.ToString();

            if (sidebar.sidebar_nav_states.Count() == 0)
            {
                sidebar.sidebar_nav_states.Add("Campaigns");
            }

            sidebar.sidebar_nav_states.Add(sidebar.current_campaign);

            Secondary_st_pnl.Children.Clear();
            Adventure_Btn_Grid.Visibility = Visibility.Visible;
            NPCs_Btn_Grid.Visibility = Visibility.Visible;
            Encounters_Btn_Grid.Visibility = Visibility.Visible;

        }

        internal void ResultAdventure_Click(object sender, RoutedEventArgs e)
        {
            if (sidebar.sidebar_state == 1) sidebar.sidebar_state++; else sidebar.sidebar_state += 2;

            Button button = e.Source as Button;

            sidebar.current_adventure = button.Content.ToString();

            if (sidebar.sidebar_nav_states.Count() == 0)
            {
                sidebar.sidebar_nav_states.Add("Adventures");
            }

            sidebar.sidebar_nav_states.Add(sidebar.current_adventure);

            Secondary_st_pnl.Children.Clear();
            NPCs_Btn_Grid.Visibility = Visibility.Visible;
            Encounters_Btn_Grid.Visibility = Visibility.Visible;
        }

        private void Adventure_Btn_Click(object sender, RoutedEventArgs e)
        {
            sidebar.Adventure_Btn_Click();

            sidebar.sidebar_nav_states.Add("Adventures");
        }

        private void NPC_Btn_Click(object sender, RoutedEventArgs e)
        {
            sidebar.NPC_Btn_Click();

            sidebar.sidebar_nav_states.Add("NPCs");

        }

        private void Encounter_Btn_Click(object sender, RoutedEventArgs e)
        {
            sidebar.Encounter_Btn_Click();

            sidebar.sidebar_nav_states.Add("Encounters");

        }

        private void Creation_Btn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PreviewDown(object sender, MouseButtonEventArgs e)
        {
            flowChart.PreviewDown(sender, e);
        }

        private void PreviewUp(object sender, MouseButtonEventArgs e)
        {
            flowChart.PreviewUp();
        }

        private void Zoom(object sender, MouseWheelEventArgs e)
        {
            flowChart.Zoom(e.Delta, e.GetPosition(container_canvas));
        }

        private void MoveMouse(object sender, MouseEventArgs e)
        {
            flowChart.MoveMouse(sender, e);
        }
    }
}
