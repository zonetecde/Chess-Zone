using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ClassLibrary;
using Newtonsoft.Json;
using zck_client;
using static ClassLibrary.ClassLibrary;

namespace ChessZone
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Couleur
        private readonly Brush EveryOtherBoxColor;

        // GameBoard
        private const int GAME_BOARD_SIZE = 8; // Taille du gameBoard
        private Border[,] GameBoard;           // Données de la partie en cours dans .Tag

        // Server
        private ZoneckClient ZoneckClient;     // Serveur

        // Serveur - Adversaire
        private string IdAdversaire;

        // Déroulement
        private bool IsSearchingPlayer;         // Est-ce que l'on est actuellement en train de rechercher un adversaire ?

        public MainWindow()
        {
            InitializeComponent();

            // Extension
            PanelExtension.IdenticalSize(this, Border_GameBoard); // Gère la taille du gameBoard pour qu'elle s'adapte à celle de la fenêtre

            // Couleur
            BrushConverter brushConverter = new BrushConverter();                       // Permet de convertir un code couleur Hex en Media.Brush
            EveryOtherBoxColor = (Brush)brushConverter.ConvertFromString("#F5E6BF")!;   // Couleur d'une case sur 2 sur le plateau

            // GameBoard
            GameBoardInit(out GameBoard);
        }

        /// <summary>
        /// Affiche le gameBoard dans la uniformGrid_gameBoard
        /// </summary>
        private void ShowGameBoard()
        {
            for (int x = 0; x < GAME_BOARD_SIZE; x++)
            {
                for (int y = 0; y < GAME_BOARD_SIZE; y++)
                {
                    
                }
            }
        }

        /// <summary>
        /// Génère le gameBoard
        /// </summary>
        private void GameBoardInit(out Border[,] gameBoard)
        {
            int cellCounter = 0;                                            // Permet de savoir si c'est une case paire ou impaire
            gameBoard = new Border[GAME_BOARD_SIZE, GAME_BOARD_SIZE];     // Init le gameBoard

            for (int y = 0; y < GAME_BOARD_SIZE; y++)
            { 
                for (int x = 0; x < GAME_BOARD_SIZE; x++)
                {
                    gameBoard[x, y] = new Border()        // Ajoute la case au plateau
                    {
                        Background = cellCounter % 2 == 0 ? EveryOtherBoxColor : Brushes.Transparent,  // Couleur de la case
                        BorderThickness = new Thickness(2),
                        BorderBrush = Brushes.Black,
                        Child = new Image() { Stretch = Stretch.UniformToFill, Margin = new Thickness(2.5,2.5,2.5,2.5) }, // Image où s'affichera les pièces
                        Tag = new GameCell(Piece.VOID)   // Ajoute une pièce
                    };
                    
                                 
                    uniformGrid_gameBoard.Children.Add(gameBoard[x, y]);

                    cellCounter++;
                }

                cellCounter++;
            }
        }

        /// <summary>
        /// Fenêtre initialisée
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Window_ContentRendered(object sender, EventArgs e)
        {
            // Server
            try
            {
                ZoneckClient = new ZoneckClient("Chess", "127.0.0.1", 30_000, MessageRecieved);
                while (ZoneckClient.MyId == null)
                {
                    await Task.Delay(250);
                    //Label_SearchPlayer.Content = ZoneckClient.MyId;
                }

                // Recherche un adversaire
                // Délai sinon des fois il ne trouve pas d'adversaire
                await Task.Delay(1050);
                ZoneckClient.Send("adversaire_trouve_first");
            }
            catch
            {
                MessageBox.Show("Aucun serveur trouvé.");
            }

            // Animation
            this.DataContext = new AnimationExtension();

            // Essaie de trouver un joueur
            //    Le reste se passe dans MessageRecieved
            IsSearchingPlayer = true;
        }

        /// <summary>
        /// Message reçu du serveur
        /// </summary>
        /// <param name="obj"></param>
        private void MessageRecieved(Message msg)
        {
            if(msg.MessageType == MESSAGE_TYPE.MESSAGE)
            {
                // Les autres messages sont dans le msg.Content
                //     Ces messages indiquent des mouvements in-game, des informations
                //     sur si un adversaire a été trouvé etc.
                if (!String.IsNullOrEmpty(msg.Content))
                {
                    // Si l'on a une demande pour être un adversaire  
                    if (msg.Content == "adversaire_trouve_first" || msg.Content == "adversaire_trouve")
                    {
                        // Et que l'on cherche une game
                        if (IsSearchingPlayer)
                        {
                            // Adversaire trouvé, on n'en cherche plus un.
                            IsSearchingPlayer = false;

                            // Informe à ce client qu'un adversaire a été trouvé. 
                            IdAdversaire = msg.Id;

                            // Enlève l'écran de recherche de joueur
                            Dispatcher.Invoke(() =>
                            {
                                Label_SearchPlayer.Visibility = Visibility.Hidden;

                                // Lance partie
                                StartGame();
                            });

                            // Si c'est pas nous qui avons fait la demande nous devons renvoyer la confirmation
                            // au client de l'adversaire qu'il en a trouvé un
                            if (msg.Content == "adversaire_trouve_first")
                            {
                                ZoneckClient.Send("adversaire_trouve", IdAdversaire);
                                return; // On return directement car le client va immédiatement renvoyer un message, il faut être capable de le recevoir
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            ChessMessage cM = FromJsonMessage(msg.Content);
                            switch (cM.MessageType)
                            {

                            }
                        }
                        catch { }
                    }
                }
            }
        }

        /// <summary>
        /// Commence la partie
        /// </summary>
        private void StartGame()
        {
            // Place les pions, les deux clients ont les pions blancs

            PlacePiecesFromColorAtRow('w', 7, 6);
            PlacePiecesFromColorAtRow('b', 0, 1);

        }

        /// <summary>
        /// Place les pièces de la couleur
        /// </summary>
        /// <param name="color">b or w</param>
        /// <param name="line_pawn">La ligne où tous les petits pions sont</param>
        /// <param name="line_other">La ligne où toutes les autres pièces sont</param>
        private void PlacePiecesFromColorAtRow(char color, byte line_other, byte line_pawn)
        {
            // Place tous les pawns
            for (int i = 0; i < 8; i++)
            {
                (GameBoard[i, line_pawn].Child as Image)!.Source = new BitmapImage(new Uri("https://github.com/zonetecde/Chess-Zone/assets/pieces/" + color + "p.png"));
                ((GameCell)GameBoard[i, line_pawn].Tag).Piece = Piece.PAWN;
            }

            // Place les tours 'r'
            (GameBoard[0, line_other].Child as Image)!.Source = new BitmapImage(new Uri("https://github.com/zonetecde/Chess-Zone/assets/pieces/" + color + "r.png"));
            (GameBoard[7, line_other].Child as Image)!.Source = new BitmapImage(new Uri("https://github.com/zonetecde/Chess-Zone/assets/pieces/" + color + "r.png"));
            ((GameCell)GameBoard[0, line_other].Tag).Piece = Piece.ROOK;
            ((GameCell)GameBoard[7, line_other].Tag).Piece = Piece.ROOK;

            // Cavalier 'n'
            (GameBoard[1, line_other].Child as Image)!.Source = new BitmapImage(new Uri("https://github.com/zonetecde/Chess-Zone/assets/pieces/" + color + "n.png"));
            (GameBoard[6, line_other].Child as Image)!.Source = new BitmapImage(new Uri("https://github.com/zonetecde/Chess-Zone/assets/pieces/" + color + "n.png"));
            ((GameCell)GameBoard[1, line_other].Tag).Piece = Piece.KNIGHT;
            ((GameCell)GameBoard[6, line_other].Tag).Piece = Piece.KNIGHT;

            // Fou 'b'
            (GameBoard[2, line_other].Child as Image)!.Source = new BitmapImage(new Uri("https://github.com/zonetecde/Chess-Zone/assets/pieces/" + color + "b.png"));
            (GameBoard[5, line_other].Child as Image)!.Source = new BitmapImage(new Uri("https://github.com/zonetecde/Chess-Zone/assets/pieces/" + color + "b.png"));
            ((GameCell)GameBoard[2, line_other].Tag).Piece = Piece.BISHOP;
            ((GameCell)GameBoard[5, line_other].Tag).Piece = Piece.BISHOP;

            // Dame
            (GameBoard[3, line_other].Child as Image)!.Source = new BitmapImage(new Uri("https://github.com/zonetecde/Chess-Zone/assets/pieces/" + color + "q.png"));
            ((GameCell)GameBoard[3, line_other].Tag).Piece = Piece.QUEEN;

            // Roi
            (GameBoard[4, line_other].Child as Image)!.Source = new BitmapImage(new Uri("https://github.com/zonetecde/Chess-Zone/assets/pieces/" + color + "k.png"));
            ((GameCell)GameBoard[4, line_other].Tag).Piece = Piece.KING;
        }

        private string ToJsonMessage(ChessMessage chessMessage)
        {
            return JsonConvert.SerializeObject(chessMessage);
        }
        private ChessMessage FromJsonMessage(string chessMessage)
        {
            return JsonConvert.DeserializeObject<ChessMessage>(chessMessage)!;
        }

        private void Label_SearchPlayer_MouseDown(object sender, MouseButtonEventArgs e)
        {
        }
    }
}
