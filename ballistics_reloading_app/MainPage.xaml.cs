namespace ballistics_reloading_app;

public partial class MainPage : ContentPage
{

	public MainPage()
	{
		InitializeComponent();
	}

	private void OnCounterClicked(object sender, EventArgs e)
	{
		Navigation.PushModalAsync(new Dialogs.DialogCreateGun());
	}
}


