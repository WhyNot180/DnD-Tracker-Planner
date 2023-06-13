﻿using System;
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
            
        }

        private List<string> sidebar_nav_states = new List<string>();

        private void Back_Btn_Clicked(object sender, RoutedEventArgs e)
        {

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
        private string current_encounter;

        private async void Campaign_Btn_Click()
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
            sidebar_nav_states.Add(current_adventure);

            Secondary_st_pnl.Children.Clear();
            NPCs_Btn_Grid.Visibility = Visibility.Visible;
            Encounters_Btn_Grid.Visibility = Visibility.Visible;
        }

        private void ResultNPC_Click(object sender, RoutedEventArgs e)
        {
            Button button = e.Source as Button;

            current_npc = button.Content.ToString();
        }

        private void ResultEncounter_Click(object sender, RoutedEventArgs e)
        {
            Button button = e.Source as Button;

            current_encounter = button.Content.ToString();
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
                    SearchText_Grid.Visibility = Visibility.Visible;

                    var encounters = await sessionRead("MATCH (e:Encounter) RETURN e.name");

                    Create_Content_Btns(encounters, new RoutedEventHandler(ResultNPC_Click));

                    break;

                case 1:
                    Adventure_Btn_Grid.Visibility = Visibility.Collapsed;
                    NPCs_Btn_Grid.Visibility = Visibility.Collapsed;
                    Encounters_Btn_Grid.Visibility = Visibility.Collapsed;

                    var campaign_encounters = await sessionRead("MATCH (e:Encounter)-[:BELONGS_TO]->(a:Adventure)-[:BELONGS_TO]->(c:Campaign) " +
                                    $"WHERE c.name = \"{current_campaign}\" " +
                                    " RETURN e.name");

                    Create_Content_Btns(campaign_encounters, new RoutedEventHandler(ResultNPC_Click));

                    break;

                case 2:
                    NPCs_Btn_Grid.Visibility = Visibility.Collapsed;
                    Encounters_Btn_Grid.Visibility = Visibility.Collapsed;

                    var adventure_encounters = await sessionRead("MATCH (e:Encounter)-[:BELONGS_TO]->(a:Adventure) " +
                                    $"WHERE a.name = \"{current_adventure}\" " +
                                    " RETURN e.name");

                    Create_Content_Btns(adventure_encounters, new RoutedEventHandler(ResultNPC_Click));

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
    }
}
