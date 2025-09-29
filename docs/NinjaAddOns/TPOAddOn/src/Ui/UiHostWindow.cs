using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript.AddOns.Core;

namespace NinjaTrader.NinjaScript.AddOns.Ui
{
    public class UiHostWindow : NTWindow
    {
        private TextBlock lastUpdated, bias, dayType, confidence, morph;
        private Border flag;
        private TextBox commentary, evidence;
        private Canvas dpoc;
        private IList<DpocPoint> _trail;

        public UiHostWindow()
        {
            this.Caption = "TPO v9.4.4 — Session Monitor";
            this.Width = 950;
            this.Height = 650;
            this.ResizeMode = ResizeMode.CanResize;

            // Professional theme defaults
            this.Background = new SolidColorBrush(Color.FromRgb(30, 30, 30));     // #1E1E1E
            this.Foreground = new SolidColorBrush(Color.FromRgb(224, 224, 224));  // #E0E0E0
            this.FontFamily = new FontFamily("Segoe UI");
            this.FontSize = 13;

            var grid = new Grid { Margin = new Thickness(10) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
            this.Content = grid;

            // Header
            var header = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10) };
            lastUpdated = new TextBlock { Text = "Last Updated (ET): --:--:--", FontWeight = FontWeights.Bold, Foreground = Brushes.LightGray, Margin = new Thickness(0, 0, 20, 0) };
            flag = new Border { Width = 16, Height = 16, Background = Brushes.Gray, Margin = new Thickness(0, 0, 8, 0) };
            bias = new TextBlock { Text = "Bias: neutral", Margin = new Thickness(0, 0, 20, 0), Foreground = Brushes.LightGray };
            dayType = new TextBlock { Text = "Day: -", Margin = new Thickness(0, 0, 20, 0), Foreground = Brushes.LightGray };
            confidence = new TextBlock { Text = "Conf: -", Margin = new Thickness(0, 0, 20, 0), Foreground = Brushes.LightGray };
            morph = new TextBlock { Text = "Morph: low", Foreground = new SolidColorBrush(Color.FromRgb(255, 152, 0)), FontWeight = FontWeights.Bold };

            header.Children.Add(lastUpdated);
            header.Children.Add(flag);
            header.Children.Add(bias);
            header.Children.Add(dayType);
            header.Children.Add(confidence);
            header.Children.Add(morph);
            Grid.SetColumnSpan(header, 2);
            grid.Children.Add(header);

            // Commentary (left)
            commentary = new TextBox
            {
                AcceptsReturn = true,
                IsReadOnly = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Background = new SolidColorBrush(Color.FromRgb(45, 45, 48)),  // Dark panel
                Foreground = Brushes.White,
                BorderBrush = Brushes.Gray,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 12
            };
            Grid.SetRow(commentary, 1);
            grid.Children.Add(commentary);

            // Right side
            var right = new Grid { Margin = new Thickness(10, 0, 0, 0) };
            right.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Star) });
            right.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            dpoc = new Canvas { Background = Brushes.Black, Margin = new Thickness(0, 0, 0, 6) };
            dpoc.SizeChanged += (s, e) => RedrawTrail();

            evidence = new TextBox
            {
                AcceptsReturn = true,
                IsReadOnly = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Background = new SolidColorBrush(Color.FromRgb(45, 45, 48)),
                Foreground = Brushes.White,
                BorderBrush = Brushes.Gray,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 12
            };

            Grid.SetRow(dpoc, 0);
            Grid.SetRow(evidence, 1);
            right.Children.Add(dpoc);
            right.Children.Add(evidence);

            Grid.SetColumn(right, 1);
            Grid.SetRow(right, 1);
            grid.Children.Add(right);
        }

        private Brush FlagBrush(string biasStr)
        {
            if (string.Equals(biasStr, "bullish", StringComparison.OrdinalIgnoreCase)) return Brushes.LimeGreen;
            if (string.Equals(biasStr, "bearish", StringComparison.OrdinalIgnoreCase)) return Brushes.Red;
            if (string.Equals(biasStr, "neutral", StringComparison.OrdinalIgnoreCase)) return Brushes.Goldenrod;
            return Brushes.Gray;
        }

        public void UpdateUi(DateTime et, string sym, string biasStr, string dayTypeStr, int conf, string morphRisk,
            IEnumerable<string> comments, IEnumerable<string> ev, IList<DpocPoint> trail)
        {
            this.Dispatcher.InvokeAsync(() =>
            {
                lastUpdated.Text = "Last Updated (ET): " + et.ToString("HH:mm:ss") + " — " + sym;
                flag.Background = FlagBrush(biasStr);
                bias.Text = "Bias: " + biasStr;
                dayType.Text = "Day: " + dayTypeStr;
                confidence.Text = "Conf: " + conf;
                morph.Text = "Morph: " + morphRisk;

                if (comments != null)
                {
                    foreach (var c in comments) commentary.AppendText("• " + c + Environment.NewLine);
                    commentary.ScrollToEnd();
                }

                evidence.Clear();
                if (ev != null)
                {
                    foreach (var x in ev) evidence.AppendText("• " + x + Environment.NewLine);
                }

                _trail = trail;
                RedrawTrail();
            });
        }

        private void RedrawTrail()
        {
            dpoc.Children.Clear();
            if (_trail == null || _trail.Count < 2) return;

            double min = double.MaxValue, max = double.MinValue;
            foreach (var pt in _trail) { min = Math.Min(min, pt.Price); max = Math.Max(max, pt.Price); }
            if (Math.Abs(max - min) < 1e-9) { max += 1; min -= 1; }

            double w = dpoc.ActualWidth > 0 ? dpoc.ActualWidth : 400;
            double h = dpoc.ActualHeight > 0 ? dpoc.ActualHeight : 200;
            var poly = new Polyline { Stroke = Brushes.DeepSkyBlue, StrokeThickness = 2.0 };

            for (int i = 0; i < _trail.Count; i++)
            {
                double x = (i / Math.Max(1.0, (double)_trail.Count - 1)) * (w - 10) + 5;
                double y = h - 5 - ((_trail[i].Price - min) / (max - min)) * (h - 10);
                poly.Points.Add(new System.Windows.Point(x, y));
            }
            dpoc.Children.Add(poly);
        }
    }
}
