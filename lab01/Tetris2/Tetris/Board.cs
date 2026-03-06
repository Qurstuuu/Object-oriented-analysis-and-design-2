using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tetris
{
    internal class Board
    {
        public const int HEIGHT = 22;
        public const int WIDTH = 10;
        private int[,] _grid;
        public int this[int y, int x] => _grid[y, x];

        public Board()
        {
            _grid = new int[HEIGHT, WIDTH];
        }

        public bool CanPlace(Tetromino piece)
        {
            for (int row = 0; row < piece.Size; row++)
                for (int col = 0; col < piece.Size; col++)
                {
                    if (piece[row, col] == 0) continue;

                    int boardX = piece.X + col;
                    int boardY = piece.Y + row;

                    if (boardX < 0 || boardX >= WIDTH) return false;
                    if (boardY < 0 || boardY >= HEIGHT) return false;
                    if (_grid[boardY, boardX] != 0) return false;
                }
            return true;
        }

        public void FreezePiece(Tetromino piece)
        {
            for (int row = 0; row < piece.Size; row++)
            {
                for (int col = 0; col < piece.Size; col++)
                {
                    int value = piece[row, col];
                    if (value == 0) continue;

                    int boardX = piece.X + col;
                    int boardY = piece.Y + row;

                    if (boardY >= 0 && boardY < HEIGHT && boardX >= 0 && boardX < WIDTH)
                    {
                        if (value < 0) // стираем
                        {
                            _grid[boardY, boardX] = 0;
                        }
                        else // добавляем
                        {
                            _grid[boardY, boardX] = value;
                        }
                    }
                }
            }
        }

        public bool IsGameOver() // квадрат 4х4 выше на 2 ряда, чем верхний ряд
        {
            for(int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                {
                    if (_grid[18 + j, 3 + i] != 0) return true;
                }
            return false;
        }

        public int ClearFullLines()
        {
            int linesRemoved = 0;
            for (int i = HEIGHT - 1; i >= 0; i--)
            {
                bool isFull = true;
                bool isEmpty = true;
                for (int j = 0; j < WIDTH; j++)
                {
                    if (_grid[i, j] == 0)
                    {
                        isFull = false;
                    }
                    if (_grid[i, j] != 0)
                    {
                        isEmpty = false;
                    }
                }

                if (isFull) // если линия была полная начисляем очки
                {
                    RemoveLine(i);
                    linesRemoved++;
                }
                if (isEmpty) RemoveLine(i); // если пустая, то просто убираем
            }
            return linesRemoved;
        }

        private void RemoveLine(int y)
        {
            for (int i = y; i < HEIGHT - 1; i++)
                for (int j = 0; j < WIDTH; j++)
                {
                    _grid[i, j] = _grid[i + 1, j];
                }

            // очищаем самую верхнюю строку
            for (int j = 0; j < WIDTH; j++)
            {
                _grid[HEIGHT - 1, j] = 0;
            }
        }
    }
}
