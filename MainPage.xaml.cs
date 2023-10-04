using CommunityToolkit.Maui.Views;
using My_Cycle.Data;
using My_Cycle.Models; 

namespace My_Cycle
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private List<string> imageList = new List<string>{"alarm.jpg","fly.png","calendar.jpg","cat.png","evil.png","mm.png","moon.jpg","pom.svg","shark.png","reaper.jpg","wine.svg", "click.svg"};

        private async void ChangeImage(object sender, EventArgs e)
        {
            int currentIndex = 0;
            try
            {
                currentIndex = Int32.Parse(await SecureStorage.Default.GetAsync("imageIndex"));
            }
            catch
            {
                Console.WriteLine("Keep current image at zero rather than getting local storage"); 
            }
             // Get the next image from the list
            currentIndex++;
    
            // If we've reached the end of the list, wrap around to the beginning
            if (currentIndex >= imageList.Count)
            {
                currentIndex = 0;
            }

            // Set the image source to the current image
            pom.Source = imageList[currentIndex];
            await SecureStorage.Default.SetAsync("imageIndex", currentIndex.ToString());
        }

        private async Task updateMainPageLabels()
        {
            try
            {
                int currentIndex = Int32.Parse(await SecureStorage.Default.GetAsync("imageIndex"));
                pom.Source = imageList[currentIndex];
            }
            catch { Console.Write("Don't have saved image preference in local storage"); }
            MyDatabase database = await MyDatabase.Instance;
            try
            {
                double averageUnrounded = await database.getThreeMonthAverage();
                Item highestItem = await database.GetHighestdate();
                DateTime highestDate = highestItem.date;

                int roundedAverage = (int)Math.Floor(averageUnrounded);

                DateTime predictedNextDate = highestDate.AddDays(roundedAverage);
                int daysLeft = (predictedNextDate - DateTime.Today).Days;
                if (daysLeft < 0)
                {
                    daysLeft = roundedAverage;
                    predictedNextDate = DateTime.Now;
                }

                daysLabel.Text = $"{daysLeft}";
                averageLabel.Text = $"Based on your average {roundedAverage} day cycle";
                predictedDateLabel.Text = predictedNextDate.ToString("MMM. d");
            }
            catch
            {
                Console.WriteLine("database probably empty, so not updating labels");
            }
            return;
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await updateMainPageLabels(); 
            
        }
        private async void onHistoryClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new HistoryPage());
        }

        
        private void OnAddClicked(object sender, EventArgs e)
        {

            //await Navigation.PushAsync(new dateAdd());
        }

        private async Task<int> getLatestId()
        {
            MyDatabase database = await MyDatabase.Instance;
            Item newestItem = await database.GetHighestId();
            return newestItem.Id + 1;
        }

        private async void OnDateSelected(object sender, EventArgs e)
        {
            DateTime selectedDate = MyDatePicker.Date;
            MyDatabase database = await MyDatabase.Instance;
            int id = 0;
            int daysSince = 28;
            // if it didn't try, maybe database was empty or connection bad? 
            try
            {
                id = await getLatestId();

            }
            catch
            {
                Console.Write("Tried to query database for id and days since");
            }
            Item newItem = new Item(id, selectedDate, daysSince);
            await database.SaveItemAsync(newItem);
            await database.fixDaysSinceLast();
            await updateMainPageLabels();
            await DisplayAlert("Saved", $"Date {selectedDate.ToShortDateString()} added to database.", "OK");

        }
    }
}