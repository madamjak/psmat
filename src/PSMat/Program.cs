using System;
using System.Threading.Tasks;

namespace PSMat
{
    class Program
    {
        private static PisaciAutomat.Program _editor;

        static void Main(string[] args)
        {
            var cestaKSuboru = args != null && args.Length == 1 ? args[0] : null;

            _editor = new PisaciAutomat.Program(cestaKSuboru);
            _editor.NacitajSuborAVykresli();

            Console.TreatControlCAsInput = true;

            while (true)
            {
                var vstup = Console.ReadKey(intercept: true);
                
                _editor.SpracujVstup(vstup);

                if (_editor.Ukonci)
                {
                    Environment.Exit(0);
                }
                
                _editor.Prekresli();
            }
        }
    }
}
