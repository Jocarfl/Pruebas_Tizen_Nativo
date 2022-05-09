using System;
using Xamarin.Forms;
using Tizen.Sensor;
using HRM = Tizen.Sensor.HeartRateMonitor;
using Tizen;
using HeartRateMonitor.Model;
using NetMQ;
using NetMQ.Sockets;

namespace Pruebas_Tizen_Nativo
{

    public class App : Application
    {
        Button botonStart;
        Button botonStop;
        public Label labelHRM;     
        private HeartRateMonitorModel heartRateMonitorModel;
        private HRM hrm;

        public App()
        {
            Permission();
            MainPage = new ContentPage
            {
                Content = new StackLayout
                {
                    VerticalOptions = LayoutOptions.Center,
                    Children = {
                        LabelHRM(),
                        BotonStart(),
                        BotonStop(),
                    }
                }
            };
        }

        private void InitializeHRM()
        {
            hrm = new HRM
            {
                Interval = 1000,
                PausePolicy = SensorPausePolicy.None
            };
            hrm.DataUpdated += OnHRMDataUpdated;
            hrm.Start();
        }

        private void StopHRM()
        {
            hrm.Stop();
        }

        private void OnHRMDataUpdated(object sender, HeartRateMonitorDataUpdatedEventArgs e)
        {
            
            if (e.HeartRate > 0)
            {
                labelHRM.Text ="HRM: "+e.HeartRate.ToString();
                SendZMQ(e.HeartRate);
            }
            else
            {
                labelHRM.Text = "Midiendo...";
            }
        }


        public Label LabelHRM()
        {
            labelHRM = new Label
            {
                Text = "HRM: ",
                HorizontalOptions = LayoutOptions.Center,
            };

            return labelHRM;
        }

        public Button BotonStart()
        {

            botonStart = new Button
            {
                Text = "Start Connection",
                BackgroundColor = Color.Green,
                HorizontalOptions = LayoutOptions.Center,
            };

            botonStart.Clicked += OnButtonStartClicked;

            return botonStart;

        }

        void OnButtonStartClicked(object s, EventArgs e)
        {
            if (!botonStop.IsEnabled)
            {
                botonStart.IsEnabled = false;
                botonStop.IsEnabled = true;
            }

            InitializeHRM();
          
        }

        public Button BotonStop()
        {
            botonStop = new Button
            {
                Text = "Stop Connection",
                BackgroundColor = Color.Red,
                HorizontalOptions = LayoutOptions.Center,
                IsEnabled = false,
            };

            botonStop.Clicked += OnButtonStopClicked;

            return botonStop;
        }

        void OnButtonStopClicked(object s, EventArgs e)
        {

            if (!botonStart.IsEnabled)
            {
                botonStop.IsEnabled = false;
                botonStart.IsEnabled = true;
            }

            StopHRM();
        }

        async void Permission()
        {
            heartRateMonitorModel = new HeartRateMonitorModel();
            if (!await heartRateMonitorModel.CheckPrivileges())
            {
                Log.Error("PERMISION","No Sensor Permision");

                return;
            }
            else
            {
                heartRateMonitorModel.Init();
            }
            
        }

        public static void SendZMQ(int hrm)
        {
            Console.WriteLine("Connecting to hello world server…");
            using (var requester = new RequestSocket())
            {
                requester.Connect("tcp://localhost:5555");
                requester.SendFrame(hrm.ToString());
   
            }
        }

        public static void DisconnectZMQ()
        {
            Console.WriteLine("Disconnecting to hello world server…");
            using (var requester = new RequestSocket())
            {
                requester.Disconnect("tcp://localhost:5555");
            }
        }
        protected override void OnStart()
        {

        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
