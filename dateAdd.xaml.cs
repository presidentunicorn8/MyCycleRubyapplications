using CommunityToolkit.Maui.Views;
using My_Cycle.Data;
using My_Cycle.Models;

namespace My_Cycle;

public partial class dateAdd : ContentPage
{
	public dateAdd()
	{
		InitializeComponent();
	}

    private async Task<int> getLatestId()
    {
        MyDatabase database = await MyDatabase.Instance;
        Item newestItem = await database.GetHighestId();
        return newestItem.Id + 1;
    }

    private async Task<int> getDaysSince(DateTime selectedDate)
    {
        MyDatabase database = await MyDatabase.Instance;
        Item newestItem = await database.GetHighestdate();
        if (newestItem.date > selectedDate)
        {
            return 0;
        }
        return (selectedDate - newestItem.date).Days;
    }

    private async void OnDateSelected(object sender, EventArgs e)
    {
        DateTime selectedDate = MyDatePicker.Date; 
        MyDatabase database = await MyDatabase.Instance;
        int id = 0;
        int daysSince = 0; 
        // if it didn't try, maybe database was empty or connection bad? 
        try
        {
            id = await getLatestId();
            daysSince = await getDaysSince(selectedDate);
        }
        catch
        {
            Console.Write("Tried to query database for id and days since"); 
        }
        Item newItem = new Item(id, selectedDate, daysSince);
        await database.SaveItemAsync(newItem);

    }
}