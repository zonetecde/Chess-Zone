using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Timers;
using System.Windows.Threading;
using System.Xml.Linq;

namespace ChessZone
{
    public class AnimationExtension : ObservableObject
    {
        private string searchForPlayerAnimatedText = "Recherche d'un joueur...";

        public string SearchForPlayerAnimatedText 
        {
            get => searchForPlayerAnimatedText;
            set => SetProperty(ref searchForPlayerAnimatedText, value);
        } 

        private DispatcherTimer T_Animation { get; set; }

        /// <summary>
        /// Ajoute toutes les animations
        /// </summary>
        public AnimationExtension()
        {
            T_Animation = new DispatcherTimer();
            T_Animation.Interval = new TimeSpan(0,0,0,0,500);

            int tickCount = 2;
            T_Animation.Tick += (sender, e) =>
            {
                // Animation SearchForPlayerAnimatedText :
                //      Les 3 petits points à la fin apparaissent et disparaissent. 
                //      Affiché dans Label_SearchPlayer.Content dans MainWindow
                tickCount++;
                switch (tickCount)
                {
                    case 3:
                        tickCount = 0;
                        SearchForPlayerAnimatedText = "Recherche d'un joueur.  ";
                        break;
                    case 2:
                        SearchForPlayerAnimatedText = "Recherche d'un joueur...";
                        break;
                    case 1:
                        SearchForPlayerAnimatedText = "Recherche d'un joueur.. ";
                        break;
                }
            };

            T_Animation.Start();
        }
    }
}
