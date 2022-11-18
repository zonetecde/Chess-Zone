namespace ChessZone
{
    internal class GameCell
    {
        internal Piece Piece;
        internal int X;
        internal int Y;

        internal bool? IsMyPiece; // null = void, true = white, false = black

        // Si Piece == Piece.MOVING_PROPOSITION ces 2 variables auront la valeur de 
        // la pièce à bouger 
        internal int ToMoveX;
        internal int ToMoveY;

        public GameCell(Piece piece, int x, int y, bool? isMyPiece)
        {
            Piece = piece;
            X = x;
            Y = y;
            IsMyPiece = isMyPiece;
        }
    }
}