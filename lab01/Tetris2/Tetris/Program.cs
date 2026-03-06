using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tetris
{

    internal static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);


            var Board = new Board();
            var providers = new IPieceProvider[] { new IProvider(), new OProvider(), new TProvider(), new JProvider(), new LProvider(), new SProvider(), new ZProvider()};
            var BagOfProviders = new BagOfProviders(providers);
            var Game = new Game(Board, BagOfProviders);
            
            Application.Run(new Form1(Game));
        }
    }

}
