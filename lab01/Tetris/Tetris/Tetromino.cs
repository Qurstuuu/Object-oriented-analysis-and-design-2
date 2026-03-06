using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Tetris
{
    internal class Tetromino
    {
        // при вращении и перемещении создаются новые фигуры
        private readonly int[,] _shape; // всегда квадрат
        private readonly int _x;
        private readonly int _y;
        public int X => _x;
        public int Y => _y;
        public int this[int x, int y] => _shape[x, y];
        public int Size => _shape.GetLength(0);

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

        public Tetromino(int[,] shape, int x = -100, int y = -100) { 
            if(x == -100)
            {
                x = 0;
            }
            if (y == -100)
            {
                y = 21;
            }
            _x = x;
            _y = y;
            _shape = shape;
        }

        public Tetromino Move(int dx, int dy)
        {
            return new Tetromino(_shape, _x + dx, _y + dy);
        }

        public Tetromino Rotate()
        {
            int[,] rotated = new int[Size, Size];
            for (int i = 0; i < Size; i++)
                for (int j = 0; j < Size; j++)
                    rotated[j, Size - 1 - i] = _shape[i,j];
            return new Tetromino(rotated, _x, _y);
        }

        public Tetromino SetCoords(int x, int y)
        {
            return new Tetromino(_shape, x, y);
        }
    }
}
