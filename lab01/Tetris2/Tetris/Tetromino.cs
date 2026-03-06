using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tetris
{
    internal class Tetromino
    {
        // при вращении и перемещении создаются новые фигуры
        protected readonly int[,] _shape; // всегда квадрат
        protected readonly int _x;
        protected readonly int _y;
        public int X => _x;
        public int Y => _y;
        public int this[int x, int y] => _shape[x, y];
        public virtual int Size => _shape.GetLength(0);

        public Tetromino() {
            _x = 0;
            _y = 0;
            _shape = new int[1,1] { { 8 } };
        }

        public Tetromino(Tetromino other) // конструктор копирования
        {
            _x = other.X;
            _y = other.Y;
            _shape = (int[,])other._shape.Clone(); // массив содержит сами значения а не ссылки
        }

        public Tetromino(int[,] shape, int x = 0, int y = 21) { 
            _x = x;
            _y = y;
            _shape = shape;
        }

        public virtual Tetromino Clone()
        {
            return new Tetromino(_shape, _x, _y);
        }

        public virtual Tetromino Move(int dx, int dy)
        {
            return new Tetromino(_shape, _x + dx, _y + dy);
        }

        public virtual Tetromino Rotate()
        {
            int[,] rotated = new int[Size, Size];
            for (int i = 0; i < Size; i++)
                for (int j = 0; j < Size; j++)
                    rotated[j, Size - 1 - i] = _shape[i,j];
            return new Tetromino(rotated, _x, _y);
        }

        public virtual Tetromino SetCoords(int x, int y)
        {
            return new Tetromino(_shape, x, y);
        }
    }

    internal class IPiece: Tetromino
    {
        private static readonly int[,] SHAPE = 
            {
            { 0, 0, 1, 0 },
            { 0, 0, 1, 0 },
            { 0, 0, 1, 0 },
            { 0, 0, 1, 0 }
            };
        public IPiece(int x = 0, int y = 21) : base(SHAPE, x, y) {}
        public IPiece(int[,] shape, int x = 0, int y = 21) : base(shape, x, y) {}
        public IPiece(IPiece other) : base(other._shape, other._x, other._y) {}
        public override Tetromino Clone()
        {
            return new IPiece(this);
        }
    }

    internal class TPiece : Tetromino
    {
        private static readonly int[,] SHAPE =
            {
            { 0, 3, 0 },
            { 3, 3, 3 },
            { 0, 0, 0 }
        };
        public TPiece(int x = 0, int y = 21) : base(SHAPE, x, y) { }
        public TPiece(int[,] shape, int x = 0, int y = 21) : base(shape, x, y) { }
        public TPiece(TPiece other) : base(other._shape, other._x, other._y) { }
        public override Tetromino Clone()
        {
            return new TPiece(this);
        }
    }

    internal class JPiece : Tetromino
    {
        private static readonly int[,] SHAPE =
            {
            { 4, 0, 0 },
            { 4, 4, 4 },
            { 0, 0, 0 }
        };
        public JPiece(int x = 0, int y = 21) : base(SHAPE, x, y) { }
        public JPiece(int[,] shape, int x = 0, int y = 21) : base(shape, x, y) { }
        public JPiece(JPiece other) : base(other._shape, other._x, other._y) { }
        public override Tetromino Clone()
        {
            return new JPiece(this);
        }
    }

    internal class LPiece : Tetromino
    {
        private static readonly int[,] SHAPE =
            {
            { 0, 0, 5 },
            { 5, 5, 5 },
            { 0, 0, 0 }
        };
        public LPiece(int x = 0, int y = 21) : base(SHAPE, x, y) { }
        public LPiece(int[,] shape, int x = 0, int y = 21) : base(shape, x, y) { }
        public LPiece(LPiece other) : base(other._shape, other._x, other._y) { }
        public override Tetromino Clone()
        {
            return new LPiece(this);
        }
    }

    internal class SPiece : Tetromino
    {
        private static readonly int[,] SHAPE =
            {
            { 0, 6, 6 },
            { 6, 6, 0 },
            { 0, 0, 0 }
        };
        public SPiece(int x = 0, int y = 21) : base(SHAPE, x, y) { }
        public SPiece(int[,] shape, int x = 0, int y = 21) : base(shape, x, y) { }
        public SPiece(SPiece other) : base(other._shape, other._x, other._y) { }
        public override Tetromino Clone()
        {
            return new SPiece(this);
        }
    }

    internal class ZPiece : Tetromino
    {
        private static readonly int[,] SHAPE =
            {
            { 7, 7, 0 },
            { 0, 7, 7 },
            { 0, 0, 0 }
        };
        public ZPiece(int x = 0, int y = 21) : base(SHAPE, x, y) { }
        public ZPiece(int[,] shape, int x = 0, int y = 21) : base(shape, x, y) { }
        public ZPiece(ZPiece other) : base(other._shape, other._x, other._y) { }
        public override Tetromino Clone()
        {
            return new ZPiece(this);
        }
    }
}
