using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Nitrogen_Test.Services;
using Nitrogen_Test.Views;

namespace Nitrogen_Test
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            DependencyService.Register<MockDataStore>();
            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
