using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Musify.Algorithms;

namespace Musify
{
    public partial class OutPage : PhoneApplicationPage
    {
        public OutPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            NetworkFlowAlgorithm networkFlow = new NetworkFlowAlgorithm();
            outputText.Text = networkFlow.Run();
            KruskalsAlgorithm kruskal = new KruskalsAlgorithm();
            outputText.Text += kruskal.Run();
            UnionFind unionFind = new UnionFind();
            outputText.Text += unionFind.Run();
        }
    }
}