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
            var PieceMaker = new PieceMaker();
            var Game = new Game(Board, PieceMaker);
            
            Application.Run(new Form1(Game));
        }
    }
}
