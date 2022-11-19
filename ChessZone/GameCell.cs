namespace ChessZone
{
    internal class GameCell
    {
        internal Piece Piece;
        internal Piece EnemyPiece; // La type de la pièce si c'est une pièce ennemi.
                                            // .Piece peut prendre la valeur de MOVEMENT_PROPOSITION, c'est pour
                                            // cela que l'on créé une deuxième variable.
        internal int X;
        internal int Y;

        internal bool? IsMyPiece; // null = void, true = white, false = black

        // Si Piece == Piece.MOVING_PROPOSITION ces 2 variables auront la valeur de 
        // la pièce à bouger 
        internal int ToMoveX;
        internal int ToMoveY;

        public GameCell(Piece piece, int x, int y, bool? isMyPiece, Piece enemyPiece = Piece.VOID)
        {
            Piece = piece;
            X = x;
            Y = y;
            IsMyPiece = isMyPiece;
            EnemyPiece = enemyPiece;
        }
    }
}