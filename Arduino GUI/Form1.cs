using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
//this is an example interface. Each component needs its block to manage actions
//like the ones below. When data is received from Arduino it is handled by Write2Form

namespace Arduino_GUI
{
    public partial class Form1 : Form
    {
        private string val; //global string variable declaration
        public delegate void d1(string indata);
        private static int counter; //integer variable declaration

        private Dictionary<char, System.Windows.Forms.DataVisualization.Charting.Chart> chart_names; //chart-relatd dictionaries
        private Dictionary<char, Single[]> chart_values;


        public Form1() //this initializes the serial port and the chart
        {
            InitializeComponent();
            serialPort1.Open();
            //pairs charts with their control characters
            chart_names = new Dictionary<char, Chart>
            {
                {'0', chart1},
            };
            //pairs chart lenghts with their control characters
            chart_values = new Dictionary<char, Single[]>
            {
                {'0', new float[40] },
            };

            foreach (Chart c in chart_names.Values)
            {
                c.Series["Series2"].Points.Clear(); //this is a transparent series that prevents the scale of the y axis from changing
                c.Series["Series2"].Color = Color.Transparent;
                c.Series["Series2"].IsVisibleInLegend = false;
                c.Series["Series2"].Points.AddXY(1, 5);
                c.Update();
            }
        }

        public void WriteToPort(String message)
        {
            serialPort1.Write("<" + message + ">");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Sends whatever string to the arduino. Double click on button to create code
            WriteToPort("ToggleLED");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string t1 = "R" + textBox1.Text; //string creation and concatenation
            WriteToPort(t1);
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            val = $"{trackBar1.Value}"; //converts value to string
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(val))
            {
                val = "R0"; //default value of zero
            }
            serialPort1.Write($"R{val}");
        }

        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            //clicking on lightining bolt in serial port properties
            //this stuff communicates to a specific text box via Write2Form
            string indata = serialPort1.ReadLine();
            d1 writeit = new d1(Write2Form);
            Invoke(writeit, indata);

        }
        public void Write2Form(string indata)
        {
            //data is received in chunks of 8 characters
            string[] data_pieces = { };
            counter = -1;
            for (int i = 0; i < indata.Length / 8; i++) //splits the data in chunks of 8 characters
            {
                char firstchar = indata[i * 8];
                if (firstchar == 'C') //if the first character is 'C' the data is appended to the last data chunk
                {
                    data_pieces[counter] += indata.Substring(i * 8 + 1, 7).Trim();
                }
                else
                {
                    data_pieces = data_pieces.Append(indata.Substring(i * 8, 8).Trim()).ToArray();
                    counter++; //index of last data chunk
                }
                
            }
            Single var1; //floating point numbers
            Single var2;
            Single oldvalue;
            Single newvalue;
            

            for (int j = 0; j < data_pieces.Length; j++) //handles each data chunk
            {
                char firstchar = data_pieces[j][0]; //the first character, used for control
                char secondchar = data_pieces[j][1]; //the second character, used for control
                string data_piece = data_pieces[j].Substring(2).Trim(); //the actual data

                switch (firstchar) //switches data handling mode based on first character
                {
                    case 'S': //strings. Displayed in text boxes as they are
                        switch (secondchar)
                        {
                            case '0': //each case handles a control character, which identifies the textbox
                                textBox2.Text = data_piece; //converts value to string and displays it in Textbox 2
                                break;
                            case '1':
                                textBox4.Text = data_piece;
                                break;
                            case '2':
                                textBox5.Text = data_piece;
                                break;
                            case '3':
                                textBox6.Text = data_piece;
                                break;
                            case '4':
                                textBox7.Text = data_piece;
                                break;
                            case '5':
                                textBox8.Text = data_piece;
                                break;

                        }
                        break;

                    case 'I': //10 bit integers converted to voltages. Displayed in text boxes and progress bars
                        var1 = Convert.ToSingle(data_piece); 
                        var2 = var1 * 5 / 1024; //converts 10 bit integer to voltage
                        switch (secondchar)
                        {
                            case '0': //each case handles a control character
                                textBox3.Text = String.Format("{0:0.00}", var2); //for decimal places
                                progressBar1.Value = Convert.ToInt16(data_piece); //progress bar   
                                break;
                        }       
                        break;

                    case 'G': //chart display receiving one value at a time from Arduino
                        var1 = Convert.ToSingle(data_piece); //substring starting from index 1
                        var2 = var1 * 5 / 1024;
                        newvalue = var2;
                        
                        chart_names[secondchar].Series["Series1"].Points.Clear();
                        int l = chart_values[secondchar].Length;
                        for (int i = l - 1; i >= 0; i--) //bumps all value one place up the array and puts the new one at the end
                        {
                            oldvalue = chart_values[secondchar][i];
                            chart_names[secondchar].Series["Series1"].Points.AddXY(i + 1, newvalue);//adds the point to the chart in x position i+1
                            chart_values[secondchar][i] = newvalue;
                            newvalue = oldvalue;
                        }
                        chart_names[secondchar].Update();
                        break;

                }
            }       
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
