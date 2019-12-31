using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using Xamarin.Forms;
using LumenWorks.Framework.IO.Csv;
using System.Data;
using System.Diagnostics;

namespace Nitrogen_App
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {

        DataTable ValueTable = new DataTable();

        public MainPage()
        {
            InitializeComponent();
            ReadCsv();  // Load initial table data.
        }        

        void CalcButtonClicked(object sender, EventArgs e) 
        {
            // When Calculation button is clicked, call the calculator.
            //SCFM_Calc(Double.Parse(Temperature.Text), Double.Parse(Gauge_Pressure.Text));
            try
            {
                Table_Call(Double.Parse(Temperature.Text), Double.Parse(Gauge_Pressure.Text));
            }
            catch
            {
                if(Double.Parse(Temperature.Text) > 400)
                {
                    Answer.Text = "Error : T > 400°F.";
                }
                else if (Double.Parse(Gauge_Pressure.Text) > 15000)
                {
                    Answer.Text = "Error : P > 15,000 psi.";
                }                
                else
                {
                    Answer.Text = "Invalid input data.";
                }                
            }
        }

        void ClearButtonClicked(object sender, EventArgs e) 
        {
            // Clear editor boxes.
            Temperature.Text = "";
            Gauge_Pressure.Text = "";
            Answer.Text = "";
        }        

        void ReadCsv()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Nitrogen_App.ValueTable.csv";

            // open the file "data.csv" which is a CSV file with headers
            using (var csv = new CachedCsvReader(new StreamReader(assembly.GetManifestResourceStream(resourceName)), false))
            {                
                ValueTable.Load(csv);
               
                //Debug testing only.
                foreach(DataRow row in ValueTable.Rows)
                {
                    Console.Write("Row : " + ValueTable.Rows.IndexOf(row) + " ");
                    foreach(var item in row.ItemArray)
                    {
                        Console.Write(item.ToString() + " ");
                    }
                    Console.Write("Next \n");
                }
            }

            
        }
        
        void Table_Call(double a_temp, double a_psig)
        {
            //Round temperature to nearest 10, and pressure to nearest 100.
            a_temp = (int)Math.Round(a_temp / 10) * 10;
            a_psig = (int)Math.Round(( a_psig + 14.6959) / 100) * 100;
            // Table is in PSIA so add 1 ATM (14.6959 PSI) to all gauge pressure readings (PSIG).

            if (a_psig < 50)
            {
                //Progam rouunds down below 50 PSI and a gas with 0 PSI occupies 0 volume,  so we discard that answer.
                Answer.Text = "Error : Pressures of 35 PSI and below are not supported.";
                return;
            }
            else
            {
                int col;
                int row;
                int s_bbl;

                col = (int)a_temp / 10;
                Debug.Print("Column " + col);
                row = (int)a_psig / 100;
                Debug.Print("Row " + row);

                // Answer is in scf / bbl

                s_bbl = int.Parse(ValueTable.Rows[row][col].ToString());

                String result = String.Format("Estimate {0:F1} SCF / BBL", s_bbl);

                Answer.Text = result.ToString();
            }
        }

        void SCFM_Calc(double a_temp, double a_psig) 
        {
            // This one isn't used as I don't have good numbers for compressibility.
            
            // Calculation based on gas law
            // V2 = V1 * (P1 / P2) * (T2 / T1) * (Z2 / Z1)
            //      T: Temperature in Rankine  ( Ideal gas law requires absolute scale )
            //      P: Pressure in PSIG  (Gauge Pressure,  with 0 = 1 ATM)
            //      Z: Compressibility factor.   1 for our purposes.

            // We can more accurately describe what we're measuring here in terms of 
            // actual and standard volumes and temperatures, so let the 2 subscript represent
            // standard and the 1 subscript represent actual.

            // N2 Standard Definitions
            // 1 SCF of N2 measured at 70°F, 14.6959 PSIA weighs 0.07245 lbs

            double s_temp = 70;
            double s_psia = 14.69;
            double rankine_zero_F = 459.67;
            double ft3_per_bbl = 5.614583334;


            double s_volume;
            double s_bbl;
            double a_volume = 1;  // Converting to 1 ACF.
            double pressure_factor = (a_psig + s_psia) / s_psia;
            double temp_factor = (rankine_zero_F + s_temp) / (rankine_zero_F + a_temp);
            double compress_factor = 1;   // May implement this later.

            
            // Answer is in scf / ft3
            s_volume = a_volume * pressure_factor * temp_factor * compress_factor;

            s_bbl = s_volume * ft3_per_bbl;

            String result = String.Format("Estimate {0:F1} SCF / FT3  or  {1:F1} SCF / BBL", s_volume, s_bbl);

            Answer.Text = result.ToString();
        }
    }
}
