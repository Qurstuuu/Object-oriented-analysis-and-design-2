using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tetris
{
    internal class PieceMaker
    {
        Random _rnd;
        Queue<int> _bag;

        public PieceMaker()
        {
            _rnd = new Random();
            RefillBag();
        }
        public PieceMaker(int seed)
        {
            _rnd = new Random(seed);
            RefillBag();
        }

        public void Reset()
        {
            _bag.Clear();
            RefillBag();
        }

        private void RefillBag()
        {
            var order = new List<int> { 0, 1, 2, 3, 4, 5, 6 };
            order = order.OrderBy(x => _rnd.Next()).ToList();
            _bag = new Queue<int>(order);
        }

        private Tetromino CreateI()
        {
            int[,] shape = new int[,]
        {
            { 0, 0, 1, 0 }, 
            { 0, 0, 1, 0 }, 
            { 0, 0, 1, 0 }, 
            { 0, 0, 1, 0 }
        };
            return new Tetromino(shape);
        }

        private Tetromino CreateO()
        {
            int[,] shape = new int[,]
        {
            { 2, 2 },
            { 2, 2 }
        };
            return new Tetromino(shape);
        }

        private Tetromino CreateT()
        {
            int[,] shape = new int[,]
        {
            { 0, 3, 0 },
            { 3, 3, 3 },
            { 0, 0, 0 }
        };
            return new Tetromino(shape);
        }

        private Tetromino CreateJ()
        {
            int[,] shape = new int[,]
        {
            { 4, 0, 0 },
            { 4, 4, 4 },
            { 0, 0, 0 }
        };
            return new Tetromino(shape);
        }

        private Tetromino CreateL()
        {
            int[,] shape = new int[,]
        {
            { 0, 0, 5 },
            { 5, 5, 5 },
            { 0, 0, 0 }
        };
            return new Tetromino(shape);
        }

        private Tetromino CreateS()
        {
            int[,] shape = new int[,]
        {
            { 0, 6, 6 },
            { 6, 6, 0 },
            { 0, 0, 0 }
        };
            return new Tetromino(shape);
        }

        private Tetromino CreateZ()
        {
            int[,] shape = new int[,]
        {
            { 7, 7, 0 },
            { 0, 7, 7 },
            { 0, 0, 0 }
        };
            return new Tetromino(shape);
        }

        public Tetromino Create()
        {
            if(_bag.Count == 0)
            {
                RefillBag();
            }

            int element = _bag.Dequeue();
            switch (element)
            {
                case 0:
                    return CreateI();
                case 1:
                    return CreateO();
                case 2:
                    return CreateT();
                case 3:
                    return CreateJ();
                case 4:
                    return CreateL();
                case 5:
                    return CreateS();
                case 6:
                    return CreateZ();
                default:
                    return new Tetromino();
            }
        }
    }
}
