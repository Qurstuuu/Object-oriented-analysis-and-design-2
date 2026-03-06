using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Tetris
{
    internal class Game
    {
        private Board _board;
        private IPieceProvider _provider;
        private Tetromino _currentPiece;
        private Tetromino _nextPiece;
        private System.Timers.Timer _moveDownTimer;
        private bool _isGameOver;
        private int _score;
        private int _highScore;

        public event EventHandler GameOver;
        public Board Board { get { return _board; } }
        public Timer MoveDownTimer { get { return _moveDownTimer; } }
        public Tetromino CurrentPiece => _currentPiece?.Clone();
        public Tetromino NextPiece => _nextPiece?.Clone();
        public int Score => _score;
        public int HighScore => _highScore;
        public Game(Board board, IPieceProvider provider)
        {
            _board = board;
            _provider = provider;
            _moveDownTimer = new System.Timers.Timer(200); // как часто падает фигура
            _moveDownTimer.Elapsed += OnMoveDown;
            _moveDownTimer.AutoReset = true;
            _isGameOver = false;
            _highScore = 0;
            _score = 0;
        }


        private void OnMoveDown(Object source, ElapsedEventArgs e) // движение фигуры вниз по таймеру
        {
            MoveDown();
        }

        public void StartGame()
        {
            _ = new OPiece();// принудительная инициализация
            _isGameOver = false;
            _nextPiece = _provider.Create();
            SpawnNextPiece();
            _score = 0;
            _moveDownTimer.Enabled = true;
            Console.WriteLine("Game Started");
        }

        public void Restart()
        {
            _board = new Board();
            _provider.Reset();
            StartGame();
        }

        public void MoveDown()
        {
            if (_isGameOver)
            {
                _moveDownTimer.Stop();
                return;
            }
            var moved = _currentPiece.Move(0, -1);
            if (_board.CanPlace(moved))
            {
                _currentPiece = moved;
            }
            else
            {
                FreezePiece();
            }
        }

        private void FreezePiece() // закрепляет фигуру на доске, очищает линии и спавнит новую фигуру
        {
            if (_currentPiece is OPiece oPiece)
            {
                _currentPiece = oPiece.Explosion();
            }
            _board.FreezePiece(_currentPiece);
            int dScore = _board.ClearFullLines();
            _score += dScore * dScore; // чем больше линий за раз, тем больше счет
            _isGameOver = _board.IsGameOver();
            if (_isGameOver)
            {
                GameOver?.Invoke(this, EventArgs.Empty);
                if(_highScore < _score) _highScore = _score;
            }
            SpawnNextPiece();
        }

        private void SpawnNextPiece() // спавн новой фигуры
        {
            int spawnX = (Board.WIDTH - _nextPiece.Size + 1) / 2;
            int spawnY = Board.HEIGHT - _nextPiece.Size;
            _currentPiece = _nextPiece.SetCoords(spawnX, spawnY); // размещаем по центру поля
            _nextPiece = _provider.Create();
        }

        public void MoveLeft()
        {
            if (_isGameOver) return;
            var moved = _currentPiece.Move(-1, 0);
            if (_board.CanPlace(moved))
            {
                _currentPiece = moved;
            }
        }

        public void MoveRight()
        {
            if (_isGameOver) return;
            var moved = _currentPiece.Move(1, 0);
            if (_board.CanPlace(moved))
            {
                _currentPiece = moved;
            }
        }

        public void Rotate()
        {
            if (_isGameOver) return;
            var moved = _currentPiece.Rotate();
            if (_board.CanPlace(moved))
            {
                _currentPiece = moved;
            }
        }
        /* начинает игру
         * берет первую фигуру из фабрики
         * начинает ее опускать с интервалом таймера
         * обновляет таймер, если было выполнено перемещение\вращение
         * когда нажимают кнопку, проверяет, что можно двигать фигуру и двигает ее
         * когда нужно подвинуть фигуру вниз, но нельзя, замораживаем фигуру и спавним новую, проверяем, не закончилась ли игра
         * 
         */
    }
}
