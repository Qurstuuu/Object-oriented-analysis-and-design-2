using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Tetris
{
    internal partial class Form1 : Form
    {
        private int _squareSize;
        private Game _game;
        private Board _board;
        private System.Windows.Forms.Timer _renderTimer;
        private int _padding;

        public Form1(Game game)
        {
            InitializeComponent();
            Init(game);
        }

        public void Init(Game game)
        {
            _squareSize = 25;
            _padding = 100;
            _game = game;
            _game.GameOver += OnGameOver;
            _board = _game.Board;
            _renderTimer = new System.Windows.Forms.Timer();
            _renderTimer.Interval = 33; // 30 FPS
            _renderTimer.Tick += RedrawMap;
            game.StartGame();
            _renderTimer.Enabled = true; // сразу начинаем отрисовку
            this.Focus();
        }

        private void OnGameOver(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnGameOver(sender, e)));
                return;
            }
            labelScore.Text = "Game over! Your score is: " + _game.Score;
            labelHighScore.Text = "Highscore: " + _game.HighScore;
        }

        private void OnKeyDown(object sender, KeyEventArgs e) // обработка нажатий
        {
            switch (e.KeyCode)
            {
                case Keys.Left: _game.MoveLeft(); break;
                case Keys.Right: _game.MoveRight(); break;
                case Keys.Down: _game.MoveDown(); break;
                case Keys.Up: _game.Rotate(); break;
            }
        }

        private Brush GetBrush(int n)
        {
            Brush brush = Brushes.DarkGray;
            switch (n)
            {
                case 0:
                    brush = Brushes.White;
                    break;
                case 1:
                    brush = Brushes.MediumTurquoise;
                    break;
                case 2:
                    brush = Brushes.Yellow;
                    break;
                case 3:
                    brush = Brushes.MediumPurple;
                    break;
                case 4:
                    brush = Brushes.CornflowerBlue;
                    break;
                case 5:
                    brush = Brushes.Orange;
                    break;
                case 6:
                    brush = Brushes.MediumSeaGreen;
                    break;
                case 7:
                    brush = Brushes.MediumVioletRed;
                    break;
                case 8:
                    brush = Brushes.Black;
                    break;
                default: break;
            }
            return brush;
        }

        private void RedrawMap(object sender, EventArgs e)
        {
            labelScore.Text = $"Score: {_game.Score}";
            labelHighScore.Text = $"High Score: {_game.HighScore}";
            Invalidate();
        }

        private void DrawGrid(Graphics g)
        {
            for (int i = 0; i <= Board.HEIGHT; i++)
            {
                g.DrawLine(Pens.Black, new Point(_padding, _padding + i * _squareSize), new Point(_padding + Board.WIDTH * 25, _padding + i * _squareSize));
            }
            for (int i = 0; i <= Board.WIDTH; i++)
            {
                g.DrawLine(Pens.Black, new Point(_padding + i * _squareSize, _padding), new Point(_padding + i * _squareSize, _padding + Board.HEIGHT * 25));
            }
            Pen dashedPen = new Pen(Color.Red, 3);
            dashedPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            g.DrawRectangle(dashedPen, _padding + 3 * _squareSize, _padding, _squareSize * 4, _squareSize * 4);
            g.DrawLine(dashedPen, _padding, _padding + 2 * _squareSize, _padding + 3 * _squareSize, _padding + 2 * _squareSize);
            g.DrawLine(dashedPen, _padding + (Board.WIDTH - 3) * _squareSize, _padding + 2 * _squareSize, _padding + Board.WIDTH * _squareSize, _padding + 2 * _squareSize);
            Brush transparentBrush = new SolidBrush(Color.FromArgb(30, Color.Red));
            g.FillRectangle(transparentBrush, _padding + 3 * _squareSize, _padding, _squareSize * 4, _squareSize * 4);
        }

        private void DrawMap(Graphics g)
        {
            for (int y = 0; y < Board.HEIGHT; y++)
            {
                for (int x = 0; x < Board.WIDTH; x++)
                {
                    if (_board[y, x] == 0) continue;

                    Brush brush = GetBrush(_board[y, x]);

                    int drawX = _padding + x * _squareSize;
                    int drawY = _padding + (Board.HEIGHT - 1 - y) * _squareSize;

                    g.FillRectangle(brush,
                        drawX + 1,
                        drawY + 1,
                        _squareSize - 1,
                        _squareSize - 1);
                }
            }
        }

        private void DrawNextPiece(Graphics g)
        {
            Tetromino piece = _game.NextPiece;
            if (piece == null) return;
            Pen pen = new Pen(Brushes.Black);
            g.DrawRectangle(pen,
                _padding * 2 + Board.WIDTH * _squareSize,
                _padding,
                _squareSize * piece.Size,
                _squareSize * piece.Size);
            for (int row = 0; row < piece.Size; row++)
                for (int col = 0; col < piece.Size; col++)
                {
                    if (piece[row, col] == 0) continue;
                    int drawX = _padding * 2 + Board.WIDTH * _squareSize + col * _squareSize;
                    int drawY = _padding + (piece.Size - 1 - row) * _squareSize;
                    Brush brush = GetBrush(piece[row, col]);
                    g.FillRectangle(brush,
                        drawX + 1,
                        drawY + 1,
                        _squareSize - 1,
                        _squareSize - 1);
                    g.DrawRectangle(pen,
                        drawX,
                        drawY,
                        _squareSize,
                        _squareSize);
                }
        }

        private void DrawCurrentPiece(Graphics g)
        {
            Tetromino piece = _game.CurrentPiece;
            if (piece == null) return;

            for (int row = 0; row < piece.Size; row++)
            {
                for (int col = 0; col < piece.Size; col++)
                {
                    if (piece[row, col] == 0) continue;

                    int boardX = piece.X + col;
                    int boardY = piece.Y + row;

                    if (boardY < 0 || boardY >= Board.HEIGHT) continue;
                    if (boardX < 0 || boardX >= Board.WIDTH) continue;

                    Brush brush = GetBrush(piece[row, col]);

                    int drawX = _padding + boardX * _squareSize;
                    int drawY = _padding + (Board.HEIGHT - 1 - boardY) * _squareSize;

                    g.FillRectangle(brush,
                        drawX + 1,
                        drawY + 1,
                        _squareSize - 1,
                        _squareSize - 1);
                }
            }
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            DrawMap(e.Graphics);
            DrawCurrentPiece(e.Graphics);
            DrawNextPiece(e.Graphics);
            DrawGrid(e.Graphics);
        }

        private void buttonRestart_Click(object sender, EventArgs e)
        {
            _game.Restart();
            _board = _game.Board;
            labelScore.Text = "0";
            this.Focus();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Игровые клавиши обрабатываем здесь
            if (keyData == Keys.Left || keyData == Keys.Right ||
                keyData == Keys.Up || keyData == Keys.Down ||
                keyData == Keys.Space)
            {
                OnKeyDown(null, new KeyEventArgs(keyData));
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
