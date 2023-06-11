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

        private void SearchText_Changed(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(SearchText.Text);
        }

        private async void Campaign_Btn_Click(object sender, RoutedEventArgs e)
        {
            Campaign_Btn_Grid.Visibility = Visibility.Collapsed;
            Adventure_Btn_Grid.Visibility = Visibility.Collapsed;
            NPCs_Btn_Grid.Visibility = Visibility.Collapsed;
            Encounters_Btn_Grid.Visibility = Visibility.Collapsed;

            Back_Btn_Grid.Visibility = Visibility.Visible;
            SearchText_Grid.Visibility = Visibility.Visible;

            var session = _driver.AsyncSession();
            var campaigns = await session.ExecuteReadAsync(
                async tx =>
                {

                    var resultsList = new List<string>();

                    var reader = await tx.RunAsync(
                        "MATCH (c:Campaign) RETURN c.name");
                    
                    while (await reader.FetchAsync())
                    {
                        resultsList.Add(reader.Current[0].ToString());
                    }

                    return resultsList;
                });
            
            foreach (var campaign in campaigns )
            {
                Grid grid = new Grid();
                Button button = new Button();
                st_pnl.Children.Add(grid);
                grid.Children.Add(button);
                grid.Style = (Style) st_pnl.FindResource("sidebar_grids"); 
                button.Style = (Style) st_pnl.FindResource("sidebar_buttons");
                button.Content = campaign;
            }
            
        }
    }
}
