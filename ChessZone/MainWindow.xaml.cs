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
        private bool IsMyTurn = false;          // Est-ce que c'est à mon tour de jouer ?

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
                    var piece_image = new Image()
                    {
                        Stretch = Stretch.UniformToFill,
                        Margin = new Thickness(2.5, 2.5, 2.5, 2.5)
                    };

                    piece_image.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(GameCellMouseDown);

                    gameBoard[x, y] = new Border()        // Ajoute la case au plateau
                    {
                        Background = cellCounter % 2 == 0 ? EveryOtherBoxColor : Brushes.Transparent,  // Couleur de la case
                        BorderThickness = new Thickness(2),
                        BorderBrush = Brushes.Black,
                        Child = piece_image, // Image où s'affichera les pièces
                        Tag = new GameCell(Piece.VOID, x, y, (y == 7 || y == 6) ? true : (y == 0 || y == 1) ? false : null)   
                                                                                // Ajoute une pièce. Si elle est dans les 2 dernières rangées c'est une pièce
                                                                                 // blanche, donc la notre. Si c'est une pièce ennemi c'est false. Sinon c'est null.
                    };

                    gameBoard[x, y].MouseLeftButtonUp += new MouseButtonEventHandler(GameCellMouseDown);
                                 
                    uniformGrid_gameBoard.Children.Add(gameBoard[x, y]);

                    cellCounter++;
                }

                cellCounter++;
            }
        }

        /// <summary>
        /// Clique sur une cellule du plateau
        /// </summary>
        /// <param name="sender">Cellule cliqué</param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void GameCellMouseDown(object sender, MouseButtonEventArgs e)
        {
            Border clicked_cell = sender is Image ? (Border)VisualTreeHelper.GetParent(sender as UIElement) : (Border)sender;
            GameCell clicked_gameCell = (GameCell)clicked_cell.Tag;

            // Si c'est notre tour
            if(IsMyTurn)
            {
                // En fonction de la case cliqué 
                switch (clicked_gameCell.Piece)
                {
                    case Piece.PAWN:
                        // La pièce PAWN peut avancer de 2 vers l'avant si son y = 6
                        // Si la pièce PAWN est sur y < 6 alors elle ne peut avancer que d'un rang
                        var possible_movement_pos_pawn = new List<int[]>();
                        possible_movement_pos_pawn.Add(new int[2] { clicked_gameCell.X, clicked_gameCell.Y - 1 });

                        if (clicked_gameCell.Y - 1 >= 0)
                        {
                            if (((GameCell)GameBoard[clicked_gameCell.X, clicked_gameCell.Y - 1].Tag).IsMyPiece == null // Il n'y a pas de pièce devant donc on peu passer la deuxième case au dessus
                                && clicked_gameCell.Y == 6) // On est en première ligne
                                possible_movement_pos_pawn.Add(new int[2] { clicked_gameCell.X, clicked_gameCell.Y - 2 });
                        }

                        ShowPieceMovePreview(clicked_gameCell, possible_movement_pos_pawn);
                        
                        break;
                    case Piece.KNIGHT:
                        // La pièce KNIGHT peut avancer de 2-1 toutes directions confondu
                        ShowPieceMovePreview(clicked_gameCell, new List<int[]>
                        { 
                            new int[2]{ clicked_gameCell.X + 1, clicked_gameCell.Y + 2 },
                            new int[2]{ clicked_gameCell.X - 1, clicked_gameCell.Y + 2 },
                            new int[2]{ clicked_gameCell.X - 1, clicked_gameCell.Y - 2 },
                            new int[2]{ clicked_gameCell.X + 1, clicked_gameCell.Y - 2 },
                            new int[2]{ clicked_gameCell.X + 2, clicked_gameCell.Y - 1 },
                            new int[2]{ clicked_gameCell.X + 2, clicked_gameCell.Y + 1 },
                            new int[2]{ clicked_gameCell.X - 2, clicked_gameCell.Y + 1 },
                            new int[2]{ clicked_gameCell.X - 2, clicked_gameCell.Y - 1 },
                        });                       
                        break;
                    case Piece.ROOK:
                        // La pièce ROOK peut aller dans toutes les directions verticales et horizontales
                        var possible_movement_pos_rook = new List<int[]>();

                        AddHorizontalAndVerticalMovement(clicked_gameCell, possible_movement_pos_rook);

                        ShowPieceMovePreview(clicked_gameCell, possible_movement_pos_rook);
                        break;

                    case Piece.BISHOP:
                        // La pièce BISHOP peut aller dans toutes les directions diagonales
                        var possible_movement_pos_bishop = new List<int[]>();

                        AddDiagonalMovement(clicked_gameCell, possible_movement_pos_bishop);

                        ShowPieceMovePreview(clicked_gameCell, possible_movement_pos_bishop);
                        break;

                    case Piece.QUEEN:
                        // La pièce QUEEN peut aller dans toutes les directions diagonales, verticales et horizontales.
                        var possible_movement_pos_queen = new List<int[]>();

                        AddDiagonalMovement(clicked_gameCell, possible_movement_pos_queen);
                        AddHorizontalAndVerticalMovement(clicked_gameCell, possible_movement_pos_queen);

                        ShowPieceMovePreview(clicked_gameCell, possible_movement_pos_queen);
                        break;

                    case Piece.KING:
                        // La pièce KING peut aller dans toutes ses cases le juxtaposant. 

                        ShowPieceMovePreview(clicked_gameCell, new List<int[]>()
                        {
                            new int[2]{ clicked_gameCell.X - 1, clicked_gameCell.Y - 1 },
                            new int[2]{ clicked_gameCell.X + 1, clicked_gameCell.Y + 1 },
                            new int[2]{ clicked_gameCell.X - 1, clicked_gameCell.Y + 1 },
                            new int[2]{ clicked_gameCell.X + 1, clicked_gameCell.Y - 1 },
                            new int[2]{ clicked_gameCell.X, clicked_gameCell.Y - 1 },
                            new int[2]{ clicked_gameCell.X, clicked_gameCell.Y + 1 },
                            new int[2]{ clicked_gameCell.X + 1, clicked_gameCell.Y },
                            new int[2]{ clicked_gameCell.X - 1, clicked_gameCell.Y },
                        });

                        break;

                    case Piece.MOVING_PROPOSITION:
                        // Change à qui est le tour
                        IsMyTurn = false;
                        MyTurnIndicator();

                        // Envois au serveur la nouvelle position de la pièce 
                        MovementInformation mi = new MovementInformation(
                            new int[2] { clicked_gameCell.ToMoveX, clicked_gameCell.ToMoveY },
                            new int[2] { clicked_gameCell.X, clicked_gameCell.Y }
                        );

                        string message = ToJsonMessage(new ChessMessage(CHESS_MESSAGE_TYPE.MOVEMENT, ZoneckClient.MyId,
                            JsonConvert.SerializeObject(mi)));

                        ZoneckClient.Send(message, IdAdversaire);

                        // Bouge la pièce. On le fait seulement maintenant pour conserver les variables lors de l'envoi des infos à l'adversaire
                        MovePiece(new int[2] { clicked_gameCell.ToMoveX, clicked_gameCell.ToMoveY }, new int[2] { clicked_gameCell.X, clicked_gameCell.Y });

                        // Cache les preview
                        HideAllMovingProposition(null);

                        break;
                }

                // Permet de ne pas déclencher une deuxième fois l'event
                e.Handled = true;
            }
        }

        /// <summary>
        /// Ajoute tous les mouvements horizontaux et verticaux que la pièce peut effectuer par rapport à gameCell
        /// </summary>
        /// <param name="gameCell"></param>
        /// <param name="possible_movement_pos_rook"></param>
        private void AddHorizontalAndVerticalMovement(GameCell gameCell, List<int[]> possible_movement_pos_rook)
        {
            // Gauche
            int temp = gameCell.X;
            while (temp - 1 >= 0)
            {
                temp--;

                possible_movement_pos_rook.Add(new int[2] { temp, gameCell.Y });

                if (((GameCell)GameBoard[temp, gameCell.Y].Tag).IsMyPiece != null) break; // On a rencontré une pièce, on ne peut pas la dépasser
            }

            // Droite
            temp = gameCell.X;
            while (temp + 1 <= 7)
            {
                temp++;

                possible_movement_pos_rook.Add(new int[2] { temp, gameCell.Y });

                if (((GameCell)GameBoard[temp, gameCell.Y].Tag).IsMyPiece != null) break; // On a rencontré une pièce, on ne peut pas la dépasser
            }

            // Haut
            temp = gameCell.Y;
            while (temp - 1 >= 0)
            {
                temp--;

                possible_movement_pos_rook.Add(new int[2] { gameCell.X, temp });

                if (((GameCell)GameBoard[gameCell.X, temp].Tag).IsMyPiece != null) break; // On a rencontré une pièce, on ne peut pas la dépasser
            }

            // Bas
            temp = gameCell.Y;
            while (temp + 1 <= 7)
            {
                temp++;

                possible_movement_pos_rook.Add(new int[2] { gameCell.X, temp });

                if (((GameCell)GameBoard[gameCell.X, temp].Tag).IsMyPiece != null) break; // On a rencontré une pièce, on ne peut pas la dépasser
            }
        }

        /// <summary>
        /// Ajoute toutes les positions diagonaux que la pièce peut faire par apport à gameCell
        /// </summary>
        /// <param name="gameCell"></param>
        /// <param name="possible_movement_pos"></param>
        private void AddDiagonalMovement(GameCell gameCell, List<int[]> possible_movement_pos)
        {
            // Haut Gauche
            int temp_bishop_X = gameCell.X;
            int temp_bishop_Y = gameCell.Y;
            while (temp_bishop_X - 1 >= 0 && temp_bishop_Y - 1 >= 0)
            {
                temp_bishop_X--;
                temp_bishop_Y--;

                possible_movement_pos.Add(new int[2] { temp_bishop_X, temp_bishop_Y });

                if (((GameCell)GameBoard[temp_bishop_X, temp_bishop_Y].Tag).IsMyPiece != null) break; // On a rencontré une pièce, on ne peut pas la dépasser
            }

            // Haut Droite
            temp_bishop_X = gameCell.X;
            temp_bishop_Y = gameCell.Y;
            while (temp_bishop_X + 1 <= 7 && temp_bishop_Y - 1 >= 0)
            {
                temp_bishop_X++;
                temp_bishop_Y--;

                possible_movement_pos.Add(new int[2] { temp_bishop_X, temp_bishop_Y });

                if (((GameCell)GameBoard[temp_bishop_X, temp_bishop_Y].Tag).IsMyPiece != null) break; // On a rencontré une pièce, on ne peut pas la dépasser
            }

            // Bas Gauche
            temp_bishop_X = gameCell.X;
            temp_bishop_Y = gameCell.Y;
            while (temp_bishop_X - 1 >= 0 && temp_bishop_Y + 1 <= 7)
            {
                temp_bishop_X--;
                temp_bishop_Y++;

                possible_movement_pos.Add(new int[2] { temp_bishop_X, temp_bishop_Y });

                if (((GameCell)GameBoard[temp_bishop_X, temp_bishop_Y].Tag).IsMyPiece != null) break; // On a rencontré une pièce, on ne peut pas la dépasser
            }

            // Bas Droite
            temp_bishop_X = gameCell.X;
            temp_bishop_Y = gameCell.Y;
            while (temp_bishop_X + 1 <= 7 && temp_bishop_Y + 1 <= 7)
            {
                temp_bishop_X++;
                temp_bishop_Y++;

                possible_movement_pos.Add(new int[2] { temp_bishop_X, temp_bishop_Y });

                if (((GameCell)GameBoard[temp_bishop_X, temp_bishop_Y].Tag).IsMyPiece != null) break; // On a rencontré une pièce, on ne peut pas la dépasser
            }
        }

        /// <summary>
        /// Déplace la pièce de posFrom à posTo
        /// </summary>
        /// <param name="posFrom">Pos de la pièce à déplacer</param>
        /// <param name="posTo">Pos où déplacer la pièce</param>
        private void MovePiece(int[] posFrom, int[] posTo)
        {
            Border cell_to_move = GameBoard[posFrom[0], posFrom[1]];
            GameCell gameCell_to_move = (GameCell)cell_to_move.Tag;

            Border cell_destination = GameBoard[posTo[0], posTo[1]];
            GameCell gameCell_destination = (GameCell)cell_destination.Tag;


            // Bouge la pièce (pic)                  Image de la pièce à bouger
            ((Image)cell_destination.Child).Source = ((Image)GameBoard[gameCell_to_move.X, gameCell_to_move.Y].Child).Source;
            // Enlève l'image de la pièce qui a été bougé
            ((Image)GameBoard[gameCell_to_move.X, gameCell_to_move.Y].Child).Source = null;

            // Change le curseur de la case déplacé
            GameBoard[gameCell_to_move.X, gameCell_to_move.Y].Cursor = Cursors.Arrow;

            // Type                      Piece à bouger
            gameCell_destination.Piece = ((GameCell)GameBoard[gameCell_to_move.X, gameCell_to_move.Y].Tag).Piece;
            // Personne à qui appartient la pièce
            gameCell_destination.IsMyPiece = ((GameCell)GameBoard[gameCell_to_move.X, gameCell_to_move.Y].Tag).IsMyPiece;

            // Deviens une case vide
            ((GameCell)GameBoard[gameCell_to_move.X, gameCell_to_move.Y].Tag).IsMyPiece = null;
            ((GameCell)GameBoard[gameCell_to_move.X, gameCell_to_move.Y].Tag).Piece = Piece.VOID;
            gameCell_destination.ToMoveX = -1;
            gameCell_destination.ToMoveY = -1;
        }

        /// <summary>
        /// Affiche un point pour pouvoir montrer que l'on peut bouger dans cette case
        /// </summary>
        /// <param name="cell">La cell (permet d'avoir les co de la pièce à bouger)</param>
        /// <param name="pos">Les positions où l'on peut bouger cette pièce</param>
        private void ShowPieceMovePreview(GameCell cell, List<int[]> poss)
        {
            // Enlève les previews de tous les autres endroits avant d'afficher celle ci.
            bool DoShowPieceMovePreview = HideAllMovingProposition(cell);

            if (DoShowPieceMovePreview) // Ceci concerne si on appuie 2x sur une même pièce, la première fois
                                        // on affiche sa trajectoire, la deuxième fois on veut la cacher.
            {
                // La pièce PAWN est spécial :
                // Si l'on a un ennemi en y -1 x1-(-1) alors on peut le tuer, sinon on ne peut pas y aller
                // Par contre elle ne peut tuer l'ennemi se trouvant devant
                if(cell.Piece == Piece.PAWN)
                {
                    if(cell.X - 1 >= 0 && cell.Y - 1 >= 0) // Index out of range prevention
                        if(((GameCell)GameBoard[cell.X - 1, cell.Y - 1].Tag).IsMyPiece == false)                   
                            poss.Add(new int[2] { cell.X - 1, cell.Y - 1 });

                    if (cell.X + 1 <= 7 && cell.Y - 1 >= 0) // Index out of range prevention
                        if (((GameCell)GameBoard[cell.X + 1, cell.Y - 1].Tag).IsMyPiece == false)                  
                            poss.Add(new int[2] { cell.X + 1, cell.Y - 1 });

                    if (cell.Y - 1 >= 0) // Index out of range prevention
                        if (((GameCell)GameBoard[cell.X, cell.Y - 1].Tag).IsMyPiece == false)                    
                            poss.RemoveAll(c => c[0] == cell.X && c[1] == cell.Y - 1);
                    
                    if (cell.Y - 2 >= 0) // Index out of range prevention
                        if (((GameCell)GameBoard[cell.X, cell.Y - 2].Tag).IsMyPiece == false)                    
                            poss.RemoveAll(c => c[0] == cell.X && c[1] == cell.Y - 2);
                    
                }

                foreach (int[] pos in poss)
                {
                    //  Si la pos est :
                    // 1. dans le GameBoard (IndexOutOfRange)
                    if (pos[0] < 0 || pos[0] > 7 || pos[1] < 0 || pos[1] > 7)
                        continue;
                    // 2. Si il n'y a pas déjà une pièce à nous dans cette emplacement
                    if (((GameCell)GameBoard[pos[0], pos[1]].Tag).IsMyPiece == true) // Si c'est ma pièce
                        continue;

                    // Si la pièce peut manger une pièce ennemi on change le fond d'une couleur pour l'indiquer
                    if (((GameCell)GameBoard[pos[0], pos[1]].Tag).IsMyPiece == false)
                    {
                        GameBoard[pos[0], pos[1]].BorderBrush = Brushes.DarkRed;
                        GameBoard[pos[0], pos[1]].BorderThickness = new Thickness(4);
                    }
                    else
                    {
                        // Sinon on affiche simplement un point indiquant que l'on peut bouger ça pièce dans cette emplacement
                        var img_mvoing_proposition = new BitmapImage();
                        img_mvoing_proposition.BeginInit();
                        img_mvoing_proposition.StreamSource = Assembly.GetEntryAssembly()!.GetManifestResourceStream("ChessZone.assets.pieces.moving_proposition.png");
                        img_mvoing_proposition.EndInit();

                        ((Image)GameBoard[pos[0], pos[1]].Child).Source = img_mvoing_proposition;
                    }

                    // Curseur
                    GameBoard[pos[0], pos[1]].Cursor = Cursors.Hand;

                    // Type de la cell
                    ((GameCell)GameBoard[pos[0], pos[1]].Tag).Piece = Piece.MOVING_PROPOSITION;

                    // Indique la pièce à bouger si on sélectionne la proposition
                    ((GameCell)GameBoard[pos[0], pos[1]].Tag).ToMoveX = cell.X;
                    ((GameCell)GameBoard[pos[0], pos[1]].Tag).ToMoveY = cell.Y;
                }
            }
        }

        /// <summary>
        /// Cache les movings proposition
        /// </summary>
        /// <param name="cell">La cellule pour vérifier si les positions d'une même pièce ont été cliqué 2 fois à la suite</param>
        /// <returns>Vrai si la même pièce a été cliqué 2 fois à la suite</returns>
        private bool HideAllMovingProposition(GameCell? cell = null)
        {
            bool DoShowPieceMovePreview = true;

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    GameCell gameCell = (GameCell)GameBoard[x, y].Tag;

                    if (gameCell.Piece == Piece.MOVING_PROPOSITION)
                    {
                        if(cell != null)
                            if (gameCell.ToMoveX == cell.X && gameCell.ToMoveY == cell.Y)
                                DoShowPieceMovePreview = false;

                        gameCell.ToMoveX = -1;
                        gameCell.ToMoveY = -1;
                        gameCell.Piece = Piece.VOID;

                        // Change uniquement si la source est celle d'une proposition - soit sur une celle vide
                        // (pour éviter que ça enlève l'image d'une pièce adversaire après proposition de la prendre
                        if (((GameCell)GameBoard[x, y].Tag).IsMyPiece == null)
                            ((Image)GameBoard[x, y].Child).Source = null;
                        
                        GameBoard[x, y].Cursor = Cursors.Arrow;
                    }

                    // Solution temporaire :
                    // Lorsque l'on prend une pièce ennemi le cercle rouge autour de la pièce reste pour
                    // des raisons inconnues
                    if (GameBoard[x,y].BorderBrush == Brushes.DarkRed)
                    {
                        GameBoard[x, y].BorderBrush = Brushes.Black;
                        GameBoard[x, y].BorderThickness = new Thickness(2);
                    }
                }
            }

            return DoShowPieceMovePreview;
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

                            if (msg.Content == "adversaire_trouve_first")
                                IsMyTurn = true; // Le joueur qui envois l'invitation commence
                            else
                                IsMyTurn = false;

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
                                // Si l'adversaire a joué; il a bougé une pièce
                                case CHESS_MESSAGE_TYPE.MOVEMENT:
                                    MovementInformation mi = JsonConvert.DeserializeObject<MovementInformation>(cM.Message.ToString()!)!;
                                    Dispatcher.Invoke(() =>
                                    {
                                        // Les coordonnées sont encore les pièces des blancs. Il faut donc changer
                                        // pour que ça soit la pièce équivalente noir qui bouche à la place
                                        MovePiece(PiecePosFromBlackPlayerViewConverter(mi.FromPos), PiecePosFromBlackPlayerViewConverter(mi.ToPos));

                                        IsMyTurn = true;
                                        MyTurnIndicator();
                                    });
                                    break;
                            }
                        }
                        catch { }
                    }
                }
            }
        }

        /// <summary>
        /// Prends la position donné et la transforme en son équivalent en pièce noir
        ///     Utiliser dans MessageRecieved() -> l'adversaire a bougé sa pièce
        /// </summary>
        /// <param name="pos">Coordonnées de la pièce blanche</param>
        /// <returns>Coordonnées de son équivalent noir</returns>
        private int[] PiecePosFromBlackPlayerViewConverter(int[] pos)
        {
            // Change uniquement le y ([1]) de cette façon :
            // 7 <-> 0
            // 6 <-> 1
            // 5 <-> 2
            // 4 <-> 3
            pos[1] = 7 - pos[1];
            return pos;
        }

        /// <summary>
        /// Commence la partie
        /// </summary>
        private void StartGame()
        {
            // Place les pions, les deux clients ont les pions blancs
            PlacePiecesFromColorAtRow('w', 7, 6);
            PlacePiecesFromColorAtRow('b', 0, 1);

            // Le joueur qui commence est celui qui a lancé la demande de partie
            MyTurnIndicator();
        }

        /// <summary>
        /// Effet visuelle (couleur de la bordure du plateau) pour savoir si c'est notre
        /// tour de jouer.
        /// </summary>
        private void MyTurnIndicator()
        {
            if (IsMyTurn)
            {
                Border_GameBoard.BorderBrush = Brushes.DarkGreen;
            }
            else
            {
                Border_GameBoard.BorderBrush = Brushes.Red;
            }
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
                if (color == 'w')
                {
                    GameBoard[i, line_pawn].Cursor = Cursors.Hand; // 6 = la ligne avec tous les pawns
                    GameBoard[i, 7].Cursor = Cursors.Hand; // 7 = la dernière ligne avec toutes les autres pièces
                }

                (GameBoard[i, line_pawn].Child as Image)!.Source = GetImageSourceFromResource(color + "p");
                if (color == 'w')
                    ((GameCell)GameBoard[i, line_pawn].Tag).Piece = Piece.PAWN;

                // Void row
                GameBoard[i, 5].Cursor = Cursors.Arrow;
                GameBoard[i, 4].Cursor = Cursors.Arrow;
                GameBoard[i, 3].Cursor = Cursors.Arrow;
                GameBoard[i, 2].Cursor = Cursors.Arrow;
                ((Image)GameBoard[i, 5].Child).Source = null;
                ((Image)GameBoard[i, 4].Child).Source = null;
                ((Image)GameBoard[i, 3].Child).Source = null;
                ((Image)GameBoard[i, 2].Child).Source = null;
            }

            // Place les tours 'r'
            (GameBoard[0, line_other].Child as Image)!.Source = GetImageSourceFromResource(color + "r");
            (GameBoard[7, line_other].Child as Image)!.Source = GetImageSourceFromResource(color + "r");

            // Cavalier "n"
            (GameBoard[1, line_other].Child as Image)!.Source = GetImageSourceFromResource(color + "n");
            (GameBoard[6, line_other].Child as Image)!.Source = GetImageSourceFromResource(color + "n");

            // Fou "b"
            (GameBoard[2, line_other].Child as Image)!.Source = GetImageSourceFromResource(color + "n");
            (GameBoard[5, line_other].Child as Image)!.Source = GetImageSourceFromResource(color + "n");

            // Dame "q"
            (GameBoard[3, line_other].Child as Image)!.Source = GetImageSourceFromResource(color + "q");

            // Roi "k"
            (GameBoard[4, line_other].Child as Image)!.Source = GetImageSourceFromResource(color + "k");

            // Set uniquement nos pièces, les pièces ennemis sont en Piece.VOID
            if (color == 'w')
            {
                ((GameCell)GameBoard[0, line_other].Tag).Piece = Piece.ROOK;
                ((GameCell)GameBoard[7, line_other].Tag).Piece = Piece.ROOK;
                ((GameCell)GameBoard[1, line_other].Tag).Piece = Piece.KNIGHT;
                ((GameCell)GameBoard[6, line_other].Tag).Piece = Piece.KNIGHT;
                ((GameCell)GameBoard[2, line_other].Tag).Piece = Piece.BISHOP;
                ((GameCell)GameBoard[5, line_other].Tag).Piece = Piece.BISHOP;
                ((GameCell)GameBoard[3, line_other].Tag).Piece = Piece.QUEEN;
                ((GameCell)GameBoard[4, line_other].Tag).Piece = Piece.KING;
            }
        }

        private BitmapImage GetImageSourceFromResource(string resourceName)
        {
            var img = new BitmapImage();
            img.BeginInit();
            img.StreamSource = Assembly.GetEntryAssembly()!.GetManifestResourceStream("ChessZone.assets.pieces." + resourceName + ".png");
            img.EndInit();
            return img;
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
