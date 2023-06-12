using System;
using System.Collections.Generic;
using System.Linq;
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

        private int sidebar_state = 0;

        private void SearchText_Changed(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(SearchText.Text);
        }

        private async Task<List<string>> sessionRead(string query)
        {
            using (var session = _driver.AsyncSession())
            {
            var queryResults = await session.ExecuteReadAsync(
                async tx =>
                {

                    var resultsList = new List<string>();

                    var reader = await tx.RunAsync(
                        query);

                    while (await reader.FetchAsync())
                    {
                        resultsList.Add(reader.Current[0].ToString());
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

        private string current_campaign;
        private string current_adventure;
        private string current_npc;

        private async void Campaign_Btn_Click(object sender, RoutedEventArgs e)
        {
            Campaign_Btn_Grid.Visibility = Visibility.Collapsed;
            Adventure_Btn_Grid.Visibility = Visibility.Collapsed;
            NPCs_Btn_Grid.Visibility = Visibility.Collapsed;
            Encounters_Btn_Grid.Visibility = Visibility.Collapsed;

            Back_Btn_Grid.Visibility = Visibility.Visible;
            SearchText_Grid.Visibility = Visibility.Visible;

            var campaigns = await sessionRead("MATCH (c:Campaign) RETURN c.name");

            Create_Content_Btns(campaigns, new RoutedEventHandler(ResultCampaign_Click));
            
        }

        private void ResultCampaign_Click(object sender, RoutedEventArgs e)
        {
            sidebar_state++;
            
            Button button = e.Source as Button;

            current_campaign = button.Content.ToString();

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

            Secondary_st_pnl.Children.Clear();
            NPCs_Btn_Grid.Visibility = Visibility.Visible;
            Encounters_Btn_Grid.Visibility = Visibility.Visible;
        }

        private void ResultNPC_Click(object sender, RoutedEventArgs e)
        {
            Button button = e.Source as Button;

            current_npc = button.Content.ToString();
        }

        private async void Adventure_Btn_Click(object sender, RoutedEventArgs e)
        {
            switch (sidebar_state)
            {
                case 0:
                    Campaign_Btn_Grid.Visibility = Visibility.Collapsed;
                    Adventure_Btn_Grid.Visibility = Visibility.Collapsed;
                    NPCs_Btn_Grid.Visibility = Visibility.Collapsed;
                    Encounters_Btn_Grid.Visibility = Visibility.Collapsed;

                    Back_Btn_Grid.Visibility = Visibility.Visible;
                    SearchText_Grid.Visibility = Visibility.Visible;

                    var adventures = await sessionRead("MATCH (a:Adventure) RETURN a.name");

                    Create_Content_Btns(adventures, new RoutedEventHandler(ResultAdventure_Click));
                    
                    break;

                case 1:
                    Adventure_Btn_Grid.Visibility = Visibility.Collapsed;
                    NPCs_Btn_Grid.Visibility = Visibility.Collapsed;
                    Encounters_Btn_Grid.Visibility = Visibility.Collapsed;

                    var campaign_adventures = await sessionRead("MATCH (a:Adventure)-[:BELONGS_TO]->(c:Campaign) " +
                                    $"WHERE c.name = \"{current_campaign}\" " +
                                    " RETURN a.name");

                    Create_Content_Btns(campaign_adventures, new RoutedEventHandler(ResultAdventure_Click));

                    break;

                default:
                    Console.WriteLine("Adventure defaulted!!!");
                    break;
            }
        }

        private async void NPC_Btn_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(sidebar_state);
            switch (sidebar_state)
            {
                case 0:
                    Campaign_Btn_Grid.Visibility = Visibility.Collapsed;
                    Adventure_Btn_Grid.Visibility = Visibility.Collapsed;
                    NPCs_Btn_Grid.Visibility = Visibility.Collapsed;
                    Encounters_Btn_Grid.Visibility = Visibility.Collapsed;

                    Back_Btn_Grid.Visibility = Visibility.Visible;
                    SearchText_Grid.Visibility = Visibility.Visible;

                    var npcs = await sessionRead("MATCH (n:NPC) RETURN n.name");

                    Create_Content_Btns(npcs, new RoutedEventHandler(ResultNPC_Click));

                    break;

                case 1:
                    Adventure_Btn_Grid.Visibility = Visibility.Collapsed;
                    NPCs_Btn_Grid.Visibility = Visibility.Collapsed;
                    Encounters_Btn_Grid.Visibility = Visibility.Collapsed;

                    var campaign_npcs = await sessionRead("MATCH (n:NPC)-[:BELONGS_TO]->(a:Adventure)-[:BELONGS_TO]->(c:Campaign) " +
                                    $"WHERE c.name = \"{current_campaign}\" " +
                                    " RETURN n.name");

                    Create_Content_Btns(campaign_npcs, new RoutedEventHandler(ResultNPC_Click));

                    break;

                case 2:
                    NPCs_Btn_Grid.Visibility = Visibility.Collapsed;
                    Encounters_Btn_Grid.Visibility = Visibility.Collapsed;

                    var adventure_npcs = await sessionRead("MATCH (n:NPC)-[:BELONGS_TO]->(a:Adventure) " +
                                    $"WHERE a.name = \"{current_adventure}\" " +
                                    " RETURN n.name");

                    Create_Content_Btns(adventure_npcs, new RoutedEventHandler(ResultNPC_Click));

                    break;

                default:
                    Console.WriteLine("NPC defaulted!!!");
                    break;
            }
        }
    }
}
