using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace ChessZone
{
    /// <summary>
    /// Extension pour les contrôles dérivant du type Panel
    /// </summary>
    internal static class PanelExtension
    {
        /// <summary>
        ///     En fonction de la taille de la fenêtre, change la taille du Decorator de manière à ce qu'elle
        /// est la même longueur et largeur.
        /// </summary>
        /// <param name="window"></param>
        /// <param name="panel"></param>
        /// <param name="margin"></param>
        internal static void IdenticalSize(Window window, Decorator panel, double margin = 40)
        {
            // Le panel doit s'adapter à la nouvelle taille de la fenêtre
            window.SizeChanged += (sender, e) =>
            {
                if(window.ActualWidth >= window.ActualHeight)
                {
                    panel.Width = window.ActualHeight - margin * 2;
                    panel.Height = window.ActualHeight - margin * 2;
                }
                else
                {
                    panel.Width = window.ActualWidth - margin * 2;
                    panel.Height = window.ActualWidth - margin * 2;
                }
            };
        }
    }
}
