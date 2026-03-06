using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tetris
{
    internal interface IPieceProvider // интерфейс фабрики
    {
        Tetromino Create();
        void Reset();
    }

    internal class IProvider: IPieceProvider
    {
        public Tetromino Create()
        {
            return new IPiece();
        }
        public void Reset() { }
    }

    internal class OProvider: IPieceProvider
    {
        private Random _rnd;
        public Tetromino Create()
        {
            if( _rnd == null ) _rnd = new Random();
            if( _rnd.Next(3) == 0 ) return new OPiece(); // с вероятностью 1\3 возвращаем бомбу
            int[,] shape =
            {
            { 2, 2 },
            { 2, 2 }
            };
            return new Tetromino(shape);
        }
        public void Reset() { }
    }
    internal class TProvider : IPieceProvider
    {
        public Tetromino Create()
        {
            return new TPiece();
        }
        public void Reset() { }
    }
    internal class JProvider : IPieceProvider
    {
        public Tetromino Create()
        {
            return new JPiece();
        }
        public void Reset() { }
    }
    internal class LProvider : IPieceProvider
    {
        public Tetromino Create()
        {
            return new LPiece();
        }
        public void Reset() { }
    }
    internal class SProvider : IPieceProvider
    {
        public Tetromino Create()
        {
            return new SPiece();
        }
        public void Reset() { }
    }
    internal class ZProvider : IPieceProvider
    {
        public Tetromino Create()
        {
            return new ZPiece();
        }
        public void Reset() { }
    }

    internal class BagOfProviders : IPieceProvider
    {
        private IPieceProvider[] _providers;
        private Queue<IPieceProvider> _bag;
        private Random _rnd;
        private int[] _weights;

        public BagOfProviders(IPieceProvider[] providers) // с одинаковыми весами
        {
            _providers = providers;
            _rnd = new Random();
            _weights = new int[providers.Length];
            for (int i = 0; i < providers.Length; i++) _weights[i] = 1;
            RefillBag();
        }

        public BagOfProviders(IPieceProvider[] providers, int[] weights) // с заданными весами
        {
            _providers = providers;
            _rnd = new Random();
            _weights = weights;
            RefillBag();
        }

        public Tetromino Create()
        {
            if (_bag.Count == 0)
                RefillBag();
            return _bag.Dequeue().Create(); // передает создание конкретной фабрике
        }

        public void Reset()
        {
            RefillBag();
        }

        private void RefillBag()
        {
            var order = new List<IPieceProvider> (_providers);
            order = order.OrderBy(x => _rnd.Next()).ToList();
            _bag = new Queue<IPieceProvider>(order);
        }
    }
}
