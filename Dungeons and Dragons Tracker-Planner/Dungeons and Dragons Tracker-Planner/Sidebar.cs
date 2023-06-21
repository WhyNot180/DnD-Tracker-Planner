using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using Neo4j.Driver;

namespace Dungeons_and_Dragons_Tracker_Planner
{
    internal class Sidebar
    {

        private int sidebar_state = 0;

        MainWindow targetWindow = Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow;

        private Grid Campaign_Btn_Grid, Adventure_Btn_Grid, NPCs_Btn_Grid, Encounters_Btn_Grid, Back_Btn_Grid;

        private StackPanel st_pnl, Secondary_st_pnl;

        private TextBox SearchText;

        private IDriver _driver;

        // Reads from the database and creates a button in the sidebar for each result
        private async void Search_SessionReader(string query)
        {
            var results = await sessionRead(query, 2);

            for (var i = 0; i < results[0].Count; i++)
            {
                Grid grid = new Grid();
                Button button = new Button();
                Secondary_st_pnl.Children.Add(grid);
                grid.Children.Add(button);
                grid.Style = (Style)st_pnl.FindResource("sidebar_grids");
                button.Style = (Style)st_pnl.FindResource("sidebar_buttons");
                button.Content = results[0][i];

                switch (results[1][i])
                {
                    case "Campaign":
                        button.Click += targetWindow.ResultCampaign_Click;
                        break;

                    case "Adventure":
                        button.Click += targetWindow.ResultAdventure_Click;
                        break;
                }
            }
        }

        // Eventhandler for search bar
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

                    Create_Content_Btns(campaigns[0], new RoutedEventHandler(targetWindow.ResultCampaign_Click));

                    break;

                case "Adventures":

                    var adventures = await sessionRead($"MATCH (a:Adventure) WHERE a.name CONTAINS \"{SearchText.Text}\" RETURN a.name", 1);

                    Create_Content_Btns(adventures[0], new RoutedEventHandler(targetWindow.ResultAdventure_Click));

                    break;
                case "NPCs":

                    var npcs = await sessionRead($"MATCH (n:NPC) WHERE n.name CONTAINS \"{SearchText.Text}\" RETURN n.name", 1);

                    Create_Content_Btns(npcs[0], null);

                    break;
                case "Encounters":

                    var encounters = await sessionRead($"MATCH (e:Encounter) WHERE e.name CONTAINS \"{SearchText.Text}\" RETURN e.name", 1);

                    Create_Content_Btns(encounters[0], null);

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

                            Search_SessionReader($@"MATCH (n)-[:BELONGS_TO]->(a:Adventure)-[:BELONGS_TO]->(c:Campaign) 
                                WHERE n.name CONTAINS ""{SearchText.Text}""
                                RETURN n.name, labels(n)");

                            Search_SessionReader($@"MATCH (n)-[:BELONGS_TO]->(c:Campaign) 
                                WHERE n.name CONTAINS ""{SearchText.Text}""
                                RETURN n.name, labels(n)");

                            break;
                        case "Adventures":

                            if (SearchText.Text.Equals(""))
                            {
                                NPCs_Btn_Grid.Visibility = Visibility.Visible;
                                Encounters_Btn_Grid.Visibility = Visibility.Visible;
                                return;
                            }

                            Search_SessionReader($@"MATCH (n)-[:BELONGS_TO]->(a:Adventure)-[:BELONGS_TO]->(c:Campaign) 
                                WHERE n.name CONTAINS ""{SearchText.Text}""
                                RETURN n.name, labels(n)");

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
                                    resultsList[i].Add(reader.Current[i].ToString());
                                }
                                else resultsList[i].Add(null);
                            }
                        }

                        return resultsList;
                    });
                return queryResults;
            }
        }

        // Creates sidebar buttons from names list and assigns a provided eventhandler
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
                if (eventHandler != null) button.Click += eventHandler;
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

            Create_Chart_Entries(campaigns[0]);

            Create_Content_Btns(campaigns[0], new RoutedEventHandler(targetWindow.ResultCampaign_Click));
        }

        private async void Adventure_Btn_Click()
        {
            switch (sidebar_state)
            {
                // If Button is pressed from root
                case 0:
                    Campaign_Btn_Grid.Visibility = Visibility.Collapsed;
                    Adventure_Btn_Grid.Visibility = Visibility.Collapsed;
                    NPCs_Btn_Grid.Visibility = Visibility.Collapsed;
                    Encounters_Btn_Grid.Visibility = Visibility.Collapsed;

                    Back_Btn_Grid.Visibility = Visibility.Visible;

                    var adventures = await sessionRead("MATCH (a:Adventure) OPTIONAL MATCH (a)-[r]->(b:Adventure) RETURN a.name, b.name, type(r)", 3);

                    Create_Chart_Entries(adventures[0]);

                    Create_Chart_Lines(adventures[1], adventures[2]);

                    Create_Content_Btns(adventures[0], new RoutedEventHandler(targetWindow.ResultAdventure_Click));



                    break;

                // If button is pressed from campaign
                case 1:
                    Adventure_Btn_Grid.Visibility = Visibility.Collapsed;
                    NPCs_Btn_Grid.Visibility = Visibility.Collapsed;
                    Encounters_Btn_Grid.Visibility = Visibility.Collapsed;

                    var campaign_adventures = await sessionRead($@"MATCH (a:Adventure)-[:BELONGS_TO]->(c:Campaign) 
                                    WHERE c.name = ""{current_campaign}"" 
                                    OPTIONAL MATCH (a)-[r]->(b:Adventure) 
                                    RETURN a.name, b.name, type(r)", 3);

                    Create_Chart_Entries(campaign_adventures[0]);

                    Create_Chart_Lines(campaign_adventures[1], campaign_adventures[2]);

                    Create_Content_Btns(campaign_adventures[0], new RoutedEventHandler(targetWindow.ResultAdventure_Click));

                    break;

                default:
                    Console.WriteLine("Adventure defaulted!!!");
                    break;
            }
        }

        private async void NPC_Btn_Click()
        {
            switch (sidebar_state)
            {
                // If button is pressed from root
                case 0:
                    Campaign_Btn_Grid.Visibility = Visibility.Collapsed;
                    Adventure_Btn_Grid.Visibility = Visibility.Collapsed;
                    NPCs_Btn_Grid.Visibility = Visibility.Collapsed;
                    Encounters_Btn_Grid.Visibility = Visibility.Collapsed;

                    Back_Btn_Grid.Visibility = Visibility.Visible;

                    var npcs = await sessionRead("MATCH (n:NPC) OPTIONAL MATCH (n)-[r]->(b:NPC) RETURN n.name, b.name, type(r)", 3);

                    Create_Chart_Entries(npcs[0]);

                    Create_Chart_Lines(npcs[1], npcs[2]);

                    Create_Content_Btns(npcs[0], null);

                    break;

                // If button is pressed from Campaign
                case 1:
                    Adventure_Btn_Grid.Visibility = Visibility.Collapsed;
                    NPCs_Btn_Grid.Visibility = Visibility.Collapsed;
                    Encounters_Btn_Grid.Visibility = Visibility.Collapsed;

                    var campaign_npcs = await sessionRead($@"MATCH (n:NPC)-[:BELONGS_TO]->(a:Adventure)-[:BELONGS_TO]->(c:Campaign) 
                                    WHERE c.name = ""{current_campaign}"" 
                                    OPTIONAL MATCH (n)-[r]->(b:NPC) 
                                    RETURN n.name, b.name, type(r)", 3);

                    Create_Chart_Entries(campaign_npcs[0]);

                    Create_Chart_Lines(campaign_npcs[1], campaign_npcs[2]);

                    Create_Content_Btns(campaign_npcs[0], null);

                    break;

                // If button is pressed from adventure
                case 2:
                    NPCs_Btn_Grid.Visibility = Visibility.Collapsed;
                    Encounters_Btn_Grid.Visibility = Visibility.Collapsed;

                    var adventure_npcs = await sessionRead($@"MATCH (n:NPC)-[:BELONGS_TO]->(a:Adventure) 
                                    WHERE a.name = ""{current_adventure}"" 
                                    OPTIONAL MATCH (n)-[r]->(b:NPC) 
                                    RETURN n.name, b.name, type(r)", 3);

                    Create_Chart_Entries(adventure_npcs[0]);

                    Create_Chart_Lines(adventure_npcs[1], adventure_npcs[2]);

                    Create_Content_Btns(adventure_npcs[0], null);

                    break;

                default:
                    Console.WriteLine("NPC defaulted!!!");
                    break;
            }
        }

        private async void Encounter_Btn_Click()
        {
            switch (sidebar_state)
            {
                // If button is pressed from root
                case 0:
                    Campaign_Btn_Grid.Visibility = Visibility.Collapsed;
                    Adventure_Btn_Grid.Visibility = Visibility.Collapsed;
                    NPCs_Btn_Grid.Visibility = Visibility.Collapsed;
                    Encounters_Btn_Grid.Visibility = Visibility.Collapsed;

                    Back_Btn_Grid.Visibility = Visibility.Visible;

                    var encounters = await sessionRead("MATCH (e:Encounter) OPTIONAL MATCH (e)-[r]->(b:Encounter) RETURN e.name, b.name, type(r)", 3);

                    Create_Chart_Entries(encounters[0]);

                    Create_Chart_Lines(encounters[1], encounters[2]);

                    Create_Content_Btns(encounters[0], null);

                    break;

                // If button is pressed from Campaign
                case 1:
                    Adventure_Btn_Grid.Visibility = Visibility.Collapsed;
                    NPCs_Btn_Grid.Visibility = Visibility.Collapsed;
                    Encounters_Btn_Grid.Visibility = Visibility.Collapsed;

                    var campaign_encounters = await sessionRead($@"MATCH (e:Encounter)-[:BELONGS_TO]->(a:Adventure)-[:BELONGS_TO]->(c:Campaign) 
                                    WHERE c.name = ""{current_campaign}"" 
                                    OPTIONAL MATCH (e)-[r]->(b:Encounter) 
                                    RETURN e.name, b.name, type(r)", 3);

                    Create_Chart_Entries(campaign_encounters[0]);

                    Create_Chart_Lines(campaign_encounters[1], campaign_encounters[2]);

                    Create_Content_Btns(campaign_encounters[0], null);

                    break;

                // If button is pressed from adventure
                case 2:
                    NPCs_Btn_Grid.Visibility = Visibility.Collapsed;
                    Encounters_Btn_Grid.Visibility = Visibility.Collapsed;

                    var adventure_encounters = await sessionRead($@"MATCH (e:Encounter)-[:BELONGS_TO]->(a:Adventure) 
                                    WHERE a.name = ""{current_adventure}"" 
                                    OPTIONAL MATCH (e)-[r]->(b:Encounter) 
                                    RETURN e.name, b.name, type(r)", 3);

                    Create_Chart_Entries(adventure_encounters[0]);

                    Create_Chart_Lines(adventure_encounters[1], adventure_encounters[2]);

                    Create_Content_Btns(adventure_encounters[0], null);

                    break;

                default:
                    Console.WriteLine("Encounter defaulted!!!");
                    break;
            }
        }

        public Sidebar(IDriver _driver) 
        {
            this._driver = _driver;

            Campaign_Btn_Grid = targetWindow.Campaign_Btn_Grid;
            Adventure_Btn_Grid = targetWindow.Adventure_Btn_Grid;
            NPCs_Btn_Grid = targetWindow.NPCs_Btn_Grid;
            Encounters_Btn_Grid = targetWindow.Encounters_Btn_Grid;
            Back_Btn_Grid = targetWindow.Back_Btn_Grid;

            st_pnl = targetWindow.st_pnl;
            Secondary_st_pnl = targetWindow.Secondary_st_pnl;

            SearchText = targetWindow.SearchText;
        }
    }
}
