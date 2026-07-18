using System;
using System.Collections.Generic;

namespace PisaciStroj.Pamat
{
    internal class PamatOperacii
    {
        private Stack<Operacia> _operacieNaVratenie;
        private Stack<Operacia> _operacieNaZopakovanie;

        public int PocetOperaciiNaVratenie { get; private set; }
        public int PocetOperaciiNaZopakovanie { get; private set; }

        private DateTime? _posledneUlozenie;
        private int _pocetOperaciiOdPoslUlozenia;

        public PamatOperacii()
        {
            _operacieNaVratenie = new Stack<Operacia>();
            _operacieNaZopakovanie = new Stack<Operacia>();
        }
        public void PridajOperaciuNaVratenie(Operacia operacia)
        {
            if (_posledneUlozenie.HasValue)
            {
                _pocetOperaciiOdPoslUlozenia++;
            }

            _operacieNaVratenie.Push(operacia);

            PocetOperaciiNaVratenie++;
        }

        

        public void PridajOperaciuNaZopakovanie(Operacia operacia)
        {
            if (_posledneUlozenie.HasValue)
            {
                _pocetOperaciiOdPoslUlozenia--;
            }

            _operacieNaZopakovanie.Push(operacia);

            PocetOperaciiNaZopakovanie++;
        }

        public Operacia OperaciaNaVratenie()
        {
            var operacia = _operacieNaVratenie.Pop();
            PocetOperaciiNaVratenie--;

            return operacia;
        }

        public Operacia OperaciaNaZopakovanie()
        {
            var operacia = _operacieNaZopakovanie.Pop();
            PocetOperaciiNaZopakovanie--;

            return operacia;
        }

        public void VycistiOperacieNaZopakovanie()
        {
            if (PocetOperaciiNaZopakovanie > 0)
            {
                _operacieNaZopakovanie.Clear();
                PocetOperaciiNaZopakovanie = 0;
            }
        }

        internal void SuborUlozeny()
        {
            _posledneUlozenie = DateTime.Now;
            _pocetOperaciiOdPoslUlozenia = 0;
        }

        internal bool MaZmenu()
        {
            var upravenyNeulozeny = PocetOperaciiNaVratenie > 0 && !_posledneUlozenie.HasValue;
            var zmeneny = _posledneUlozenie.HasValue && _pocetOperaciiOdPoslUlozenia != 0;

            return upravenyNeulozeny || zmeneny;
        }
    }
}
