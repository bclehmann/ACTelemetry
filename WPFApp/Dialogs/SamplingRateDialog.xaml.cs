using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AssettoCorsaTelemetryApp.Dialogs
{
	/// <summary>
	/// Interaction logic for SamplingRateDialog.xaml
	/// </summary>
	public partial class SamplingRateDialog : Window
	{
		public SamplingRateDialog()
		{
			InitializeComponent();
			UpdateFrequencyLabel();
		}

		private void UpdateFrequencyLabel()
		{
			if (frequencyLabel != null)
			{
				frequencyLabel.Text = (int)SamplingRate.Value + " Hz";
			}
		}

		private void SamplingRate_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			UpdateFrequencyLabel();
		}

		private void OKButton_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
			Close();
		}
	}
}
