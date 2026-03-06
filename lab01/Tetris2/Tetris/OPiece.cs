using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tetris
{
    internal class OPiece : Tetromino
    {
        private static readonly int[,] SHAPE = { { 8, 8 }, { 8, 8 } };
        private static readonly int[,] BOOM_SHAPE =
        {
            { 0, -1, -1, -1, -1, 0 },
            {-1, -1, -1, -1, -1, -1 },
            {-1, -1, -1, -1, -1, -1 },
            {-1, -1, -1, -1, -1, -1 },
            {-1, -1, -1, -1, -1, -1 },
            { 0, -1, -1, -1, -1, 0 }
        };

        public OPiece(int x = 0, int y = 21) :
            base(SHAPE, x, y)
        {

        }

        public OPiece(int[,] shape, int x = 0, int y = 21) :
            base(shape, x, y)
        {

        }

        public OPiece(OPiece other) : base(other._shape, other._x, other._y) // конструктор копирования
        {

        }

        public override Tetromino Clone()
        {
            return new OPiece(this);
        }

        public override Tetromino Move(int dx, int dy) // мерцает черным и красным при движении
        {
            int changeTo = 7;
            if (_shape[0, 0] == 7) changeTo = 8;
            int[,] newShape = new int[Size, Size];
            for (int i = 0; i < Size; i++)
                for (int j = 0; j < Size; j++)
                    newShape[i, j] = changeTo;

            return new OPiece(newShape, _x + dx, _y + dy);
        }

        public override Tetromino Rotate() // ничего не меняется при повороте т.к. это квадрат
        {
            return this;
        }

        public override Tetromino SetCoords(int x, int y)
        {
            return new OPiece(_shape, x, y);
        }

        public Tetromino Explosion()
        {
            int centerX = _x + Size / 2;
            int centerY = _y + Size / 2;
            int boomX = centerX - 3;
            int boomY = centerY - 3;

            return new OPiece(BOOM_SHAPE, boomX, boomY);
        }
    }
}
