using System;

namespace BackdropExtension
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application. Note that this will only be called if you change the project properties output type to windows application instead of class library which you can do for testing
        /// </summary>
        static void Main(string[] args)
        {
            using (BackdropTestApp game = new BackdropTestApp())
            {
                game.Run();
            }
        }
    }
}
